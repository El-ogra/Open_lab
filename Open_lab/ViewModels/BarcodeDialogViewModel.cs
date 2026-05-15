using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
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
            CaseCode = data.CaseCode;
            FileCode = data.FileCode;
            LabCode = data.LabCode;
            CaseBarcode = barcodeService.GenerateCode128(CaseCode);
            FileBarcode = barcodeService.GenerateCode128(FileCode);
            LabBarcode = barcodeService.GenerateCode128(LabCode);
            TubeLabels = new ObservableCollection<TubeBarcodeLabel>();
            foreach (var label in data.SampleLabels)
            {
                var text = $"{LabCode}-{label}";
                TubeLabels.Add(new TubeBarcodeLabel { Text = text, Barcode = barcodeService.GenerateCode128(text, 300, 64) });
            }

            PrintAllCommand = new RelayCommand(async _ => await PrintAsync("كل أكواد الباركود", BuildAllLines()));
            PrintCaseCommand = new RelayCommand(async _ => await PrintAsync("كود الحالة", new[] { CaseCode }));
            PrintFileCommand = new RelayCommand(async _ => await PrintAsync("كود الملف", new[] { FileCode }));
            PrintLabCommand = new RelayCommand(async _ => await PrintAsync("كود المعمل", new[] { LabCode }));
        }

        public string PatientName { get; }
        public string CaseCode { get; }
        public string FileCode { get; }
        public string LabCode { get; }
        public ImageSource CaseBarcode { get; }
        public ImageSource FileBarcode { get; }
        public ImageSource LabBarcode { get; }
        public ObservableCollection<TubeBarcodeLabel> TubeLabels { get; }
        public ICommand PrintAllCommand { get; }
        public ICommand PrintCaseCommand { get; }
        public ICommand PrintFileCommand { get; }
        public ICommand PrintLabCommand { get; }

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

        private IReadOnlyCollection<string> BuildAllLines()
        {
            var lines = new List<string>
            {
                $"Patient: {PatientName}",
                $"Case: {CaseCode}",
                $"File: {FileCode}",
                $"Lab: {LabCode}",
                $"Offset X: {OffsetX:N0}, Offset Y: {OffsetY:N0}"
            };

            foreach (var tube in TubeLabels)
            {
                lines.Add($"Tube: {tube.Text}");
            }

            return lines;
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
        public string Text { get; set; } = string.Empty;
        public ImageSource? Barcode { get; set; }
    }
}
