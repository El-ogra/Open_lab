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
        private string _separationType = "Centrifuge";
        private SampleCollectionRow? _selectedRow;
        private string _statusMessage = string.Empty;

        public SampleCollectionViewModel(ISampleCollectionService sampleCollectionService)
        {
            _sampleCollectionService = sampleCollectionService;
            Items = new ObservableCollection<SampleCollectionRow>();
            LoadCommand = new RelayCommand(async _ => await LoadAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsView));
            MarkCollectedCommand = new RelayCommand(async _ => await MarkCollectedAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit) && SelectedRow != null);
            MarkExternalCollectedCommand = new RelayCommand(async _ => await MarkExternalCollectedAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit) && SelectedRow != null);
            MarkSeparatedCommand = new RelayCommand(async _ => await MarkSeparatedAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit) && SelectedRow != null);
            MarkNotCollectedCommand = new RelayCommand(async _ => await MarkNotCollectedAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit) && SelectedRow != null);
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

        public string SeparationType
        {
            get => _separationType;
            set => SetProperty(ref _separationType, value);
        }

        public SampleCollectionRow? SelectedRow
        {
            get => _selectedRow;
            set
            {
                if (SetProperty(ref _selectedRow, value))
                {
                    (MarkCollectedCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (MarkExternalCollectedCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (MarkSeparatedCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
        public ICommand MarkExternalCollectedCommand { get; }
        public ICommand MarkSeparatedCommand { get; }
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

        private async Task MarkExternalCollectedAsync()
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
                await _sampleCollectionService.MarkCollectedAsync(SelectedRow.VisitTestId, AppSession.UserId, true, AppSession.UserId);
                StatusMessage = "تم تعليم العينة كخارجية.";
                await LoadAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task MarkSeparatedAsync()
        {
            if (SelectedRow == null)
            {
                return;
            }

            try
            {
                await _sampleCollectionService.MarkSeparatedAsync(SelectedRow.VisitTestId, SeparationType);
                StatusMessage = "تم تسجيل فصل العينة.";
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
