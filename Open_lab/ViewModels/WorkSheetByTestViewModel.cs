using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;

namespace Open_lab.ViewModels
{
    public class WorkSheetByTestViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private DateTime _from = DateTime.Today;
        private DateTime _to = DateTime.Today;
        private string _statusMessage = string.Empty;

        public WorkSheetByTestViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            Rows = new ObservableCollection<WorkSheetTestRow>();
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

        public ObservableCollection<WorkSheetTestRow> Rows { get; }

        public ICommand LoadCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                using var db = _dbFactory();
                var from = From.Date;
                var to = To.Date.AddDays(1).AddSeconds(-1);

                var rows = await db.VisitTests
                    .AsNoTracking()
                    .Where(vt => vt.Visit.VisitDate >= from && vt.Visit.VisitDate <= to)
                    .GroupBy(vt => vt.Test.NameReport)
                    .Select(g => new WorkSheetTestRow
                    {
                        TestName = g.Key,
                        Count = g.Count()
                    })
                    .OrderBy(r => r.TestName)
                    .ToListAsync();

                Rows.Clear();
                foreach (var row in rows)
                {
                    Rows.Add(row);
                }

                StatusMessage = $"تم تحميل {Rows.Count} تحليل.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
