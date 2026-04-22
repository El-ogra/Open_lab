using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class BlankReportViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IPrintService? _printService;
        private int _visitId;
        private string _patientName = string.Empty;
        private string _labId = string.Empty;
        private string _gender = string.Empty;
        private string _birthDate = string.Empty;
        private string _phone = string.Empty;
        private string _visitDate = string.Empty;
        private string _referralName = string.Empty;
        private string _statusMessage = string.Empty;
        private VisitReportData? _reportData;

        public BlankReportViewModel(IReportService reportService, IPrintService? printService = null)
        {
            _reportService = reportService;
            _printService = printService;
            LoadCommand = new RelayCommand(async _ => await LoadAsync());
            PrintBlankCommand = new RelayCommand(async _ => await PrintBlankAsync(), _ => _printService != null && VisitId > 0);
        }

        public int VisitId
        {
            get => _visitId;
            set
            {
                if (SetProperty(ref _visitId, value))
                {
                    (PrintBlankCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
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

        public string Gender
        {
            get => _gender;
            private set => SetProperty(ref _gender, value);
        }

        public string BirthDate
        {
            get => _birthDate;
            private set => SetProperty(ref _birthDate, value);
        }

        public string Phone
        {
            get => _phone;
            private set => SetProperty(ref _phone, value);
        }

        public string VisitDate
        {
            get => _visitDate;
            private set => SetProperty(ref _visitDate, value);
        }

        public string ReferralName
        {
            get => _referralName;
            private set => SetProperty(ref _referralName, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand PrintBlankCommand { get; }

        private async Task LoadAsync()
        {
            if (VisitId <= 0)
            {
                StatusMessage = "يرجى إدخال رقم الزيارة.";
                return;
            }

            try
            {
                _reportData = await _reportService.GetVisitReportAsync(VisitId);
                if (_reportData == null)
                {
                    StatusMessage = "لم يتم العثور على بيانات.";
                    return;
                }

                PatientName = _reportData.Patient.FullName;
                LabId = _reportData.Patient.LabId;
                Gender = _reportData.Patient.Gender;
                BirthDate = _reportData.Patient.BirthDate?.ToString("yyyy-MM-dd") ?? "—";
                Phone = _reportData.Patient.Phone ?? "—";
                VisitDate = _reportData.Visit.VisitDate.ToString("yyyy-MM-dd");
                ReferralName = _reportData.Visit.Referral?.Name ?? "—";

                StatusMessage = "تم تحميل بيانات المريض.";
                (PrintBlankCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task PrintBlankAsync()
        {
            if (_printService == null)
            {
                StatusMessage = "خدمة الطباعة غير متاحة.";
                return;
            }

            if (VisitId <= 0)
            {
                StatusMessage = "يرجى إدخال رقم الزيارة.";
                return;
            }

            try
            {
                _reportData ??= await _reportService.GetVisitReportAsync(VisitId);
                if (_reportData == null)
                {
                    StatusMessage = "لم يتم العثور على بيانات.";
                    return;
                }

                var lines = new ObservableCollection<string>
                {
                    $"المريض: {_reportData.Patient.FullName}",
                    $"Lab ID: {_reportData.Patient.LabId}",
                    $"التاريخ: {_reportData.Visit.VisitDate:yyyy-MM-dd}",
                    string.Empty,
                    "تحاليل مطلوبة (بدون نتائج):"
                };

                foreach (var test in _reportData.Tests)
                {
                    lines.Add($"- {test.Test.NameReport}");
                }

                await _printService.PrintTextReportAsync("تقرير فارغ", lines, $"BlankReport_{VisitId}");
                StatusMessage = "تم إرسال التقرير الفارغ للطباعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
