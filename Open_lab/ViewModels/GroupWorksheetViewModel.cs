using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class GroupWorksheetViewModel : BaseViewModel
    {
        private readonly IGroupWorksheetService _groupWorksheetService;
        private readonly ITestCatalogService _testCatalogService;
        private readonly IPrintService _printService;
        private DateTime _from = DateTime.Today;
        private DateTime _to = DateTime.Today;
        private int? _selectedGroupId;
        private int? _selectedCustomGroupId;
        private bool _isCustomGroup;
        private string _statusMessage = string.Empty;

        public GroupWorksheetViewModel(IGroupWorksheetService groupWorksheetService, ITestCatalogService testCatalogService, IPrintService printService)
        {
            _groupWorksheetService = groupWorksheetService;
            _testCatalogService = testCatalogService;
            _printService = printService;
            Groups = new ObservableCollection<TestGroupItem>();
            CustomGroups = new ObservableCollection<CustomGroupListItem>();
            Rows = new ObservableCollection<WorkSheetPatientRow>();

            LoadGroupsCommand = new RelayCommand(async _ => await LoadGroupsAsync());
            LoadWorksheetCommand = new RelayCommand(async _ => await LoadWorksheetAsync(), _ => CanLoadWorksheet());
            PrintCommand = new RelayCommand(async _ => await PrintAsync(), _ => Rows.Count > 0);

            _ = LoadGroupsAsync();
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

        public bool IsCustomGroup
        {
            get => _isCustomGroup;
            set
            {
                if (SetProperty(ref _isCustomGroup, value))
                {
                    _selectedGroupId = null;
                    _selectedCustomGroupId = null;
                    OnPropertyChanged(nameof(SelectedGroupId));
                    OnPropertyChanged(nameof(SelectedCustomGroupId));
                    (LoadWorksheetCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public int? SelectedGroupId
        {
            get => _selectedGroupId;
            set
            {
                if (SetProperty(ref _selectedGroupId, value))
                {
                    (LoadWorksheetCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public int? SelectedCustomGroupId
        {
            get => _selectedCustomGroupId;
            set
            {
                if (SetProperty(ref _selectedCustomGroupId, value))
                {
                    (LoadWorksheetCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<TestGroupItem> Groups { get; }
        public ObservableCollection<CustomGroupListItem> CustomGroups { get; }
        public ObservableCollection<WorkSheetPatientRow> Rows { get; }

        public ICommand LoadGroupsCommand { get; }
        public ICommand LoadWorksheetCommand { get; }
        public ICommand PrintCommand { get; }

        private bool CanLoadWorksheet()
        {
            if (IsCustomGroup)
                return SelectedCustomGroupId.HasValue;
            return SelectedGroupId.HasValue;
        }

        private async Task LoadGroupsAsync()
        {
            try
            {
                var groups = await _testCatalogService.GetTestGroupsAsync();
                Groups.Clear();
                foreach (var group in groups)
                {
                    Groups.Add(new TestGroupItem { GroupId = group.GroupId, GroupName = group.GroupName });
                }

                var customGroups = await _testCatalogService.GetCustomGroupsAsync();
                CustomGroups.Clear();
                foreach (var cg in customGroups)
                {
                    CustomGroups.Add(new CustomGroupListItem { CustomGroupId = cg.CustomGroupId, Name = cg.Name });
                }

                StatusMessage = $"تم تحميل {Groups.Count} مجموعة و {CustomGroups.Count} مجموعة مخصصة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadWorksheetAsync()
        {
            try
            {
                var from = From.Date;
                var to = To.Date.AddDays(1).AddSeconds(-1);

                List<WorkSheetPatientRow> rows;
                if (IsCustomGroup && SelectedCustomGroupId.HasValue)
                {
                    rows = await _groupWorksheetService.GetGroupWorksheetByCustomGroupAsync(SelectedCustomGroupId.Value, from, to);
                }
                else if (SelectedGroupId.HasValue)
                {
                    rows = await _groupWorksheetService.GetGroupWorksheetByGroupAsync(SelectedGroupId.Value, from, to);
                }
                else
                {
                    return;
                }

                Rows.Clear();
                foreach (var row in rows)
                {
                    Rows.Add(row);
                }

                (PrintCommand as RelayCommand)?.RaiseCanExecuteChanged();
                StatusMessage = $"تم تحميل {Rows.Count} زيارة للمجموعة المحددة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task PrintAsync()
        {
            try
            {
                await _printService.PrintWorksheetByPatientAsync(From.Date, To.Date, Rows);
                StatusMessage = "تم إرسال ورقة العمل للطباعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ طباعة: {ex.Message}";
            }
        }
    }

    public class TestGroupItem
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
    }

    public class CustomGroupListItem
    {
        public int CustomGroupId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
