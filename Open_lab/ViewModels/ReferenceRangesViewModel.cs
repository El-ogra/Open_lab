using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class ReferenceRangesViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private Test? _selectedTest;
        private TestReferenceRange? _selectedRange;
        private string? _gender;
        private int? _ageFrom;
        private int? _ageTo;
        private decimal? _lowValue;
        private decimal? _highValue;
        private string? _normalText;
        private string _statusMessage = string.Empty;

        public ReferenceRangesViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            Tests = new ObservableCollection<Test>();
            Ranges = new ObservableCollection<TestReferenceRange>();

            LoadCommand = new RelayCommand(async _ => await LoadRangesAsync());
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedRange != null);

            _ = LoadTestsAsync();
        }

        public ObservableCollection<Test> Tests { get; }
        public ObservableCollection<TestReferenceRange> Ranges { get; }

        public Test? SelectedTest
        {
            get => _selectedTest;
            set
            {
                if (SetProperty(ref _selectedTest, value))
                {
                    _ = LoadRangesAsync();
                }
            }
        }

        public TestReferenceRange? SelectedRange
        {
            get => _selectedRange;
            set
            {
                if (SetProperty(ref _selectedRange, value))
                {
                    LoadFromRange();
                    (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string? Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }

        public int? AgeFrom
        {
            get => _ageFrom;
            set => SetProperty(ref _ageFrom, value);
        }

        public int? AgeTo
        {
            get => _ageTo;
            set => SetProperty(ref _ageTo, value);
        }

        public decimal? LowValue
        {
            get => _lowValue;
            set => SetProperty(ref _lowValue, value);
        }

        public decimal? HighValue
        {
            get => _highValue;
            set => SetProperty(ref _highValue, value);
        }

        public string? NormalText
        {
            get => _normalText;
            set => SetProperty(ref _normalText, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }

        private async Task LoadTestsAsync()
        {
            using var db = _dbFactory();
            var service = new TestCatalogService(db);
            var tests = await service.GetAllTestsAsync();
            Tests.Clear();
            foreach (var test in tests)
            {
                Tests.Add(test);
            }
        }

        private async Task LoadRangesAsync()
        {
            Ranges.Clear();
            if (SelectedTest == null)
            {
                return;
            }

            using var db = _dbFactory();
            var ranges = await db.TestReferenceRanges.AsNoTracking()
                .Where(r => r.TestId == SelectedTest.TestId)
                .ToListAsync();

            foreach (var range in ranges)
            {
                Ranges.Add(range);
            }
        }

        private void LoadFromRange()
        {
            if (SelectedRange == null)
            {
                return;
            }

            Gender = SelectedRange.Gender;
            AgeFrom = SelectedRange.AgeFrom;
            AgeTo = SelectedRange.AgeTo;
            LowValue = SelectedRange.LowValue;
            HighValue = SelectedRange.HighValue;
            NormalText = SelectedRange.NormalText;
        }

        private async Task SaveAsync()
        {
            if (SelectedTest == null)
            {
                StatusMessage = "اختر تحليلًا.";
                return;
            }

            try
            {
                using var db = _dbFactory();

                if (SelectedRange == null || SelectedRange.RangeId == 0)
                {
                    var range = new TestReferenceRange
                    {
                        TestId = SelectedTest.TestId,
                        Gender = Gender,
                        AgeFrom = AgeFrom,
                        AgeTo = AgeTo,
                        LowValue = LowValue,
                        HighValue = HighValue,
                        NormalText = NormalText
                    };

                    db.TestReferenceRanges.Add(range);
                    await db.SaveChangesAsync();
                    Ranges.Add(range);
                }
                else
                {
                    var range = await db.TestReferenceRanges.FirstAsync(r => r.RangeId == SelectedRange.RangeId);
                    range.Gender = Gender;
                    range.AgeFrom = AgeFrom;
                    range.AgeTo = AgeTo;
                    range.LowValue = LowValue;
                    range.HighValue = HighValue;
                    range.NormalText = NormalText;
                    await db.SaveChangesAsync();
                }

                StatusMessage = "تم حفظ النطاق المرجعي.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedRange == null)
            {
                return;
            }

            try
            {
                using var db = _dbFactory();
                var range = await db.TestReferenceRanges.FirstAsync(r => r.RangeId == SelectedRange.RangeId);
                db.TestReferenceRanges.Remove(range);
                await db.SaveChangesAsync();
                Ranges.Remove(SelectedRange);
                SelectedRange = null;
                StatusMessage = "تم حذف النطاق.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
