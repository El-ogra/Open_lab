using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class ReceiptPrintingViewModel : BaseViewModel
    {
        private readonly IReceiptService _receiptService;
        private int _visitId;
        private string _patientName = string.Empty;
        private string _labId = string.Empty;
        private string _visitDate = string.Empty;
        private string _totalAmount = string.Empty;
        private string _paidAmount = string.Empty;
        private string _balanceAmount = string.Empty;
        private string _statusMessage = string.Empty;

        public ReceiptPrintingViewModel(IReceiptService receiptService)
        {
            _receiptService = receiptService;
            TestItems = new ObservableCollection<VisitTest>();
            LoadCommand = new RelayCommand(async _ => await LoadAsync());
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

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<VisitTest> TestItems { get; }

        public ICommand LoadCommand { get; }

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
                    return;
                }

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

                TestItems.Clear();
                foreach (var vt in data.VisitTests)
                {
                    TestItems.Add(vt);
                }

                StatusMessage = "تم تحميل بيانات الإيصال.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
