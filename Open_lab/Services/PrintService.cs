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
        private readonly ISettingsService _settingsService;

        public PrintService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

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

                foreach (var resultItem in test.Results)
                {
                    var result = resultItem.Result;
                    var resultText = $"- {result.Parameter.Name}: {result.Value ?? "-"} {result.Flag}";

                    if (!string.IsNullOrWhiteSpace(resultItem.PreviousValue))
                    {
                        resultText += $" [Prev: {resultItem.PreviousValue} on {resultItem.PreviousDate:yyyy-MM-dd}]";
                    }

                    document.Blocks.Add(new Paragraph(new Run(resultText)));
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

        public Task PrintTextReportAsync(string title, IReadOnlyCollection<string> lines, string? jobName = null)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("العنوان مطلوب.", nameof(title));
            }

            var document = CreateDocument(title, 12);
            document.Blocks.Add(CreateHeader(title));

            if (lines.Count == 0)
            {
                document.Blocks.Add(new Paragraph(new Run("لا توجد بيانات.")));
            }
            else
            {
                foreach (var line in lines)
                {
                    document.Blocks.Add(new Paragraph(new Run(line)));
                }
            }

            PrintDocument(document, jobName ?? title);
            return Task.CompletedTask;
        }

        public Task PrintCultureReportAsync(CultureReportData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var document = CreateDocument("تقرير مزرعة وحساسية", 11);
            document.Blocks.Add(CreateHeader("تقرير مزرعة وحساسية (Culture & Sensitivity Report)"));

            document.Blocks.Add(new Paragraph(new Run($"المريض: {data.PatientName} | Lab ID: {data.LabId}")));
            document.Blocks.Add(new Paragraph(new Run($"تاريخ الزيارة: {data.VisitDate:yyyy-MM-dd} | المزرعة: {data.CultureName}")));

            var table = new Table { CellSpacing = 0, BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(0, 0, 0, 1) };
            table.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            table.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) });

            var headerRowGroup = new TableRowGroup();
            var headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("المضاد الحيوي (Antibiotic)")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("الحساسية (Sensitivity)")) { FontWeight = FontWeights.Bold }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run("ملاحظات (Notes)")) { FontWeight = FontWeights.Bold }));
            headerRowGroup.Rows.Add(headerRow);
            table.RowGroups.Add(headerRowGroup);

            var bodyRowGroup = new TableRowGroup();
            foreach (var res in data.Results)
            {
                var row = new TableRow();
                row.Cells.Add(new TableCell(new Paragraph(new Run(res.AntibioticName))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(res.Sensitivity))));
                row.Cells.Add(new TableCell(new Paragraph(new Run(res.Comment ?? "-"))));
                bodyRowGroup.Rows.Add(row);
            }
            table.RowGroups.Add(bodyRowGroup);
            document.Blocks.Add(table);

            PrintDocument(document, $"CultureReport_{data.LabId}");
            return Task.CompletedTask;
        }

        // CRITICAL FIX Phase 0: Implement actual barcode image printing (C-04)
        // Previously only printed text, now prints actual barcode images
        public Task PrintBarcodeImageAsync(string title, System.Windows.Media.ImageSource? barcodeImage, string barcodeText, string? additionalInfo = null)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("العنوان مطلوب.", nameof(title));
            }

            var document = CreateDocument(title, 12);
            document.Blocks.Add(CreateHeader(title));

            // Add barcode image if provided
            if (barcodeImage != null)
            {
                var image = new System.Windows.Controls.Image
                {
                    Source = barcodeImage,
                    Stretch = System.Windows.Media.Stretch.Uniform,
                    Width = 300,
                    Height = 80
                };

                var container = new System.Windows.Documents.BlockUIContainer(image)
                {
                    Margin = new Thickness(0, 10, 0, 10)
                };
                document.Blocks.Add(container);
            }

            // Add barcode text (for scanner readability if image fails)
            if (!string.IsNullOrWhiteSpace(barcodeText))
            {
                document.Blocks.Add(new Paragraph(new Run($"الكود: {barcodeText}"))
                {
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = System.Windows.TextAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 5)
                });
            }

            // Add additional info if provided
            if (!string.IsNullOrWhiteSpace(additionalInfo))
            {
                document.Blocks.Add(new Paragraph(new Run(additionalInfo))
                {
                    FontSize = 10,
                    TextAlignment = System.Windows.TextAlignment.Center,
                    Foreground = System.Windows.Media.Brushes.Gray
                });
            }

            PrintDocument(document, $"Barcode_{barcodeText}");
            return Task.CompletedTask;
        }

        private static FlowDocument CreateDocument(string title, double fontSize, double leftMarginCm = 0, double rightMarginCm = 0)
        {
            // Convert cm to device-independent pixels (1 cm ≈ 37.8 pixels)
            const double cmToPixels = 37.8;
            var leftMargin = leftMarginCm * cmToPixels;
            var rightMargin = rightMarginCm * cmToPixels;
            var topBottomMargin = 48; // Default top/bottom margin in pixels

            return new FlowDocument
            {
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
                FontSize = fontSize,
                PagePadding = new Thickness(leftMargin, topBottomMargin, rightMargin, topBottomMargin),
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

        public async Task<PrintQueue> ResolvePrintQueueAsync(string documentType)
        {
            var server = new LocalPrintServer();
            var queues = server.GetPrintQueues(new[]
            {
                EnumeratedPrintQueueTypes.Local,
                EnumeratedPrintQueueTypes.Connections
            }).ToList();

            string? printerName = null;

            // Determine which printer to use based on document type
            switch (documentType?.ToLowerInvariant())
            {
                case "receipt":
                    printerName = await _settingsService.GetReceiptPrinterAsync();
                    break;
                case "report":
                    printerName = await _settingsService.GetReportPrinterAsync();
                    break;
                default:
                    printerName = await _settingsService.GetDefaultPrinterAsync();
                    break;
            }

            // If a specific printer is configured and exists, use it
            if (!string.IsNullOrEmpty(printerName))
            {
                var configuredQueue = queues.FirstOrDefault(q => q.Name.Equals(printerName, StringComparison.OrdinalIgnoreCase));
                if (configuredQueue != null)
                    return configuredQueue;
            }

            // Fallback to PDF printer or default
            var pdfQueue = queues.FirstOrDefault(q => q.Name.Equals(PdfPrinterName, StringComparison.OrdinalIgnoreCase));
            return pdfQueue ?? LocalPrintServer.GetDefaultPrintQueue();
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