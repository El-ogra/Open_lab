using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.ViewModels
{
    public class WorkSheetByPatientViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private DateTime _from = DateTime.Today;
        private DateTime _to = DateTime.Today;
        private string _statusMessage = string.Empty;

        public WorkSheetByPatientViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            Rows = new ObservableCollection<WorkSheetPatientRow>();
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

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<WorkSheetPatientRow> Rows { get; }

        public ICommand LoadCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                using var db = _dbFactory();
                var from = From.Date;
                var to = To.Date.AddDays(1).AddSeconds(-1);

                var rows = await db.Visits
                    .AsNoTracking()
                    .Where(v => v.VisitDate >= from && v.VisitDate <= to)
                    .Select(v => new WorkSheetPatientRow
                    {
                        VisitId = v.VisitId,
                        PatientName = v.Patient.FullName,
                        VisitDate = v.VisitDate,
                        TestsCount = v.VisitTests.Count
                    })
                    .OrderBy(v => v.VisitDate)
                    .ToListAsync();

                Rows.Clear();
                foreach (var row in rows)
                {
                    Rows.Add(row);
                }

                StatusMessage = $"تم تحميل {Rows.Count} زيارة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
