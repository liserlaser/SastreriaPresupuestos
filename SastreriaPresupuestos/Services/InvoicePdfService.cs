using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SastreriaPresupuestos.Export;

namespace SastreriaPresupuestos.Services
{
    public static class InvoicePdfService
    {
        private const string PrimaryColor = "#2F332D";
        private const string AccentColor = "#A8B2A1";
        private const string MutedColor = "#6D7468";
        private const string BorderColor = "#D7DDD2";
        private const string BusinessName = "Martínez Mor Sastrería";
        private const string BusinessPhone = "611 66 27 10";
        private const string BusinessAddress = "Maestro Sosa, 26 Valencia";
        private const string BusinessEmail = "sastreriamartinezmor@gmail.com";

        private static string LogoPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logovectordefA.png");

        public static void ExportInvoiceToPdf(ExportInvoice invoice, string filePath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginTop(44);
                    page.MarginHorizontal(36);
                    page.MarginBottom(36);

                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(PrimaryColor));

                    page.Header().Element(content => ComposeHeader(content, invoice));
                    page.Content().PaddingTop(12).Column(column =>
                    {
                        column.Spacing(18);
                        column.Item().Element(content => ComposeClientBlock(content, invoice));
                        column.Item().Element(content => ComposeItemsTable(content, invoice));
                        column.Item().AlignRight().Text($"Total factura: {invoice.Total:N2} €").FontSize(16).Bold();

                        if (!string.IsNullOrWhiteSpace(invoice.Notes))
                            column.Item().Text(invoice.Notes).FontSize(10).FontColor(MutedColor);
                    });
                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(8).FontColor(MutedColor));
                        text.Span("Página ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf(filePath);
        }

        private static void ComposeHeader(IContainer container, ExportInvoice invoice)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(left =>
                    {
                        if (File.Exists(LogoPath))
                            left.Item().Width(110).Image(LogoPath);

                        left.Item().Text(BusinessName).FontSize(16).Bold();
                        left.Item().Text(BusinessPhone).FontSize(9).FontColor(MutedColor);
                        left.Item().Text(BusinessAddress).FontSize(9).FontColor(MutedColor);
                        left.Item().Text(BusinessEmail).FontSize(9).FontColor(MutedColor);
                    });

                    row.ConstantItem(210).AlignRight().Column(right =>
                    {
                        right.Item().AlignRight().Text("FACTURA").FontSize(20).Bold().FontColor(AccentColor);
                        right.Item().AlignRight().Text(invoice.InvoiceNumber).FontSize(13).Bold();
                        right.Item().AlignRight().Text($"Fecha: {invoice.CreatedAt:dd/MM/yyyy}").FontSize(10).FontColor(MutedColor);
                    });
                });

                column.Item().PaddingTop(6).LineHorizontal(2).LineColor(AccentColor);
            });
        }

        private static void ComposeClientBlock(IContainer container, ExportInvoice invoice)
        {
            container.Border(1).BorderColor(BorderColor).Padding(12).Column(column =>
            {
                column.Item().Text("Cliente").FontSize(11).Bold().FontColor(MutedColor);
                column.Item().Text(invoice.ClientName).FontSize(14).Bold();
                if (!string.IsNullOrWhiteSpace(invoice.ClientPhone))
                    column.Item().Text($"Teléfono: {invoice.ClientPhone}");
                if (!string.IsNullOrWhiteSpace(invoice.ClientDni))
                    column.Item().Text($"DNI/CIF: {invoice.ClientDni}");
            });
        }

        private static void ComposeItemsTable(IContainer container, ExportInvoice invoice)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(4);
                    columns.ConstantColumn(55);
                    columns.ConstantColumn(75);
                    columns.ConstantColumn(80);
                });

                table.Header(header =>
                {
                    header.Cell().Text("Producto").Bold();
                    header.Cell().Text("Descripción").Bold();
                    header.Cell().AlignRight().Text("Cant.").Bold();
                    header.Cell().AlignRight().Text("Precio").Bold();
                    header.Cell().AlignRight().Text("Total").Bold();
                });

                foreach (var item in invoice.Items)
                {
                    table.Cell().Text(item.ProductName);
                    table.Cell().Text(item.Description);
                    table.Cell().AlignRight().Text(item.Quantity.ToString());
                    table.Cell().AlignRight().Text($"{item.UnitPrice:N2} €");
                    table.Cell().AlignRight().Text($"{item.Total:N2} €");
                }
            });
        }
    }
}
