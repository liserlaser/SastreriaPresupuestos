using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SastreriaPresupuestos.ViewModels;
using System;
using System.Globalization;
using System.IO;

namespace SastreriaPresupuestos.Services
{
    public static class ReportPdfService
    {
        private const string Primary = "#2F332D";
        private const string Muted = "#6D7468";
        private const string Border = "#D7DDD2";
        private const string Soft = "#F4F6F2";

        public static void Export(ReportSummary report, string filePath)
        {
            var es = CultureInfo.GetCultureInfo("es-ES");
            Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(34);
                    page.DefaultTextStyle(style => style.FontSize(9).FontColor(Primary));

                    page.Header().Column(column =>
                    {
                        column.Item().Text("MARTÍNEZ MOR SASTRERÍA").FontSize(10).SemiBold().FontColor(Muted);
                        column.Item().PaddingTop(5).Text("Informe de actividad").FontSize(22).Bold();
                        column.Item().PaddingTop(3).Text(report.PeriodLabel).FontSize(11).FontColor(Muted);
                        column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Border);
                    });

                    page.Content().PaddingTop(18).Column(column =>
                    {
                        column.Spacing(14);

                        column.Item().Text("Presupuestos creados en el periodo").FontSize(13).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            void HeaderCell(string text) => table.Cell().Background(Soft).Padding(7).Text(text).SemiBold();
                            HeaderCell("Estado"); HeaderCell("Cantidad"); HeaderCell("Porcentaje");
                            StatusRow("Total", report.QuotesCreated, "100 %");
                            StatusRow("Aceptados", report.AcceptedQuotes, report.AcceptedPercentage);
                            StatusRow("Pendientes", report.PendingQuotes, report.PendingPercentage);
                            StatusRow("Rechazados", report.RejectedQuotes, report.RejectedPercentage);

                            void StatusRow(string label, int count, string percentage)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Border).Padding(7).Text(label);
                                table.Cell().BorderBottom(1).BorderColor(Border).Padding(7).Text(count.ToString("N0", es));
                                table.Cell().BorderBottom(1).BorderColor(Border).Padding(7).Text(percentage);
                            }
                        });
                        column.Item().Text("Los presupuestos pendientes cuya fecha de evento ya ha pasado se consideran rechazados.")
                            .FontSize(8).FontColor(Muted);

                        column.Item().Text("Actividad económica y entregas").FontSize(13).Bold();
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                            });
                            MetricRow("Entregas confirmadas", report.ConfirmedDeliveries.ToString("N0", es));
                            MetricRow("Pendientes de respuesta con evento en el periodo", report.PendingDeliveries.ToString("N0", es));
                            MetricRow("Valor contratado por fecha del evento", report.ContractedTotal.ToString("N2", es) + " €");
                            MetricRow("Anticipos registrados en el periodo", report.DepositsReceived.ToString("N2", es) + " €");
                            MetricRow("Pendiente de cobro de los trabajos del periodo", report.OutstandingTotal.ToString("N2", es) + " €");

                            void MetricRow(string label, string value)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Border).Padding(7).Text(label);
                                table.Cell().BorderBottom(1).BorderColor(Border).Padding(7).AlignRight().Text(value).SemiBold();
                            }
                        });

                        if (report.HasUnattributedLegacyDeposits)
                        {
                            column.Item().Text("Nota: hay anticipos antiguos sin movimientos fechados; no se pueden atribuir a este periodo.")
                                .FontSize(8).FontColor(Muted);
                        }

                        column.Item().Text("Trabajos previstos en el periodo").FontSize(13).Bold();
                        if (report.Deliveries.Count == 0)
                        {
                            column.Item().Text("No hay trabajos previstos.").FontColor(Muted);
                        }
                        else
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(62);
                                    columns.RelativeColumn(1.1f);
                                    columns.RelativeColumn(1.4f);
                                    columns.ConstantColumn(70);
                                    columns.ConstantColumn(72);
                                    columns.ConstantColumn(78);
                                });

                                Cell("Fecha", true); Cell("Cliente", true); Cell("Trabajo", true); Cell("Estado", true); Cell("Total", true); Cell("Pendiente", true);
                                foreach (var item in report.Deliveries)
                                {
                                    Cell(item.EventDate.ToString("dd/MM/yy", es));
                                    Cell(item.ClientName);
                                    Cell(item.Title);
                                    Cell(item.Status);
                                    Cell(item.Total.ToString("N2", es) + " €");
                                    Cell(item.Balance.ToString("N2", es) + " €");
                                }

                                void Cell(string text, bool header = false)
                                {
                                    var cell = table.Cell().BorderBottom(1).BorderColor(Border).Padding(5);
                                    if (header)
                                        cell.Background(Soft).Text(text).SemiBold();
                                    else
                                        cell.Text(text);
                                }
                            });
                        }
                    });

                    page.Footer().Row(row =>
                    {
                        row.RelativeItem().Text($"Generado el {report.GeneratedAt:dd/MM/yyyy HH:mm}").FontSize(8).FontColor(Muted);
                        row.ConstantItem(100).AlignRight().Text(text =>
                        {
                            text.Span("Página ").FontSize(8).FontColor(Muted);
                            text.CurrentPageNumber().FontSize(8).FontColor(Muted);
                            text.Span(" / ").FontSize(8).FontColor(Muted);
                            text.TotalPages().FontSize(8).FontColor(Muted);
                        });
                    });
                });
            }).GeneratePdf(filePath);
        }
    }
}
