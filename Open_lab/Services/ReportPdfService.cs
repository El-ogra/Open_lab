using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Open_lab.Models;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Open_lab.Services
{
    public class ReportPdfService : IReportPdfService
    {
        private const double PointsPerCentimeter = 28.3464567;
        private readonly ISystemSettingsService _settingsService;

        public ReportPdfService(ISystemSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public async Task<string> GenerateVisitReportPdfAsync(VisitReportData report)
        {
            if (report == null)
            {
                throw new ArgumentNullException(nameof(report));
            }

            var settings = await _settingsService.GetProfileAsync();
            var outputDirectory = Path.Combine(Path.GetTempPath(), "OpenLabReports");
            Directory.CreateDirectory(outputDirectory);
            var outputPath = Path.Combine(outputDirectory, $"visit-report-{report.Visit.VisitId}-{DateTime.UtcNow:yyyyMMddHHmmssfff}.pdf");

            using var document = new PdfDocument();
            document.Info.Title = $"Visit report {report.Visit.VisitId}";
            var page = document.AddPage();
            SetA4Size(page);

            var graphics = XGraphics.FromPdfPage(page);
            var regular = new XFont("Arial", 10, XFontStyle.Regular);
            var bold = new XFont("Arial", 12, XFontStyle.Bold);
            var title = new XFont("Arial", 18, XFontStyle.Bold);

            var left = Math.Max(0.5, settings.ReportMarginLeft) * PointsPerCentimeter;
            var right = Math.Max(0.5, settings.ReportMarginRight) * PointsPerCentimeter;
            var top = Math.Max(0.5, settings.ReportMarginTop) * PointsPerCentimeter;
            var bottom = Math.Max(0.5, settings.ReportMarginBottom) * PointsPerCentimeter;
            var contentWidth = page.Width.Point - left - right;
            var y = top;

            DrawLogo(graphics, settings.ReportLogoPath, left, y);
            graphics.DrawString(settings.ReportHeader, title, XBrushes.Black, new XRect(left, y, contentWidth, 28), XStringFormats.TopCenter);
            y += 38;

            DrawLine(graphics, regular, ref y, left, contentWidth, $"Patient: {report.Patient.FullName}");
            DrawLine(graphics, regular, ref y, left, contentWidth, $"Visit: {report.Visit.VisitId}    Date: {report.Visit.VisitDate:yyyy-MM-dd HH:mm}");
            if (!string.IsNullOrWhiteSpace(report.Patient.LabId))
            {
                DrawLine(graphics, regular, ref y, left, contentWidth, $"Lab ID: {report.Patient.LabId}");
            }

            y += 12;
            foreach (var test in report.Tests)
            {
                graphics = EnsureSpace(document, ref page, graphics, ref y, top, bottom);
                graphics.DrawString(test.Test.NameReport, bold, XBrushes.Black, new XRect(left, y, contentWidth, 18), XStringFormats.TopLeft);
                y += 20;

                foreach (var resultItem in test.Results)
                {
                    graphics = EnsureSpace(document, ref page, graphics, ref y, top, bottom);
                    var result = resultItem.Result;
                    var line = string.Format(
                        CultureInfo.InvariantCulture,
                        "{0}: {1} {2}",
                        result.Parameter.Name,
                        string.IsNullOrWhiteSpace(result.Value) ? "-" : result.Value,
                        string.IsNullOrWhiteSpace(result.Flag) ? string.Empty : $"({result.Flag})");
                    graphics.DrawString(line, regular, XBrushes.Black, new XRect(left + 14, y, contentWidth - 14, 16), XStringFormats.TopLeft);
                    y += 16;

                    if (!string.IsNullOrWhiteSpace(resultItem.PreviousValue))
                    {
                        graphics.DrawString($"Previous: {resultItem.PreviousValue} ({resultItem.PreviousDate:yyyy-MM-dd})", regular, XBrushes.Gray, new XRect(left + 28, y, contentWidth - 28, 14), XStringFormats.TopLeft);
                        y += 14;
                    }
                }

                y += 8;
            }

            graphics.DrawString(settings.ReportFooter, regular, XBrushes.Gray, new XRect(left, page.Height.Point - bottom + 8, contentWidth, 16), XStringFormats.TopCenter);
            graphics.Dispose();
            document.Save(outputPath);
            return outputPath;
        }

        private static void DrawLine(XGraphics graphics, XFont font, ref double y, double left, double width, string text)
        {
            graphics.DrawString(text, font, XBrushes.Black, new XRect(left, y, width, 16), XStringFormats.TopLeft);
            y += 16;
        }

        private static void DrawLogo(XGraphics graphics, string? logoPath, double left, double top)
        {
            if (string.IsNullOrWhiteSpace(logoPath) || !File.Exists(logoPath))
            {
                return;
            }

            using var image = XImage.FromFile(logoPath);
            graphics.DrawImage(image, left, top, 70, 45);
        }

        private static XGraphics EnsureSpace(PdfDocument document, ref PdfPage page, XGraphics graphics, ref double y, double top, double bottom)
        {
            if (y <= page.Height.Point - bottom - 40)
            {
                return graphics;
            }

            graphics.Dispose();
            page = document.AddPage();
            SetA4Size(page);
            y = top;
            return XGraphics.FromPdfPage(page);
        }

        private static void SetA4Size(PdfPage page)
        {
            page.Width = 595;
            page.Height = 842;
        }
    }
}
