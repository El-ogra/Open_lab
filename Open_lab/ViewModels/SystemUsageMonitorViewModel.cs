using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class SystemUsageMonitorViewModel : BaseViewModel
    {
        private readonly ISystemMonitorService _systemMonitorService;
        private string _statusMessage = string.Empty;

        public SystemUsageMonitorViewModel(ISystemMonitorService systemMonitorService)
        {
            _systemMonitorService = systemMonitorService;
            Sessions = new ObservableCollection<ActiveSessionRow>();
            RefreshCommand = new RelayCommand(async _ => await RefreshAsync(), _ => AppSession.HasPermission(PermissionCodes.UsersView));
            _ = RefreshAsync();
        }

        public ObservableCollection<ActiveSessionRow> Sessions { get; }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand RefreshCommand { get; }

        private async Task RefreshAsync()
        {
            try
            {
                var sessions = await _systemMonitorService.GetActiveSessionsAsync();
                Sessions.Clear();
                foreach (var session in sessions)
                {
                    Sessions.Add(session);
                }

                StatusMessage = $"عدد المستخدمين النشطين حالياً: {Sessions.Count}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
