using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class CombinedReportViewModel : BaseViewModel
    {
        private readonly IReportService _reportService;
        private int _visitId;
        private string _patientName = string.Empty;
        private string _labId = string.Empty;
        private string _visitDate = string.Empty;
        private VisitTestReportItem? _selectedTest;
        private string _statusMessage = string.Empty;

        public CombinedReportViewModel(IReportService reportService)
        {
            _reportService = reportService;
            Tests = new ObservableCollection<VisitTestReportItem>();
            LoadCommand = new RelayCommand(async _ => await LoadAsync());
            MoveUpCommand = new RelayCommand(_ => MoveUp(), _ => SelectedTest != null && Tests.IndexOf(SelectedTest) > 0);
            MoveDownCommand = new RelayCommand(_ => MoveDown(), _ => SelectedTest != null && Tests.IndexOf(SelectedTest) < Tests.Count - 1);
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

        public VisitTestReportItem? SelectedTest
        {
            get => _selectedTest;
            set
            {
                if (SetProperty(ref _selectedTest, value))
                {
                    (MoveUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (MoveDownCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<VisitTestReportItem> Tests { get; }

        public ICommand LoadCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }

        private async Task LoadAsync()
        {
            if (VisitId <= 0)
            {
                StatusMessage = "يرجى إدخال رقم الزيارة.";
                return;
            }

            try
            {
                var report = await _reportService.GetCompositeReportAsync(VisitId);
                if (report == null)
                {
                    StatusMessage = "لم يتم العثور على تقرير.";
                    return;
                }

                PatientName = report.Patient.FullName;
                LabId = report.Patient.LabId;
                VisitDate = report.Visit.VisitDate.ToString("yyyy-MM-dd");

                Tests.Clear();
                foreach (var item in report.Tests)
                {
                    Tests.Add(item);
                }

                StatusMessage = $"تم تحميل {Tests.Count} تحليل.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private void MoveUp()
        {
            if (SelectedTest == null)
            {
                return;
            }

            var index = Tests.IndexOf(SelectedTest);
            if (index > 0)
            {
                Tests.Move(index, index - 1);
                (MoveUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (MoveDownCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        private void MoveDown()
        {
            if (SelectedTest == null)
            {
                return;
            }

            var index = Tests.IndexOf(SelectedTest);
            if (index < Tests.Count - 1)
            {
                Tests.Move(index, index + 1);
                (MoveUpCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (MoveDownCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }
}
