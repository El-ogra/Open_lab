using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class BlankReportViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private readonly IBlankReportService _blankReportService;
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

        // Gap 4.8 — Print Blank Report Architecture Fix:
        // The ViewModel now depends on IBlankReportService and never builds the
        // report contents itself or calls IPrintService directly.
        public BlankReportViewModel(IReportService reportService, IBlankReportService blankReportService)
        {
            _reportService = reportService;
            _blankReportService = blankReportService;
            LoadCommand = new RelayCommand(async _ => await LoadAsync());
            PrintBlankCommand = new RelayCommand(async _ => await PrintBlankAsync(), _ => VisitId > 0);
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

        // Gap 4.8 — All building / printing responsibility delegated to BlankReportService.
        private async Task PrintBlankAsync()
        {
            if (VisitId <= 0)
            {
                StatusMessage = "يرجى إدخال رقم الزيارة.";
                return;
            }

            try
            {
                var success = await _blankReportService.PrintBlankReportAsync(VisitId);
                if (!success)
                {
                    StatusMessage = "تعذّر طباعة التقرير الفارغ (لا توجد بيانات أو خدمة الطباعة غير متاحة).";
                    return;
                }

                StatusMessage = "تم إرسال التقرير الفارغ للطباعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
