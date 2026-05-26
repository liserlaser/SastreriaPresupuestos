using System;
using System.IO;
using System.Text.RegularExpressions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SastreriaPresupuestos.Export;
using SastreriaPresupuestos.Models;

namespace SastreriaPresupuestos.Services
{
    public static class InvoicePdfService
    {
        private const string PrimaryColor = "#2F332D";
        private const string AccentColor = "#A8B2A1";
        private const string SoftBackground = "#F4F6F2";
        private const string BorderColor = "#D7DDD2";
        private const string MutedColor = "#6D7468";

        private const string BusinessName = "Martínez Mor S.C.";
        private const string BusinessTaxId = "J16389348";
        private const string BusinessAddress = "Maestro Sosa 26";
        private const string BusinessCity = "46007 Valencia";
        private const string BusinessPhone = "611 66 27 10";
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

                    page.DefaultTextStyle(x =>
                        x.FontSize(10)
                         .FontColor(PrimaryColor));

                    page.Header()
                        .Element(container => ComposeHeader(container, invoice));

                    page.Content()
                        .PaddingTop(invoice.IsCompanyInvoice ? 48 : 20) // Más espacio vertical antes del contenido
                        .PaddingBottom(18)
                        .Column(column =>
                        {
                            column.Spacing(36); // Distancia duplicada entre bloques (datos de cliente, tabla, totales)

                            if (!invoice.IsCompanyInvoice)
                            {
                                column.Item()
                                    .Element(container => ComposeSimplifiedClientBlock(container, invoice));
                            }

                            column.Item()
                                .Element(container => ComposeItemsTable(container, invoice));

                            column.Item()
                                .Element(container => ComposeTotals(container, invoice));
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.DefaultTextStyle(x => x.FontSize(8).FontColor(MutedColor));

                            text.Span("Página ");
                            text.CurrentPageNumber();
                            text.Span(" de ");
                            text.TotalPages();
                        });
                });
            })
            .GeneratePdf(filePath);
        }

        private static void ComposeHeader(IContainer container, ExportInvoice invoice)
        {
            if (invoice.IsCompanyInvoice)
            {
                ComposeCompanyHeader(container, invoice);
                return;
            }

            ComposeSimplifiedHeader(container, invoice);
        }

        private static void ComposeSimplifiedHeader(IContainer container, ExportInvoice invoice)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    if (File.Exists(LogoPath))
                    {
                        row.ConstantItem(130) // Logotipo un 25% más grande (de 104 a 130)
                            .Height(140)      // Logotipo un 25% más grande (de 112 a 140)
                            .Image(LogoPath)
                            .FitArea();

                        row.ConstantItem(24);
                    }

                    row.RelativeItem().Element(ComposeBusinessData);

                    row.ConstantItem(190)
                        .AlignRight()
                        .Column(right => ComposeDocumentMeta(right, "Factura simplificada", invoice));
                });

                column.Item()
                    .PaddingTop(4) // Mayor separación con la línea divisoria
                    .LineHorizontal(2)
                    .LineColor(AccentColor);
            });
        }

        private static void ComposeCompanyHeader(IContainer container, ExportInvoice invoice)
        {
            container.Column(column =>
            {
                column.Item()
                    .AlignCenter()
                    .PaddingBottom(14) // Separación del encabezado "Factura"
                    .Text("Factura")
                    .FontSize(22)
                    .Bold()
                    .FontColor(AccentColor);

                column.Item()
                    .AlignCenter()
                    .PaddingBottom(16) // Separación inferior de los metadatos de cabecera
                    .Text($"Fecha: {invoice.CreatedAt:dd/MM/yyyy} Hora: {invoice.CreatedAt:HH:mm}                Nº: {invoice.InvoiceNumber}") // Mayor separación lateral
                    .FontSize(10)
                    .FontColor(MutedColor);

                column.Item().PaddingTop(12).Row(row =>
                {
                    if (File.Exists(LogoPath))
                    {
                        row.ConstantItem(90) // Logotipo un 25% más grande (de 72 a 90)
                            .Height(98)      // Logotipo un 25% más grande (de 78 a 98)
                            .Image(LogoPath)
                            .FitArea();

                        row.ConstantItem(18);
                    }

                    row.RelativeItem(1.0f).Element(ComposeCompactBusinessData);

                    row.ConstantItem(1)
                        .Height(82)
                        .Background(BorderColor);

                    row.RelativeItem(1.4f) // Más proporción de ancho a la derecha para empujar el bloque
                        .PaddingLeft(60)   // Más padding izquierdo para desplazar datos del cliente hacia la derecha
                        .Column(client =>
                        {
                            var clientAddress = SplitClientAddress(invoice.ClientAddress);
                            client.Item().Text("Cliente").FontSize(12).Bold().FontColor(AccentColor);
                            client.Item().Text(invoice.ClientName).FontSize(11).Bold().FontColor(PrimaryColor);
                            client.Item().Text(invoice.ClientTaxId).FontSize(9).FontColor(MutedColor);
                            client.Item().Text(clientAddress.Street).FontSize(9).FontColor(MutedColor);

                            if (!string.IsNullOrWhiteSpace(clientAddress.City))
                                client.Item().Text(clientAddress.City).FontSize(9).FontColor(MutedColor);
                        });
                });

                column.Item()
                    .PaddingTop(4) // Mayor separación con la línea divisoria
                    .LineHorizontal(2)
                    .LineColor(AccentColor);
            });
        }

        private static (string Street, string City) SplitClientAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return ("", "");

            var normalized = Regex.Replace(address.Trim(), @"\s+", " ");
            var postalCodeMatch = Regex.Match(normalized, @"\b\d{5}\b");

            if (!postalCodeMatch.Success)
                return (normalized, "");

            var street = normalized[..postalCodeMatch.Index].Trim().Trim(',', ';', '-');
            var city = normalized[postalCodeMatch.Index..].Trim().Trim(',', ';', '-');

            return (street, city);
        }

        private static void ComposeBusinessData(IContainer container)
        {
            container.Column(left =>
            {
                left.Item()
                    .Text(BusinessName)
                    .FontSize(22)
                    .Bold()
                    .FontColor(PrimaryColor);

                left.Item().Text(BusinessTaxId).FontSize(9).FontColor(MutedColor);
                left.Item().Text(BusinessAddress).FontSize(9).FontColor(MutedColor);
                left.Item().Text(BusinessCity).FontSize(9).FontColor(MutedColor);
                left.Item().Text(BusinessPhone).FontSize(9).FontColor(MutedColor);
                left.Item().Text(BusinessEmail).FontSize(9).FontColor(MutedColor);
            });
        }

        private static void ComposeCompactBusinessData(IContainer container)
        {
            container.Column(left =>
            {
                left.Item()
                    .Text(BusinessName)
                    .FontSize(13)
                    .Bold()
                    .FontColor(PrimaryColor);

                left.Item().Text(BusinessTaxId).FontSize(8.5f).FontColor(MutedColor);
                left.Item().Text(BusinessAddress).FontSize(8.5f).FontColor(MutedColor);
                left.Item().Text(BusinessCity).FontSize(8.5f).FontColor(MutedColor);
                left.Item().Text(BusinessPhone).FontSize(8.5f).FontColor(MutedColor);
                left.Item().Text(BusinessEmail).FontSize(8.5f).FontColor(MutedColor);
            });
        }

        private static void ComposeDocumentMeta(ColumnDescriptor right, string title, ExportInvoice invoice)
        {
            right.Spacing(4); // Espaciado vertical entre líneas de metadatos

            right.Item()
                .AlignRight()
                .Text(title)
                .FontSize(16)
                .Bold()
                .FontColor(AccentColor);

            right.Item()
                .AlignRight()
                .Text(invoice.InvoiceNumber)
                .FontSize(13)
                .Bold()
                .FontColor(PrimaryColor);

            right.Item()
                .AlignRight()
                .Text($"Emisión: {invoice.CreatedAt:dd/MM/yyyy HH:mm}")
                .FontSize(10)
                .FontColor(MutedColor);
        }

        private static void ComposeSimplifiedClientBlock(IContainer container, ExportInvoice invoice)
        {
            container
                .Background(SoftBackground)
                .Border(1)
                .BorderColor(BorderColor)
                .Padding(14)
                .Column(column =>
                {
                    column.Spacing(8);

                    column.Item()
                        .Text("DATOS DEL CLIENTE")
                        .FontSize(11)
                        .Bold()
                        .FontColor(AccentColor);

                    column.Item().Text(text =>
                    {
                        text.Span("Cliente: ").Bold();
                        text.Span(invoice.ClientName);
                    });
                });
        }

        private static void ComposeItemsTable(IContainer container, ExportInvoice invoice)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2.1f);
                    columns.RelativeColumn(3.0f);
                    columns.ConstantColumn(48);
                    columns.ConstantColumn(78);

                    if (invoice.IsCompanyInvoice)
                        columns.ConstantColumn(48);

                    columns.ConstantColumn(86);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Producto");
                    header.Cell().Element(HeaderCell).Text("Tipo / Detalle");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Cant.");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Precio");

                    if (invoice.IsCompanyInvoice)
                        header.Cell().Element(HeaderCell).AlignRight().Text("IVA");

                    header.Cell().Element(HeaderCell).AlignRight().Text(invoice.IsCompanyInvoice ? "Total Neto" : "Total");
                });

                foreach (var item in invoice.Items)
                {
                    table.Cell().Element(BodyCell).Text(item.ProductName);
                    table.Cell().Element(BodyCell).Text(item.Description);
                    table.Cell().Element(BodyCell).AlignRight().Text(item.Quantity.ToString());
                    table.Cell().Element(BodyCell).AlignRight().Text($"{item.UnitPrice:N2} €");

                    if (invoice.IsCompanyInvoice)
                        table.Cell().Element(BodyCell).AlignRight().Text("21%");

                    table.Cell().Element(BodyCell).AlignRight().Text($"{item.Total:N2} €");
                }
            });
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container
                .Background(PrimaryColor)
                .PaddingVertical(7)
                .PaddingHorizontal(8)
                .DefaultTextStyle(x =>
                    x.Bold()
                     .FontColor(Colors.White)
                     .FontSize(10));
        }

        private static IContainer BodyCell(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(BorderColor)
                .PaddingVertical(8)
                .PaddingHorizontal(8);
        }

        private static void ComposeTotals(IContainer container, ExportInvoice invoice)
        {
            container.AlignRight().Width(280).Column(column =>
            {
                column.Spacing(6);

                column.Item()
                    .Background(SoftBackground)
                    .Border(1)
                    .BorderColor(BorderColor)
                    .Padding(12)
                    .Column(totalBox =>
                    {
                        totalBox.Spacing(6);

                        totalBox.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Base Imponible");
                            row.ConstantItem(120)
                                .AlignRight()
                                .Text($"{invoice.TaxBase:N2} €");
                        });

                        totalBox.Item().Row(row =>
                        {
                            row.RelativeItem().Text("IVA 21%");
                            row.ConstantItem(120)
                                .AlignRight()
                                .Text($"{invoice.VatAmount:N2} €");
                        });

                        totalBox.Item().LineHorizontal(1).LineColor(BorderColor);

                        totalBox.Item().Row(row =>
                        {
                            row.RelativeItem()
                                .Text(invoice.IsCompanyInvoice ? "TOTAL FACTURA" : "TOTAL A PAGAR")
                                .Bold();
                            row.ConstantItem(120)
                                .AlignRight()
                                .Text($"{invoice.Total:N2} €")
                                .FontSize(invoice.IsCompanyInvoice ? 17 : 15)
                                .Bold()
                                .FontColor(AccentColor);
                        });
                    });
            });
        }
    }
}