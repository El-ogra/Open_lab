using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private int _patientCount;
        private int _visitCount;
        private int _testCount;

        public DashboardViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
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
            using var db = _dbFactory();
            PatientCount = await db.Patients.CountAsync();
            VisitCount = await db.Visits.CountAsync();
            TestCount = await db.Tests.CountAsync();
        }
    }
}
