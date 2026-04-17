using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class DeliveryViewModel : BaseViewModel
    {
        private readonly IDeliveryService _deliveryService;
        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private string _keyword = string.Empty;
        private DeliveryVisitRow? _selectedVisit;
        private string _statusMessage = string.Empty;

        public DeliveryViewModel(IDeliveryService deliveryService)
        {
            _deliveryService = deliveryService;
            Visits = new ObservableCollection<DeliveryVisitRow>();

            SearchCommand = new RelayCommand(async _ => await LoadAsync(), _ => AppSession.HasPermission(PermissionCodes.DeliveryView));
            DeliverCommand = new RelayCommand(async _ => await DeliverAsync(), _ => CanDeliver());
            ReopenCommand = new RelayCommand(async _ => await ReopenAsync(), _ => CanReopen());
        }

        public ObservableCollection<DeliveryVisitRow> Visits { get; }

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

        public string Keyword
        {
            get => _keyword;
            set => SetProperty(ref _keyword, value);
        }

        public DeliveryVisitRow? SelectedVisit
        {
            get => _selectedVisit;
            set
            {
                if (SetProperty(ref _selectedVisit, value))
                {
                    RaiseActionsState();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand SearchCommand { get; }
        public ICommand DeliverCommand { get; }
        public ICommand ReopenCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                var rows = await _deliveryService.SearchAsync(DateFrom, DateTo.AddDays(1).AddSeconds(-1), Keyword);
                Visits.Clear();
                foreach (var row in rows)
                {
                    Visits.Add(row);
                }

                StatusMessage = $"تم تحميل {Visits.Count} زيارة.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
            finally
            {
                RaiseActionsState();
            }
        }

        private async Task DeliverAsync()
        {
            if (SelectedVisit == null)
            {
                return;
            }

            try
            {
                await _deliveryService.DeliverAsync(SelectedVisit.VisitId, AppSession.UserId > 0 ? AppSession.UserId : 1);
                await LoadAsync();
                StatusMessage = "تم تسليم الزيارة بنجاح.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task ReopenAsync()
        {
            if (SelectedVisit == null)
            {
                return;
            }

            try
            {
                await _deliveryService.ReopenDeliveryAsync(SelectedVisit.VisitId);
                await LoadAsync();
                StatusMessage = "تم إعادة فتح التسليم.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private bool CanDeliver()
        {
            return AppSession.HasPermission(PermissionCodes.DeliveryEdit)
                && SelectedVisit != null
                && SelectedVisit.IsReadyForDelivery
                && !SelectedVisit.IsDelivered;
        }

        private bool CanReopen()
        {
            return AppSession.HasPermission(PermissionCodes.DeliveryEdit)
                && SelectedVisit != null
                && SelectedVisit.IsDelivered;
        }

        private void RaiseActionsState()
        {
            (SearchCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (DeliverCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ReopenCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
