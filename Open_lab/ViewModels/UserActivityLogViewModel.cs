using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class UserActivityLogViewModel : BaseViewModel
    {
        private readonly IUserActivityService _userActivityService;
        private int _maxCount = 100;
        private string _statusMessage = string.Empty;

        public UserActivityLogViewModel(IUserActivityService userActivityService)
        {
            _userActivityService = userActivityService;
            Items = new ObservableCollection<UserActivityRow>();
            LoadCommand = new RelayCommand(async _ => await LoadAsync(), _ => AppSession.HasPermission(PermissionCodes.UsersView));
            _ = LoadAsync();
        }

        public int MaxCount
        {
            get => _maxCount;
            set => SetProperty(ref _maxCount, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<UserActivityRow> Items { get; }

        public ICommand LoadCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                var count = MaxCount <= 0 ? 100 : MaxCount;
                var rows = await _userActivityService.GetRecentActivitiesAsync(null, count);

                Items.Clear();
                foreach (var row in rows)
                {
                    Items.Add(row);
                }

                StatusMessage = $"تم تحميل {Items.Count} سجل نشاط.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
