using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SastreriaPresupuestos.Export;
using System;
using System.IO;
using System.Linq;

namespace SastreriaPresupuestos.Services
{
    public static class ClientSheetPdfService
    {
        private const string PrimaryColor = "#2F332D";
        private const string AccentColor = "#A8B2A1";
        private const string SoftBackground = "#F4F6F2";
        private const string BorderColor = "#D7DDD2";
        private const string MutedColor = "#6D7468";

        private const string BusinessName = "Sastrería Martínez Mor";
        private const string BusinessSubtitle = "Ficha interna de cliente";

        private static string LogoPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logovectordefA.png");

        public static void ExportClientSheetToPdf(ExportClientSheet sheet, string filePath)
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
                        x.FontSize(11)
                         .FontColor(PrimaryColor));

                    page.Header()
                        .Element(container => ComposeHeader(container, sheet));

                    page.Content()
                        .PaddingTop(8)
                        .Column(column =>
                        {
                            column.Spacing(16);

                            column.Item()
                                .Element(container => ComposeClientData(container, sheet));

                            column.Item()
                                .Element(container => ComposeOrderData(container, sheet));

                            column.Item()
                                .Element(container => ComposeObservations(container, sheet));

                            column.Item()
                                .Element(ComposeHandwrittenNotes);
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Documento interno · No entregar al cliente")
                        .FontSize(8)
                        .FontColor(MutedColor);
                });
            })
            .GeneratePdf(filePath);
        }

        private static void ComposeHeader(IContainer container, ExportClientSheet sheet)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    if (File.Exists(LogoPath))
                    {
                        row.ConstantItem(104)
                            .Height(112)
                            .Image(LogoPath)
                            .FitArea();

                        row.ConstantItem(18);
                    }

                    row.RelativeItem().Column(left =>
                    {
                        left.Item()
                            .Text(BusinessName)
                            .FontSize(20)
                            .Bold()
                            .FontColor(PrimaryColor);

                        left.Item()
                            .Text(BusinessSubtitle)
                            .FontSize(11)
                            .FontColor(MutedColor);
                    });

                    row.ConstantItem(190)
                        .AlignRight()
                        .Column(right =>
                        {
                            right.Item()
                                .AlignRight()
                                .Text("FICHA INTERNA")
                                .FontSize(16)
                                .Bold()
                                .FontColor(AccentColor);

                            if (!string.IsNullOrWhiteSpace(sheet.DisplayQuoteNumber))
                            {
                                right.Item()
                                    .AlignRight()
                                    .Text(sheet.DisplayQuoteNumber)
                                    .FontSize(9)
                                    .FontColor(MutedColor);
                            }

                            right.Item()
                                .AlignRight()
                                .Text($"Fecha evento: {sheet.EventDate:dd/MM/yyyy}")
                                .FontSize(10);
                        });
                });

                column.Item()
                    .PaddingTop(4)
                    .LineHorizontal(2)
                    .LineColor(AccentColor);
            });
        }

        private static void ComposeClientData(IContainer container, ExportClientSheet sheet)
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

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(left =>
                        {
                            left.Item().Text(text =>
                            {
                                text.Span("Cliente: ").Bold();
                                text.Span(sheet.ClientName);
                            });

                            left.Item().Text(text =>
                            {
                                text.Span("DNI: ").Bold();
                                text.Span(sheet.ClientDni);
                            });
                        });

                        row.RelativeItem().Column(right =>
                        {
                            right.Item().Text(text =>
                            {
                                text.Span("Teléfono: ").Bold();
                                text.Span(FormatPhone(sheet.ClientPhone));
                            });

                            right.Item().Text(text =>
                            {
                                text.Span("Fecha de evento: ").Bold();
                                text.Span($"{sheet.EventDate:dd/MM/yyyy}");
                            });
                        });
                    });
                });
        }

        private static void ComposeOrderData(IContainer container, ExportClientSheet sheet)
        {
            container
                .Border(1)
                .BorderColor(BorderColor)
                .Padding(14)
                .Column(column =>
                {
                    column.Spacing(10);

                    column.Item()
                        .Text("ENCARGO")
                        .FontSize(11)
                        .Bold()
                        .FontColor(AccentColor);

                    column.Item().Text(text =>
                    {
                        text.Span("Resumen / título: ").Bold();
                        text.Span(sheet.DisplayOrderTitle);
                    });

                    column.Item().Text(text =>
                    {
                        text.Span("A cuenta: ").Bold();
                        text.Span($"{sheet.Deposit:N2} €");
                    });
                });
        }

        private static void ComposeObservations(IContainer container, ExportClientSheet sheet)
        {
            container
                .Border(1)
                .BorderColor(BorderColor)
                .Padding(14)
                .MinHeight(95)
                .Column(column =>
                {
                    column.Spacing(8);

                    column.Item()
                        .Text("OBSERVACIONES")
                        .FontSize(11)
                        .Bold()
                        .FontColor(AccentColor);

                    column.Item()
                        .Text(string.IsNullOrWhiteSpace(sheet.Observations) ? " " : sheet.Observations)
                        .FontSize(10);
                });
        }

        private static void ComposeHandwrittenNotes(IContainer container)
        {
            container
                .Border(1)
                .BorderColor(BorderColor)
                .Padding(14)
                .Column(column =>
                {
                    column.Spacing(12);

                    column.Item()
                        .Text("NOTAS A MANO")
                        .FontSize(11)
                        .Bold()
                        .FontColor(AccentColor);

                    for (int i = 0; i < 9; i++)
                    {
                        column.Item()
                            .PaddingTop(8)
                            .LineHorizontal(1)
                            .LineColor(BorderColor);
                    }
                });
        }

        private static string FormatPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return "";

            var digits = new string(phone.Where(char.IsDigit).ToArray());

            if (digits.Length == 9)
                return $"{digits[..3]} {digits.Substring(3, 2)} {digits.Substring(5, 2)} {digits.Substring(7, 2)}";

            return phone.Trim();
        }
    }
}