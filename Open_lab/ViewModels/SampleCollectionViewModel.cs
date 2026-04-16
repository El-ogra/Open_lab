using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;

namespace Open_lab.ViewModels
{
    public class SampleCollectionViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private SampleCollectionRow? _selectedRow;
        private string _statusMessage = string.Empty;

        public SampleCollectionViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            Items = new ObservableCollection<SampleCollectionRow>();
            LoadCommand = new RelayCommand(async _ => await LoadAsync());
            MarkCollectedCommand = new RelayCommand(async _ => await MarkCollectedAsync(), _ => SelectedRow != null);
            MarkNotCollectedCommand = new RelayCommand(async _ => await MarkNotCollectedAsync(), _ => SelectedRow != null);
        }

        public DateTime DateFrom
        {
            get => _dateFrom;
            set => SetProperty(ref _dateFrom, value);
        }

        public DateTime DateTo
        {
            get => _dateTo;
            set => SetProperty(ref _dateTo, value);
        }

        public ObservableCollection<SampleCollectionRow> Items { get; }

        public SampleCollectionRow? SelectedRow
        {
            get => _selectedRow;
            set
            {
                if (SetProperty(ref _selectedRow, value))
                {
                    (MarkCollectedCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (MarkNotCollectedCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand MarkCollectedCommand { get; }
        public ICommand MarkNotCollectedCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                using var db = _dbFactory();
                var from = DateFrom.Date;
                var to = DateTo.Date.AddDays(1).AddSeconds(-1);

                var visitTests = await db.VisitTests
                    .Include(vt => vt.Visit)
                    .ThenInclude(v => v.Patient)
                    .Include(vt => vt.Test)
                    .Include(vt => vt.SampleCollection)
                    .ThenInclude(sc => sc!.CollectedByUser)
                    .Where(vt => vt.Visit.VisitDate >= from && vt.Visit.VisitDate <= to)
                    .OrderByDescending(vt => vt.Visit.VisitDate)
                    .ToListAsync();

                Items.Clear();
                foreach (var vt in visitTests)
                {
                    var sc = vt.SampleCollection;
                    Items.Add(new SampleCollectionRow
                    {
                        VisitTestId = vt.VisitTestId,
                        PatientName = vt.Visit.Patient.FullName,
                        TestName = vt.Test.NameReport,
                        VisitDate = vt.Visit.VisitDate,
                        Status = sc?.Status ?? "غير مسحوبة",
                        CollectedAt = sc?.CollectedAt,
                        CollectedBy = sc?.CollectedByUser?.Username
                    });
                }

                StatusMessage = "تم تحميل " + Items.Count + " تحليل.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task MarkCollectedAsync()
        {
            if (SelectedRow == null)
            {
                return;
            }

            if (AppSession.UserId <= 0)
            {
                StatusMessage = "يجب تسجيل الدخول.";
                return;
            }

            try
            {
                using var db = _dbFactory();
                var vt = await db.VisitTests
                    .Include(v => v.SampleCollection)
                    .FirstOrDefaultAsync(v => v.VisitTestId == SelectedRow.VisitTestId);

                if (vt == null)
                {
                    StatusMessage = "العنصر غير موجود.";
                    return;
                }

                if (vt.SampleCollection == null)
                {
                    vt.SampleCollection = new SampleCollection
                    {
                        VisitTestId = vt.VisitTestId,
                        CollectedBy = AppSession.UserId,
                        CollectedAt = DateTime.Now,
                        Status = "مسحوبة"
                    };
                    db.SampleCollections.Add(vt.SampleCollection);
                }
                else
                {
                    vt.SampleCollection.CollectedBy = AppSession.UserId;
                    vt.SampleCollection.CollectedAt = DateTime.Now;
                    vt.SampleCollection.Status = "مسحوبة";
                }

                await db.SaveChangesAsync();
                StatusMessage = "تم تحديث حالة العينة.";
                await LoadAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task MarkNotCollectedAsync()
        {
            if (SelectedRow == null)
            {
                return;
            }

            try
            {
                using var db = _dbFactory();
                var sample = await db.SampleCollections
                    .FirstOrDefaultAsync(s => s.VisitTestId == SelectedRow.VisitTestId);

                if (sample != null)
                {
                    db.SampleCollections.Remove(sample);
                    await db.SaveChangesAsync();
                }

                StatusMessage = "تم تحديث الحالة.";
                await LoadAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }
    }
}

