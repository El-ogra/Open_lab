using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class CustomGroupsViewModel : BaseViewModel
    {
        private readonly ITestCatalogService _testCatalogService;
        private string _groupName = string.Empty;
        private decimal _groupPrice;
        private CustomGroup? _selectedGroup;
        private Test? _selectedTest;
        private string _statusMessage = string.Empty;

        public CustomGroupsViewModel(ITestCatalogService testCatalogService)
        {
            _testCatalogService = testCatalogService;
            Groups = new ObservableCollection<CustomGroup>();
            GroupItems = new ObservableCollection<CustomGroupItem>();
            Tests = new ObservableCollection<Test>();

            SaveGroupCommand = new RelayCommand(async _ => await SaveGroupAsync());
            AddItemCommand = new RelayCommand(async _ => await AddItemAsync(), _ => SelectedGroup != null && SelectedTest != null);
            DeleteItemCommand = new RelayCommand(async _ => await DeleteItemAsync(), _ => SelectedItem != null);

            _ = LoadAsync();
        }

        public ObservableCollection<CustomGroup> Groups { get; }
        public ObservableCollection<CustomGroupItem> GroupItems { get; }
        public ObservableCollection<Test> Tests { get; }

        public CustomGroupItem? SelectedItem { get; set; }

        public CustomGroup? SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (SetProperty(ref _selectedGroup, value))
                {
                    _ = LoadItemsAsync();
                }
            }
        }

        public Test? SelectedTest
        {
            get => _selectedTest;
            set => SetProperty(ref _selectedTest, value);
        }

        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        public decimal GroupPrice
        {
            get => _groupPrice;
            set => SetProperty(ref _groupPrice, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand SaveGroupCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand DeleteItemCommand { get; }

        private async Task LoadAsync()
        {
            var groups = await _testCatalogService.GetCustomGroupsAsync();
            Groups.Clear();
            foreach (var group in groups)
            {
                Groups.Add(group);
            }

            var tests = await _testCatalogService.GetAllTestsAsync();
            Tests.Clear();
            foreach (var test in tests)
            {
                Tests.Add(test);
            }
        }

        private async Task LoadItemsAsync()
        {
            GroupItems.Clear();
            if (SelectedGroup == null)
            {
                return;
            }

            var items = await _testCatalogService.GetCustomGroupItemsAsync(SelectedGroup.CustomGroupId);
            foreach (var item in items)
            {
                GroupItems.Add(item);
            }
        }

        private async Task SaveGroupAsync()
        {
            if (string.IsNullOrWhiteSpace(GroupName))
            {
                StatusMessage = "أدخل اسم المجموعة.";
                return;
            }

            try
            {
                var group = await _testCatalogService.CreateCustomGroupAsync(new CustomGroup
                {
                    Name = GroupName,
                    Price = GroupPrice
                });

                Groups.Add(group);
                StatusMessage = "تم إنشاء المجموعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task AddItemAsync()
        {
            if (SelectedGroup == null || SelectedTest == null)
            {
                return;
            }

            try
            {
                var item = await _testCatalogService.AddCustomGroupItemAsync(new CustomGroupItem
                {
                    CustomGroupId = SelectedGroup.CustomGroupId,
                    TestId = SelectedTest.TestId
                });

                item.Test = SelectedTest;
                GroupItems.Add(item);
                StatusMessage = "تمت إضافة التحليل.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task DeleteItemAsync()
        {
            if (SelectedItem == null)
            {
                return;
            }

            try
            {
                await _testCatalogService.DeleteCustomGroupItemAsync(SelectedItem.CustomGroupItemId);
                GroupItems.Remove(SelectedItem);
                SelectedItem = null;
                StatusMessage = "تم حذف العنصر.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
