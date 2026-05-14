using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SastreriaPresupuestos.Export;
using System.IO;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SastreriaPresupuestos.Services
{
    public static class PdfExportService
    {
        private const string PrimaryColor = "#2F332D";
        private const string AccentColor = "#A8B2A1";
        private const string SoftBackground = "#F4F6F2";
        private const string BorderColor = "#D7DDD2";
        private const string MutedColor = "#6D7468";

        private const string BusinessName = "Martínez Mor Sastrería";
        private const string BusinessSubtitle = "Sastrería a medida";
        private const string BusinessPhone = "611 66 27 10";
        private const string BusinessAddress = "Maestro Sosa, 26 Valencia";
        private const string BusinessEmail = "sastreriamartinezmor@gmail.com";
        private const string BusinessFooter = "Gracias por confiar en nuestra sastrería";

        private static string LogoPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logovectordefA.png");

        public static void ExportQuoteToPdf(ExportQuote quote, string filePath)
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
                        .Element(container => ComposeHeader(container, quote));

                    page.Content()                        
                        .PaddingTop(4)
                        .PaddingBottom(18)
                        .Column(column =>
                        {
                            column.Spacing(18);

                            column.Item()
                                .Element(container => ComposeClientBlock(container, quote));

                            column.Item()
                               .Element(container => ComposeItemsTable(container, quote));

                            if (!string.IsNullOrWhiteSpace(quote.ClientNotes))
                            {
                                column.Item()
                                    .Element(container => ComposeClientNotes(container, quote));
                            }

                            column.Item()
                                .Element(container => ComposeTotals(container, quote));
                        });

                    page.Footer()
                        .Column(column =>
                        {
                            column.Spacing(3);

                            column.Item()
                                .AlignCenter()
                                .Text("IVA incluido en todos los precios")
                                .FontSize(9)
                                .FontColor(MutedColor);

                            column.Item()
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
                });
            })
            .GeneratePdf(filePath);
        }

        public static void ExportQuotesToPdf(List<ExportQuote> quotes, string filePath)
        {
            Document.Create(container =>
            {
                foreach (var quote in quotes)
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
                            .Element(container => ComposeHeader(container, quote));

                        page.Content()
                            .PaddingTop(4)
                            .PaddingBottom(18)
                            .Column(column =>
                            {
                                column.Spacing(18);

                                column.Item()
                                    .Element(container => ComposeClientBlock(container, quote));

                                column.Item()
                                    .Element(container => ComposeItemsTable(container, quote));

                                if (!string.IsNullOrWhiteSpace(quote.ClientNotes))
                                {
                                    column.Item()
                                        .Element(container => ComposeClientNotes(container, quote));
                                }

                                column.Item()
                                    .Element(container => ComposeTotals(container, quote));
                            });

                        page.Footer()
                        .Column(column =>
                        {
                            column.Spacing(3);

                            column.Item()
                                .AlignCenter()
                                .Text("IVA incluido en todos los precios")
                                .FontSize(9)
                                .FontColor(MutedColor);

                            column.Item()
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
                    });
                }
            })
            .GeneratePdf(filePath);
        }

        private static void ComposeHeader(IContainer container, ExportQuote quote)
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
                            .FontSize(22)
                            .Bold()
                            .FontColor(PrimaryColor);

                        left.Item()
                            .Text(BusinessSubtitle)
                            .FontSize(11)
                            .FontColor(MutedColor);

                        left.Item()
                            .Text($"{BusinessPhone}")
                            .FontSize(9)
                            .FontColor(MutedColor);

                        left.Item()
                            .Text($"{BusinessAddress}")
                            .FontSize(9)
                            .FontColor(MutedColor);
                    });

                    row.ConstantItem(170)
                        .AlignRight()
                        .Column(right =>
                        {
                            right.Item()
                                .AlignRight()
                                .Text(quote.DisplayTitle)
                                .FontSize(16)
                                .Bold()
                                .FontColor(AccentColor);

                            if (!string.IsNullOrWhiteSpace(quote.DisplayQuoteNumber))
                            {
                                right.Item()
                                    .AlignRight()
                                    .Text(quote.DisplayQuoteNumber)
                                    .FontSize(9)
                                    .FontColor(MutedColor);
                            }

                            if (quote.EventDate != null)
                            {
                                right.Item()
                                    .AlignRight()
                                    .Text($"Fecha evento: {quote.EventDate.Value:dd/MM/yyyy}")
                                    .FontSize(10)
                                    .FontColor(MutedColor);
                            }
                        });
                });

                column.Item()
                    .PaddingTop(4)
                    .LineHorizontal(2)
                    .LineColor(AccentColor);
            });
        }

        private static void ComposeClientBlock(IContainer container, ExportQuote quote)
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
                                text.Span(quote.ClientName);
                            });

                            left.Item().Text(text =>
                            {
                                text.Span("Teléfono: ").Bold();
                                text.Span(FormatPhone(quote.ClientPhone));
                            });
                        });

                        row.RelativeItem().Column(right =>
                        {
                            if (quote.EventDate != null)
                            {
                                right.Item().Text(text =>
                                {
                                    text.Span("Fecha evento: ").Bold();
                                    text.Span($"{quote.EventDate.Value:dd/MM/yyyy}");
                                });
                            }

                        });
                    });
                });
        }

        private static void ComposeItemsTable(IContainer container, ExportQuote quote)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2.1f);
                    columns.RelativeColumn(3.2f);
                    columns.ConstantColumn(55);
                    columns.ConstantColumn(95);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Producto");
                    header.Cell().Element(HeaderCell).Text("Tipo / Detalle");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Cant.");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Precio");
                });

                foreach (var item in quote.Items)
                {
                    if (item.IsFabricLine)
                    {
                        table.Cell()
                            .Element(FabricCell)
                            .Text($"  {item.Product}")
                            .FontSize(9)
                            .FontColor(MutedColor);

                        table.Cell()
                            .Element(FabricCell)
                            .Text(item.Description)
                            .FontSize(9)
                            .FontColor(MutedColor);

                        table.Cell()
                            .Element(FabricCell)
                            .AlignRight()
                            .Text("");

                        table.Cell()
                            .Element(FabricCell)
                            .AlignRight()
                            .Text($"{item.Price:N2} €")
                            .FontSize(9)
                            .FontColor(MutedColor);
                    }
                    else
                    {
                        table.Cell().Element(BodyCell).Text(item.Product);
                        table.Cell().Element(BodyCell).Text(item.Description);
                        table.Cell().Element(BodyCell).AlignRight().Text(item.Quantity.ToString());
                        table.Cell().Element(BodyCell).AlignRight().Text($"{item.Price:N2} €");
                    }
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

        private static IContainer FabricCell(IContainer container)
{
    return container
        .BorderBottom(1)
        .BorderColor(BorderColor)
        .PaddingVertical(5)
        .PaddingHorizontal(8)
        .Background(SoftBackground);
}

        private static void ComposeTotals(IContainer container, ExportQuote quote)
        {
            container.AlignRight().Width(240).Column(column =>
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
                            row.RelativeItem().Text("TOTAL").Bold();
                            row.ConstantItem(110)
                                .AlignRight()
                                .Text($"{quote.Total:N2} €")
                                .FontSize(15)
                                .Bold()
                                .FontColor(AccentColor);
                        });

                        if (ShouldShowPaymentInfo(quote))
                        {
                            totalBox.Item().LineHorizontal(1).LineColor(BorderColor);

                            totalBox.Item().Row(row =>
                            {
                                row.RelativeItem().Text("A Cuenta");
                                row.ConstantItem(110)
                                    .AlignRight()
                                    .Text($"{quote.Deposit:N2} €");
                            });

                            totalBox.Item().Row(row =>
                            {
                                row.RelativeItem().Text("Pendiente").Bold();
                                row.ConstantItem(110)
                                    .AlignRight()
                                    .Text($"{quote.PendingAmount:N2} €")
                                    .Bold();
                            });
                        }
                    });
            });
        }

        private static bool ShouldShowPaymentInfo(ExportQuote quote)
        {
            var status = quote.Status?.Trim().ToLower() ?? "";

            if (status == "rechazado")
                return false;

            if (status == "aceptado" || status == "entregado")
                return true;

            return quote.Deposit > 0;
        }

        private static void ComposeFooter(IContainer container)
        {
            container.Column(column =>
            {
                column.Item()
                    .LineHorizontal(1)
                    .LineColor(BorderColor);

                column.Item()
                    .PaddingTop(8)
                    .AlignCenter()
                    .Text(BusinessFooter)
                    .FontSize(9)
                    .FontColor(MutedColor);
            });
        }

        private static void ComposeClientNotes(IContainer container, ExportQuote quote)
        {
            container
                .Background(SoftBackground)
                .Border(1)
                .BorderColor(BorderColor)
                .Padding(12)
                .Column(column =>
                {
                    column.Spacing(6);

                    column.Item()
                        .Text("OBSERVACIONES")
                        .FontSize(10)
                        .Bold()
                        .FontColor(AccentColor);

                    column.Item()
                        .Text(quote.ClientNotes)
                        .FontSize(10)
                        .FontColor(PrimaryColor);
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