using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class BlankReportViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private int _visitId;
        private string _patientName = string.Empty;
        private string _labId = string.Empty;
        private string _gender = string.Empty;
        private string _birthDate = string.Empty;
        private string _phone = string.Empty;
        private string _visitDate = string.Empty;
        private string _referralName = string.Empty;
        private string _statusMessage = string.Empty;

        public BlankReportViewModel(IReportService reportService)
        {
            _reportService = reportService;
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

        private async Task LoadAsync()
        {
            if (VisitId <= 0)
            {
                StatusMessage = "يرجى إدخال رقم الزيارة.";
                return;
            }

            try
            {
                var report = await _reportService.GetVisitReportAsync(VisitId);
                if (report == null)
                {
                    StatusMessage = "لم يتم العثور على بيانات.";
                    return;
                }

                PatientName = report.Patient.FullName;
                LabId = report.Patient.LabId;
                Gender = report.Patient.Gender;
                BirthDate = report.Patient.BirthDate?.ToString("yyyy-MM-dd") ?? "—";
                Phone = report.Patient.Phone ?? "—";
                VisitDate = report.Visit.VisitDate.ToString("yyyy-MM-dd");
                ReferralName = report.Visit.Referral?.Name ?? "—";

                StatusMessage = "تم تحميل بيانات المريض.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
