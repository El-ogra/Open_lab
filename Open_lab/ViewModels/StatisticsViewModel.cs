using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly IStatisticsService _statisticsService;
        private DateTime _from = DateTime.Today.AddDays(-30);
        private DateTime _to = DateTime.Today;
        private int _visitCount;
        private int _patientCount;
        private int _testCount;
        private decimal _totalRevenue;
        private decimal _totalPaid;
        private string _statusMessage = string.Empty;

        public StatisticsViewModel(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
            LoadCommand = new RelayCommand(async _ => await LoadAsync());
        }

        public DateTime From
        {
            get => _from;
            set => SetProperty(ref _from, value);
        }

        public DateTime To
        {
            get => _to;
            set => SetProperty(ref _to, value);
        }

        public int VisitCount
        {
            get => _visitCount;
            private set => SetProperty(ref _visitCount, value);
        }

        public int PatientCount
        {
            get => _patientCount;
            private set => SetProperty(ref _patientCount, value);
        }

        public int TestCount
        {
            get => _testCount;
            private set => SetProperty(ref _testCount, value);
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            private set => SetProperty(ref _totalRevenue, value);
        }

        public decimal TotalPaid
        {
            get => _totalPaid;
            private set => SetProperty(ref _totalPaid, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadCommand { get; }

        private async Task LoadAsync()
        {
            var from = From.Date;
            var to = To.Date.AddDays(1).AddSeconds(-1);
            var summary = await _statisticsService.GetSummaryAsync(from, to);

            VisitCount = summary.VisitCount;
            PatientCount = summary.PatientCount;
            TestCount = summary.TestCount;
            TotalRevenue = summary.TotalRevenue;
            TotalPaid = summary.TotalPaid;

            StatusMessage = "تم تحميل الإحصائيات.";
        }
    }
}
