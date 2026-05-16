using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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

        // Persistence path for OffsetX/OffsetY (FIX: previously kept only in static fields,
        // values were lost when the application closed).
        private static readonly string SettingsFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "barcode_settings.json");

        private static double _savedOffsetX;
        private static double _savedOffsetY;
        private static bool _settingsLoaded;
        private double _offsetX;
        private double _offsetY;

        public BarcodeDialogViewModel(IBarcodeService barcodeService, IPrintService? printService, BarcodeDialogData data)
        {
            if (barcodeService == null) throw new ArgumentNullException(nameof(barcodeService));
            data ??= new BarcodeDialogData();
            _printService = printService;

            // Lazy-load persisted offsets the first time any ViewModel is created.
            EnsureSettingsLoaded();
            _offsetX = _savedOffsetX;
            _offsetY = _savedOffsetY;

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

                // CRITICAL FIX Phase 0: Generate unique barcode for each tube label (C-11)
                // كل ملصق يُشفّر قيمة مختلفة تتضمن نوع العينة + كود المريض (LabCode)
                var safeLabel = label.Trim();
                var contentForBarcode = string.IsNullOrWhiteSpace(LabCode)
                    ? safeLabel
                    : $"{LabCode}-{safeLabel}";
                TubeLabels.Add(new TubeBarcodeLabel
                {
                    Text = safeLabel,
                    Code = contentForBarcode,
                    PatientHeader = PatientHeaderText,
                    Barcode = SafeGenerate(barcodeService, contentForBarcode, 300, 64)
                });
            }

            PrintAllCommand = new RelayCommand(async _ => await PrintAllBarcodeImagesAsync());
            PrintCaseCommand = new RelayCommand(async _ => await PrintBarcodeImageAsync(CaseBarcode, CaseCode, "كود الحالة"));
            PrintFileCommand = new RelayCommand(async _ => await PrintBarcodeImageAsync(FileBarcode, FileCode, "كود الملف"));
            PrintLabCommand = new RelayCommand(async _ => await PrintBarcodeImageAsync(LabBarcode, LabCode, "كود المعمل"));
            PrintTubesCommand = new RelayCommand(async _ => await PrintTubeLabelsAsync());
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
            set
            {
                if (SetProperty(ref _offsetX, value))
                {
                    _savedOffsetX = value;
                    PersistSettings();
                }
            }
        }

        public double OffsetY
        {
            get => _offsetY;
            set
            {
                if (SetProperty(ref _offsetY, value))
                {
                    _savedOffsetY = value;
                    PersistSettings();
                }
            }
        }

        /// <summary>
        /// تحميل OffsetX/OffsetY المحفوظتين من ملف JSON على القرص.
        /// تُستدعى مرة واحدة فقط في عمر التطبيق (lazy load).
        /// أي خطأ في القراءة يُتجاهل بصمت — تُستخدم القيم الافتراضية (0,0).
        /// </summary>
        private static void EnsureSettingsLoaded()
        {
            if (_settingsLoaded)
            {
                return;
            }
            _settingsLoaded = true;

            try
            {
                if (!File.Exists(SettingsFilePath))
                {
                    return;
                }

                var json = File.ReadAllText(SettingsFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return;
                }

                var settings = JsonSerializer.Deserialize<BarcodeOffsetSettings>(json);
                if (settings != null)
                {
                    _savedOffsetX = settings.OffsetX;
                    _savedOffsetY = settings.OffsetY;
                }
            }
            catch
            {
                // ملف تالف أو مرفوض الوصول — تجاهل بصمت واستخدم الافتراضي
            }
        }

        /// <summary>
        /// حفظ القيم الحالية لـ OffsetX/OffsetY في ملف JSON على القرص.
        /// يُستدعى عند كل تغيير في إحدى الإزاحتين.
        /// أي خطأ كتابة (صلاحيات، قرص ممتلئ...) يُتجاهل بصمت.
        /// </summary>
        private static void PersistSettings()
        {
            try
            {
                var settings = new BarcodeOffsetSettings
                {
                    OffsetX = _savedOffsetX,
                    OffsetY = _savedOffsetY
                };

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(SettingsFilePath, json);
            }
            catch
            {
                // فشل الكتابة — تجاهل (لا نعطّل واجهة الطباعة بسبب فشل في حفظ الإعدادات)
            }
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

        // CRITICAL FIX Phase 0: Print actual barcode image instead of just text (C-04)
        private async Task PrintBarcodeImageAsync(ImageSource? barcodeImage, string barcodeText, string title)
        {
            if (_printService == null)
            {
                return;
            }

            var additionalInfo = $"{PatientName} | {Gender} | {Age ?? 0} {AgeUnit} | {BarcodeDateText}";
            await _printService.PrintBarcodeImageAsync(title, barcodeImage, barcodeText, additionalInfo);
        }

        // CRITICAL FIX Phase 0: Print all barcode images together (C-04)
        private async Task PrintAllBarcodeImagesAsync()
        {
            await PrintAsync("طباعة كل الأكواد", BuildAllLines());
        }

        // CRITICAL FIX Phase 0: Print tube labels with unique barcodes (C-11)
        // Previously all tubes had the same LabCode as barcode
        private async Task PrintTubeLabelsAsync()
        {
            if (_printService == null || TubeLabels.Count == 0)
            {
                return;
            }

            // Print each tube label with its own unique barcode
            foreach (var tube in TubeLabels)
            {
                var info = $"{tube.PatientHeader}{Environment.NewLine}{tube.Text}";
                await _printService.PrintBarcodeImageAsync($"ملصق: {tube.Text}", tube.Barcode, tube.Code, info);
            }
        }

        /// <summary>
        /// CRITICAL FIX Phase 0: بناء FlowDocument يحوي صور الباركود الفعلية (ImageSource)
        /// لكل ملصق أنبوب بدلاً من إرسال نصوص فقط عبر PrintTextReportAsync.
        /// كل ملصق يحتوي على: صورة الباركود، نص اسم العينة، اسم المريض، وتاريخ الزيارة.
        /// </summary>
        private Task PrintAsync(string title, IReadOnlyCollection<string> lines)
        {
            if (TubeLabels.Count == 0 && CaseBarcode == null && FileBarcode == null && LabBarcode == null)
            {
                return Task.CompletedTask;
            }

            var doc = new FlowDocument
            {
                PagePadding = new Thickness(24 + OffsetX, 24 + OffsetY, 24, 24),
                ColumnGap = 0,
                ColumnWidth = double.PositiveInfinity,
                FlowDirection = FlowDirection.RightToLeft
            };

            var section = new Section();

            // رأس المستند
            if (!string.IsNullOrWhiteSpace(title))
            {
                section.Blocks.Add(new Paragraph(new Run(title))
                {
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center
                });
            }

            section.Blocks.Add(new Paragraph(new Run(PatientHeaderText))
            {
                FontSize = 12,
                TextAlignment = TextAlignment.Center
            });

            section.Blocks.Add(new Paragraph(new Run(BarcodeDateText))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Center,
                Foreground = Brushes.DimGray
            });

            // الباركودات الرئيسية: Case / File / Lab
            AppendBarcodeBlock(section, CaseBarcode, "كود الحالة", CaseCode);
            AppendBarcodeBlock(section, FileBarcode, "كود الملف", FileCode);
            AppendBarcodeBlock(section, LabBarcode, "كود المعمل", LabCode);

            // ملصقات الأنابيب — صورة باركود فعلية لكل عينة + اسم العينة + اسم المريض + التاريخ
            foreach (var label in TubeLabels)
            {
                if (label.Barcode == null)
                {
                    continue;
                }

                // فاصل بين الملصقات
                section.Blocks.Add(new BlockUIContainer(new Separator()));

                // صورة الباركود
                var image = new Image
                {
                    Source = label.Barcode,
                    Width = 200,
                    Height = 80,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                var imageBlock = new BlockUIContainer(image)
                {
                    TextAlignment = TextAlignment.Center
                };
                section.Blocks.Add(imageBlock);

                // نص اسم العينة (نوع التحليل)
                section.Blocks.Add(new Paragraph(new Run(label.Text))
                {
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center
                });

                // الكود المُشفّر داخل الباركود (مرجع نصي)
                section.Blocks.Add(new Paragraph(new Run(label.Code))
                {
                    FontSize = 10,
                    TextAlignment = TextAlignment.Center,
                    Foreground = Brushes.Gray
                });

                // اسم المريض
                section.Blocks.Add(new Paragraph(new Run(PatientName))
                {
                    FontSize = 11,
                    TextAlignment = TextAlignment.Center
                });

                // تاريخ الزيارة
                section.Blocks.Add(new Paragraph(new Run(BarcodeDateText))
                {
                    FontSize = 10,
                    TextAlignment = TextAlignment.Center,
                    Foreground = Brushes.DimGray
                });
            }

            doc.Blocks.Add(section);

            // إرسال المستند للطابعة عبر PrintDialog
            try
            {
                var pd = new System.Windows.Controls.PrintDialog();
                if (pd.ShowDialog() == true)
                {
                    var paginator = ((IDocumentPaginatorSource)doc).DocumentPaginator;
                    pd.PrintDocument(paginator, string.IsNullOrWhiteSpace(title) ? "Barcode Labels" : title);
                }
            }
            catch
            {
                // فشل عرض PrintDialog (مثلاً في بيئة headless) — تجاهل بصمت
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// يُلحق كتلة باركود رئيسية (Case/File/Lab) بالـ Section مع صورته وعنوانه ونصه.
        /// </summary>
        private static void AppendBarcodeBlock(Section section, ImageSource? barcode, string title, string code)
        {
            if (barcode == null || string.IsNullOrWhiteSpace(code))
            {
                return;
            }

            section.Blocks.Add(new BlockUIContainer(new Separator()));

            section.Blocks.Add(new Paragraph(new Run(title))
            {
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            });

            var image = new Image
            {
                Source = barcode,
                Width = 240,
                Height = 90,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            section.Blocks.Add(new BlockUIContainer(image)
            {
                TextAlignment = TextAlignment.Center
            });

            section.Blocks.Add(new Paragraph(new Run(code))
            {
                FontSize = 11,
                TextAlignment = TextAlignment.Center,
                Foreground = Brushes.Gray
            });
        }

        /// <summary>
        /// نموذج تسلسل الإعدادات إلى/من JSON على القرص.
        /// </summary>
        private sealed class BarcodeOffsetSettings
        {
            public double OffsetX { get; set; }
            public double OffsetY { get; set; }
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

        /// <summary>صورة الباركود المُولّدة.</summary>
        public ImageSource? Barcode { get; set; }
    }
}