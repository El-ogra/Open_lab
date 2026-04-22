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
        private readonly ISampleTrackingService? _sampleTrackingService;
        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private string _separationType = "Centrifuge";
        private SampleCollectionRow? _selectedRow;
        private string _selectedSampleStatus = string.Empty;
        private string _statusMessage = string.Empty;

        public SampleCollectionViewModel(
            ISampleCollectionService sampleCollectionService,
            ISampleTrackingService? sampleTrackingService = null)
        {
            _sampleCollectionService = sampleCollectionService;
            _sampleTrackingService = sampleTrackingService;
            Items = new ObservableCollection<SampleCollectionRow>();
            LoadCommand = new RelayCommand(async _ => await LoadAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsView));
            MarkCollectedCommand = new RelayCommand(async _ => await MarkCollectedAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit) && SelectedRow != null);
            MarkExternalCollectedCommand = new RelayCommand(async _ => await MarkExternalCollectedAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit) && SelectedRow != null);
            MarkSeparatedCommand = new RelayCommand(async _ => await MarkSeparatedAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit) && SelectedRow != null);
            MarkNotCollectedCommand = new RelayCommand(async _ => await MarkNotCollectedAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit) && SelectedRow != null);
            RefreshSampleStatusCommand = new RelayCommand(async _ => await RefreshSampleStatusAsync(), _ => SelectedRow != null && _sampleTrackingService != null);
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
                    (RefreshSampleStatusCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    _ = RefreshSampleStatusAsync();
                }
            }
        }

        public string SelectedSampleStatus
        {
            get => _selectedSampleStatus;
            private set => SetProperty(ref _selectedSampleStatus, value);
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
        public ICommand RefreshSampleStatusCommand { get; }

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
                SelectedSampleStatus = string.Empty;
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task RefreshSampleStatusAsync()
        {
            if (_sampleTrackingService == null || SelectedRow == null)
            {
                SelectedSampleStatus = string.Empty;
                return;
            }

            try
            {
                var sample = await _sampleTrackingService.GetSampleStatusAsync(SelectedRow.VisitTestId);
                if (sample == null)
                {
                    SelectedSampleStatus = "لا يوجد سجل تتبع لهذه العينة.";
                    return;
                }

                var separation = sample.IsSeparated ? "مفصولة" : "غير مفصولة";
                var status = string.IsNullOrWhiteSpace(sample.Status) ? "غير محدد" : sample.Status;
                SelectedSampleStatus = $"الحالة: {status} | الفصل: {separation}";
            }
            catch (Exception ex)
            {
                SelectedSampleStatus = "خطأ تتبع: " + ex.Message;
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
