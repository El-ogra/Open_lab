using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Open_lab.Models;
using Open_lab.ViewModels;

namespace Open_lab.Services
{
    public class PrintService : IPrintService
    {
        private const string PdfPrinterName = "Microsoft Print to PDF";

        public Task PrintReceiptAsync(ReceiptData data, string? barcodeText = null)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var document = CreateDocument("إيصال مختبر", 12);
            document.Blocks.Add(CreateHeader($"إيصال زيارة #{data.Visit.VisitId}"));
            document.Blocks.Add(new Paragraph(new Run($"المريض: {data.Patient.FullName} | Lab ID: {data.Patient.LabId} | التاريخ: {data.Visit.VisitDate:yyyy-MM-dd HH:mm}")));
            if (!string.IsNullOrWhiteSpace(barcodeText))
            {
                document.Blocks.Add(new Paragraph(new Run($"Barcode: {barcodeText}")));
            }

            document.Blocks.Add(new Paragraph(new Run("التحاليل:")) { FontWeight = FontWeights.Bold });
            foreach (var item in data.VisitTests)
            {
                var testName = item.Test?.NameReceipt ?? item.Test?.NameReport ?? $"Test#{item.TestId}";
                document.Blocks.Add(new Paragraph(new Run($"- {testName} | السعر: {item.Price:N2} | الحالة: {item.Status}")));
            }

            if (data.Invoice != null)
            {
                document.Blocks.Add(new Paragraph(new Run($"الإجمالي: {data.Invoice.Total:N2} | الخصم: {data.Invoice.Discount:N2} | الصافي: {data.Invoice.NetTotal:N2}")));
                document.Blocks.Add(new Paragraph(new Run($"المدفوع: {data.Invoice.Paid:N2} | المتبقي: {data.Invoice.Balance:N2} | الحالة: {data.Invoice.Status}")));
            }

            PrintDocument(document, $"Receipt_{data.Visit.VisitId}");
            return Task.CompletedTask;
        }

        public Task PrintVisitReportAsync(VisitReportData report, bool isReprint = false)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            var title = isReprint ? "تقرير تحاليل (إعادة طباعة)" : "تقرير تحاليل";
            var document = CreateDocument(title, 12);
            document.Blocks.Add(CreateHeader(title));
            document.Blocks.Add(new Paragraph(new Run($"الزيارة: #{report.Visit.VisitId} | التاريخ: {report.Visit.VisitDate:yyyy-MM-dd HH:mm}")));
            document.Blocks.Add(new Paragraph(new Run($"المريض: {report.Patient.FullName} | Lab ID: {report.Patient.LabId}")));

            foreach (var test in report.Tests)
            {
                document.Blocks.Add(new Paragraph(new Run($"{test.Test.NameReport}") { FontWeight = FontWeights.Bold }));
                if (test.Results.Count == 0)
                {
                    document.Blocks.Add(new Paragraph(new Run("- لا توجد نتائج مسجلة.")));
                    continue;
                }

                foreach (var result in test.Results)
                {
                    document.Blocks.Add(new Paragraph(new Run($"- {result.Parameter.Name}: {result.Value} {result.Flag}")));
                }
            }

            PrintDocument(document, $"VisitReport_{report.Visit.VisitId}");
            return Task.CompletedTask;
        }

        public Task PrintPatientHistoryAsync(PatientHistoryReportData history)
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }

            var document = CreateDocument("التاريخ المرضي", 12);
            document.Blocks.Add(CreateHeader("التاريخ المرضي"));
            document.Blocks.Add(new Paragraph(new Run($"المريض: {history.Patient.FullName} | Lab ID: {history.Patient.LabId}")));

            if (history.Visits.Count == 0)
            {
                document.Blocks.Add(new Paragraph(new Run("لا توجد زيارات ضمن الفترة المحددة.")));
            }
            else
            {
                foreach (var visit in history.Visits.OrderByDescending(v => v.Visit.VisitDate))
                {
                    document.Blocks.Add(new Paragraph(new Run($"زيارة #{visit.Visit.VisitId} - {visit.Visit.VisitDate:yyyy-MM-dd HH:mm}") { FontWeight = FontWeights.Bold }));
                    foreach (var test in visit.Tests)
                    {
                        document.Blocks.Add(new Paragraph(new Run($"- {test.Test.NameReport} ({test.VisitTest.Status})")));
                    }
                }
            }

            PrintDocument(document, $"PatientHistory_{history.Patient.LabId}");
            return Task.CompletedTask;
        }

        public Task PrintWorksheetByPatientAsync(DateTime from, DateTime to, IReadOnlyCollection<WorkSheetPatientRow> rows)
        {
            var document = CreateDocument("Work Sheet By Patient", 12);
            document.Blocks.Add(CreateHeader("ورقة العمل - حسب المرضى"));
            document.Blocks.Add(new Paragraph(new Run($"الفترة: {from:yyyy-MM-dd} إلى {to:yyyy-MM-dd}")));

            if (rows.Count == 0)
            {
                document.Blocks.Add(new Paragraph(new Run("لا توجد بيانات.")));
            }
            else
            {
                foreach (var row in rows.OrderBy(r => r.VisitDate))
                {
                    document.Blocks.Add(new Paragraph(new Run($"#{row.VisitId} | {row.VisitDate:yyyy-MM-dd HH:mm} | {row.PatientName} | عدد التحاليل: {row.TestsCount}")));
                }
            }

            PrintDocument(document, "WorksheetByPatient");
            return Task.CompletedTask;
        }

        public Task PrintWorksheetByTestAsync(DateTime from, DateTime to, IReadOnlyCollection<WorkSheetTestRow> rows)
        {
            var document = CreateDocument("Work Sheet By Test", 12);
            document.Blocks.Add(CreateHeader("ورقة العمل - حسب التحاليل"));
            document.Blocks.Add(new Paragraph(new Run($"الفترة: {from:yyyy-MM-dd} إلى {to:yyyy-MM-dd}")));

            if (rows.Count == 0)
            {
                document.Blocks.Add(new Paragraph(new Run("لا توجد بيانات.")));
            }
            else
            {
                foreach (var row in rows.OrderByDescending(r => r.Count).ThenBy(r => r.TestName))
                {
                    document.Blocks.Add(new Paragraph(new Run($"{row.TestName} | العدد: {row.Count}")));
                }
            }

            PrintDocument(document, "WorksheetByTest");
            return Task.CompletedTask;
        }

        private static FlowDocument CreateDocument(string title, double fontSize)
        {
            return new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = fontSize,
                PagePadding = new Thickness(48),
                ColumnWidth = double.PositiveInfinity,
                Language = System.Windows.Markup.XmlLanguage.GetLanguage("ar-EG")
            };
        }

        private static Paragraph CreateHeader(string text)
        {
            return new Paragraph(new Run(text))
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 12)
            };
        }

        private static void PrintDocument(FlowDocument document, string jobName)
        {
            var queue = ResolvePrintQueue();
            var writer = PrintQueue.CreateXpsDocumentWriter(queue);
            var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
            writer.Write(paginator);
        }

        private static PrintQueue ResolvePrintQueue()
        {
            var server = new LocalPrintServer();
            var queues = server.GetPrintQueues(new[]
            {
                EnumeratedPrintQueueTypes.Local,
                EnumeratedPrintQueueTypes.Connections
            });

            var pdfQueue = queues.FirstOrDefault(q => q.Name.Equals(PdfPrinterName, StringComparison.OrdinalIgnoreCase));
            return pdfQueue ?? LocalPrintServer.GetDefaultPrintQueue();
        }
    }
}
