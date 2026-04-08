using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private DateTime _from = DateTime.Today.AddDays(-30);
        private DateTime _to = DateTime.Today;
        private int _visitCount;
        private int _patientCount;
        private int _testCount;
        private decimal _totalRevenue;
        private decimal _totalPaid;
        private string _statusMessage = string.Empty;

        public StatisticsViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
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

            using var db = _dbFactory();
            VisitCount = await db.Visits.CountAsync(v => v.VisitDate >= from && v.VisitDate <= to);
            PatientCount = await db.Patients.CountAsync();
            TestCount = await db.VisitTests.CountAsync(vt => vt.Visit.VisitDate >= from && vt.Visit.VisitDate <= to);

            TotalRevenue = await db.Invoices
                .Where(i => i.Visit.VisitDate >= from && i.Visit.VisitDate <= to)
                .SumAsync(i => (decimal?)i.NetTotal) ?? 0;
            TotalPaid = await db.Invoices
                .Where(i => i.Visit.VisitDate >= from && i.Visit.VisitDate <= to)
                .SumAsync(i => (decimal?)i.Paid) ?? 0;

            StatusMessage = "تم تحميل الإحصائيات.";
        }
    }
}
