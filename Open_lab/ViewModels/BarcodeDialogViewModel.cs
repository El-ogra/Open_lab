using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    /// <summary>
    /// ViewModel لنافذة طباعة الباركود — تعرض ملصقات مطابقة للنظام المرجعي
    /// (Barcode Labels — التكويد العمودي للعينات):
    ///   - File Barcode (برتقالي) — يُلصق على ملف المريض
    ///   - Lab ID Barcode (أحمر) — كود المعمل لمتابعة التاريخ المرضي
    ///   - Tube Labels — ملصق لكل عينة/تحليل (CBC, ESR, Urine, Semen, Stool ...)
    /// </summary>
    public class BarcodeDialogViewModel : BaseViewModel
    {
        private readonly IPrintService? _printService;
        private double _offsetX;
        private double _offsetY;

        public BarcodeDialogViewModel(IBarcodeService barcodeService, IPrintService? printService, BarcodeDialogData data)
        {
            if (barcodeService == null) throw new ArgumentNullException(nameof(barcodeService));
            data ??= new BarcodeDialogData();
            _printService = printService;

            PatientName = data.PatientName;
            Gender = string.IsNullOrWhiteSpace(data.Gender) ? "—" : data.Gender;
            Age = data.Age;
            AgeUnit = string.IsNullOrWhiteSpace(data.AgeUnit) ? "Years" : data.AgeUnit;
            BarcodeDate = data.BarcodeDate == default ? DateTime.Now : data.BarcodeDate;

            CaseCode = data.CaseCode;
            FileCode = data.FileCode;
            LabCode = data.LabCode;

            // العنوان الفرعي الذي يظهر فوق كل ملصق: "اسم - الجنس - السن"
            PatientHeaderText = BuildPatientHeader();
            BarcodeDateText = BarcodeDate.ToString("dd-MMMM-yyyy hh:mm tt", CultureInfo.InvariantCulture);

            CaseBarcode = SafeGenerate(barcodeService, CaseCode, 360, 96);
            FileBarcode = SafeGenerate(barcodeService, FileCode, 360, 96);
            LabBarcode = SafeGenerate(barcodeService, LabCode, 360, 96);

            TubeLabels = new ObservableCollection<TubeBarcodeLabel>();
            foreach (var label in data.SampleLabels)
            {
                if (string.IsNullOrWhiteSpace(label))
                {
                    continue;
                }

                var contentForBarcode = string.IsNullOrWhiteSpace(LabCode) ? label : $"{LabCode}";
                TubeLabels.Add(new TubeBarcodeLabel
                {
                    Text = label,
                    Code = contentForBarcode,
                    PatientHeader = PatientHeaderText,
                    Barcode = SafeGenerate(barcodeService, contentForBarcode, 300, 64)
                });
            }

            PrintAllCommand = new RelayCommand(async _ => await PrintAsync("كل أكواد الباركود", BuildAllLines()));
            PrintCaseCommand = new RelayCommand(async _ => await PrintAsync("كود الحالة", new[] { CaseCode }));
            PrintFileCommand = new RelayCommand(async _ => await PrintAsync("كود الملف", new[] { FileCode }));
            PrintLabCommand = new RelayCommand(async _ => await PrintAsync("كود المعمل", new[] { LabCode }));
            PrintTubesCommand = new RelayCommand(async _ => await PrintTubesAsync());
        }

        public string PatientName { get; }
        public string Gender { get; }
        public int? Age { get; }
        public string AgeUnit { get; }
        public DateTime BarcodeDate { get; }
        public string PatientHeaderText { get; }
        public string BarcodeDateText { get; }

        public string CaseCode { get; }
        public string FileCode { get; }
        public string LabCode { get; }

        public ImageSource? CaseBarcode { get; }
        public ImageSource? FileBarcode { get; }
        public ImageSource? LabBarcode { get; }

        public ObservableCollection<TubeBarcodeLabel> TubeLabels { get; }

        public ICommand PrintAllCommand { get; }
        public ICommand PrintCaseCommand { get; }
        public ICommand PrintFileCommand { get; }
        public ICommand PrintLabCommand { get; }
        public ICommand PrintTubesCommand { get; }

        public double OffsetX
        {
            get => _offsetX;
            set => SetProperty(ref _offsetX, value);
        }

        public double OffsetY
        {
            get => _offsetY;
            set => SetProperty(ref _offsetY, value);
        }

        private string BuildPatientHeader()
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(PatientName)) parts.Add(PatientName);

            var genderPart = string.Equals(Gender, "Male", StringComparison.OrdinalIgnoreCase) ? "Male"
                          : string.Equals(Gender, "Female", StringComparison.OrdinalIgnoreCase) ? "Female"
                          : Gender;

            if (Age.HasValue && !string.IsNullOrWhiteSpace(genderPart) && genderPart != "—")
            {
                parts.Add($"{genderPart} - {Age.Value} {AgeUnit}");
            }
            else if (Age.HasValue)
            {
                parts.Add($"{Age.Value} {AgeUnit}");
            }
            else if (!string.IsNullOrWhiteSpace(genderPart) && genderPart != "—")
            {
                parts.Add(genderPart);
            }

            return string.Join(Environment.NewLine, parts);
        }

        private static ImageSource? SafeGenerate(IBarcodeService barcodeService, string content, int width, int height)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }
            try
            {
                return barcodeService.GenerateCode128(content, width, height);
            }
            catch
            {
                return null;
            }
        }

        private IReadOnlyCollection<string> BuildAllLines()
        {
            var lines = new List<string>
            {
                $"Patient: {PatientName}",
                $"Gender: {Gender}",
                $"Age: {(Age.HasValue ? Age.Value.ToString(CultureInfo.InvariantCulture) + " " + AgeUnit : "—")}",
                $"Date: {BarcodeDateText}",
                $"Case: {CaseCode}",
                $"File: {FileCode}",
                $"Lab: {LabCode}",
                $"Offset X: {OffsetX:N0}, Offset Y: {OffsetY:N0}"
            };

            foreach (var tube in TubeLabels)
            {
                lines.Add($"Tube: {tube.Text}  [{tube.Code}]");
            }

            return lines;
        }

        private async Task PrintTubesAsync()
        {
            var lines = new List<string>();
            foreach (var tube in TubeLabels)
            {
                lines.Add($"{tube.PatientHeader} | {tube.Text} | {tube.Code}");
            }
            if (lines.Count == 0)
            {
                lines.Add("(لا توجد ملصقات أنابيب)");
            }
            await PrintAsync("ملصقات الأنابيب", lines);
        }

        private async Task PrintAsync(string title, IReadOnlyCollection<string> lines)
        {
            if (_printService == null)
            {
                return;
            }

            await _printService.PrintTextReportAsync(title, lines, title);
        }
    }

    public class TubeBarcodeLabel
    {
        /// <summary>اسم العينة/التحليل المطبوع على الملصق (مثل: CBC, ESR, Urine).</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>الكود المُشفّر داخل الباركود نفسه (عادةً Lab ID).</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>رأس الملصق: اسم المريض + الجنس + السن.</summary>
        public string PatientHeader { get; set; } = string.Empty;

        public ImageSource? Barcode { get; set; }
    }
}
