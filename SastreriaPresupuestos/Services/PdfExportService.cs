using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SastreriaPresupuestos.Export;
using System.IO;
using System;
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

        private const string BusinessName = "Sastrería Martínez Mor";
        private const string BusinessSubtitle = "Sastrería a medida";
        private const string BusinessPhone = "611 66 27 10";
        private const string BusinessAddress = "Calle Maestro Sosa, 26 Valencia";
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
                    page.Margin(36);

                    page.DefaultTextStyle(x =>
                        x.FontSize(10)
                         .FontColor(PrimaryColor));

                    page.Header()
                        .Element(container => ComposeHeader(container, quote));

                    page.Content()
                        .PaddingVertical(24)
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
                        .Element(ComposeFooter);
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
                        page.Margin(36);

                        page.DefaultTextStyle(x =>
                            x.FontSize(10)
                             .FontColor(PrimaryColor));

                        page.Header()
                            .Element(container => ComposeHeader(container, quote));

                        page.Content()
                            .PaddingVertical(24)
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
                            .Element(ComposeFooter);
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
                        row.ConstantItem(80)
                            .Height(120)
                            .Image(LogoPath)
                            .FitArea();

                        row.ConstantItem(18);
                    }

                    row.RelativeItem().Column(left =>
                    {
                        left.Item()
                            .Text(BusinessName)
                            .FontSize(24)
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

                            right.Item()
                                .AlignRight()
                                .Text($"Entrega: {quote.DeliveryDate:dd/MM/yyyy}")
                                .FontSize(10);
                        });
                });

                column.Item()
                    .PaddingTop(14)
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
                                text.Span(quote.ClientPhone);
                            });
                        });

                        row.RelativeItem().Column(right =>
                        {
                            right.Item().Text(text =>
                            {
                                text.Span("Fecha de entrega: ").Bold();
                                text.Span($"{quote.DeliveryDate:dd/MM/yyyy}");
                            });

                            var status = string.IsNullOrWhiteSpace(quote.Status)
                                ? "Pendiente"
                                : quote.Status;

                            if (status != "Pendiente")
                            {
                                right.Item().Text(text =>
                                {
                                    text.Span("Estado: ").Bold();
                                    text.Span(status);
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
                    header.Cell().Element(HeaderCell).Text("Descripción");
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
                                row.RelativeItem().Text("Señal");
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
    }
}