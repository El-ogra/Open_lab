using System.Threading.Tasks;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IDashboardService _dashboardService;
        private int _patientCount;
        private int _visitCount;
        private int _testCount;

        public DashboardViewModel(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
            _ = LoadAsync();
        }

        public int PatientCount
        {
            get => _patientCount;
            private set => SetProperty(ref _patientCount, value);
        }

        public int VisitCount
        {
            get => _visitCount;
            private set => SetProperty(ref _visitCount, value);
        }

        public int TestCount
        {
            get => _testCount;
            private set => SetProperty(ref _testCount, value);
        }

        private async Task LoadAsync()
        {
            var counts = await _dashboardService.GetCountsAsync();
            PatientCount = counts.PatientCount;
            VisitCount = counts.VisitCount;
            TestCount = counts.TestCount;
        }
    }
}
