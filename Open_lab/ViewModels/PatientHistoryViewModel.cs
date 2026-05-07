using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class PatientHistoryViewModel : BaseViewModel
    {
        private readonly IPatientService _patientService;
        private readonly IReportService _reportService;
        private readonly IPrintService _printService;
        private string _labId = string.Empty;
        private DateTime? _from;
        private DateTime? _to;
        private string _statusMessage = string.Empty;
        private PatientHistoryReportData? _history;

        public PatientHistoryViewModel(IPatientService patientService, IReportService reportService, IPrintService printService)
        {
            _patientService = patientService;
            _reportService = reportService;
            _printService = printService;
            Visits = new ObservableCollection<VisitReportData>();
            LoadHistoryCommand = new RelayCommand(async _ => await LoadHistoryAsync());
            PrintHistoryCommand = new RelayCommand(async _ => await PrintHistoryAsync(), _ => History != null);
        }

        public string LabId
        {
            get => _labId;
            set => SetProperty(ref _labId, value);
        }

        public DateTime? From
        {
            get => _from;
            set => SetProperty(ref _from, value);
        }

        public DateTime? To
        {
            get => _to;
            set => SetProperty(ref _to, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public PatientHistoryReportData? History
        {
            get => _history;
            private set
            {
                if (SetProperty(ref _history, value))
                {
                    (PrintHistoryCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<VisitReportData> Visits { get; }

        public ICommand LoadHistoryCommand { get; }
        public ICommand PrintHistoryCommand { get; }

        private async Task LoadHistoryAsync()
        {
            if (string.IsNullOrWhiteSpace(LabId))
            {
                StatusMessage = "يرجى إدخال Lab ID.";
                return;
            }

            try
            {
                var patient = await _patientService.GetByLabIdAsync(LabId);
                if (patient == null)
                {
                    StatusMessage = "لم يتم العثور على المريض.";
                    return;
                }

                var history = await _reportService.GetPatientHistoryAsync(patient.PatientId, From, To);
                History = history;
                Visits.Clear();
                foreach (var visit in history.Visits)
                {
                    Visits.Add(visit);
                }

                StatusMessage = $"تم تحميل {Visits.Count} زيارة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task PrintHistoryAsync()
        {
            if (History == null)
            {
                StatusMessage = "لا يوجد تاريخ مرضي للطباعة.";
                return;
            }

            try
            {
                await _printService.PrintPatientHistoryAsync(History);
                StatusMessage = "تم إرسال التاريخ المرضي للطباعة عبر Microsoft Print to PDF.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ طباعة: {ex.Message}";
            }
        }
    }
}
