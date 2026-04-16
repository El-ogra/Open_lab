using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class ReceiptPrintingViewModel : BaseViewModel
    {
        private readonly IReceiptService _receiptService;
        private readonly IPrintService _printService;
        private readonly IBarcodeService _barcodeService;
        private int _visitId;
        private string _patientName = string.Empty;
        private string _labId = string.Empty;
        private string _visitDate = string.Empty;
        private string _totalAmount = string.Empty;
        private string _paidAmount = string.Empty;
        private string _balanceAmount = string.Empty;
        private string _statusMessage = string.Empty;
        private ImageSource? _barcodeImage;
        private ReceiptData? _receiptData;

        public ReceiptPrintingViewModel(IReceiptService receiptService, IPrintService printService, IBarcodeService barcodeService)
        {
            _receiptService = receiptService;
            _printService = printService;
            _barcodeService = barcodeService;
            TestItems = new ObservableCollection<VisitTest>();
            LoadCommand = new RelayCommand(async _ => await LoadAsync());
            PrintReceiptCommand = new RelayCommand(async _ => await PrintReceiptAsync(), _ => _receiptData != null);
            PrintBarcodeCommand = new RelayCommand(async _ => await PrintBarcodeAsync(), _ => _receiptData != null);
        }

        public int VisitId
        {
            get => _visitId;
            set => SetProperty(ref _visitId, value);
        }

        public string PatientName
        {
            get => _patientName;
            private set => SetProperty(ref _patientName, value);
        }

        public string LabId
        {
            get => _labId;
            private set => SetProperty(ref _labId, value);
        }

        public string VisitDate
        {
            get => _visitDate;
            private set => SetProperty(ref _visitDate, value);
        }

        public string TotalAmount
        {
            get => _totalAmount;
            private set => SetProperty(ref _totalAmount, value);
        }

        public string PaidAmount
        {
            get => _paidAmount;
            private set => SetProperty(ref _paidAmount, value);
        }

        public string BalanceAmount
        {
            get => _balanceAmount;
            private set => SetProperty(ref _balanceAmount, value);
        }

        public ImageSource? BarcodeImage
        {
            get => _barcodeImage;
            private set => SetProperty(ref _barcodeImage, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<VisitTest> TestItems { get; }

        public ICommand LoadCommand { get; }
        public ICommand PrintReceiptCommand { get; }
        public ICommand PrintBarcodeCommand { get; }

        private async Task LoadAsync()
        {
            if (VisitId <= 0)
            {
                StatusMessage = "يرجى إدخال رقم الزيارة.";
                return;
            }

            try
            {
                var data = await _receiptService.GetReceiptDataAsync(VisitId);
                if (data == null)
                {
                    StatusMessage = "لم يتم العثور على بيانات الزيارة.";
                    _receiptData = null;
                    BarcodeImage = null;
                    RaisePrintCommands();
                    return;
                }

                _receiptData = data;
                PatientName = data.Patient.FullName;
                LabId = data.Patient.LabId;
                VisitDate = data.Visit.VisitDate.ToString("yyyy-MM-dd HH:mm");

                if (data.Invoice != null)
                {
                    TotalAmount = data.Invoice.NetTotal.ToString("N2");
                    PaidAmount = data.Invoice.Paid.ToString("N2");
                    BalanceAmount = data.Invoice.Balance.ToString("N2");
                }
                else
                {
                    TotalAmount = "—";
                    PaidAmount = "—";
                    BalanceAmount = "—";
                }

                BarcodeImage = _barcodeService.GenerateCode128(data.Patient.LabId);

                TestItems.Clear();
                foreach (var vt in data.VisitTests)
                {
                    TestItems.Add(vt);
                }

                RaisePrintCommands();
                StatusMessage = "تم تحميل بيانات الإيصال.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task PrintReceiptAsync()
        {
            if (_receiptData == null)
            {
                return;
            }

            try
            {
                await _printService.PrintReceiptAsync(_receiptData, _receiptData.Patient.LabId);
                StatusMessage = "تم إرسال الإيصال إلى Microsoft Print to PDF.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ طباعة: {ex.Message}";
            }
        }

        private async Task PrintBarcodeAsync()
        {
            if (_receiptData == null)
            {
                return;
            }

            try
            {
                await _printService.PrintReceiptAsync(_receiptData, _receiptData.Patient.LabId);
                StatusMessage = "تم إرسال الباركود للطباعة عبر Microsoft Print to PDF.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ طباعة: {ex.Message}";
            }
        }

        private void RaisePrintCommands()
        {
            (PrintReceiptCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (PrintBarcodeCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
