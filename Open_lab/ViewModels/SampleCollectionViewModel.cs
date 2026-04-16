using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class SampleCollectionViewModel : BaseViewModel
    {
        private readonly ISampleCollectionService _sampleCollectionService;
        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private SampleCollectionRow? _selectedRow;
        private string _statusMessage = string.Empty;

        public SampleCollectionViewModel(ISampleCollectionService sampleCollectionService)
        {
            _sampleCollectionService = sampleCollectionService;
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
                var from = DateFrom.Date;
                var to = DateTo.Date.AddDays(1).AddSeconds(-1);
                var rows = await _sampleCollectionService.GetRowsAsync(from, to);

                Items.Clear();
                foreach (var row in rows)
                {
                    Items.Add(row);
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
                await _sampleCollectionService.MarkCollectedAsync(SelectedRow.VisitTestId, AppSession.UserId);
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
                await _sampleCollectionService.MarkNotCollectedAsync(SelectedRow.VisitTestId);
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
