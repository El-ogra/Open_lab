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
    public class CustomGroupsViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private string _groupName = string.Empty;
        private decimal _groupPrice;
        private CustomGroup? _selectedGroup;
        private Test? _selectedTest;
        private string _statusMessage = string.Empty;

        public CustomGroupsViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
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
            using var db = _dbFactory();
            var service = new TestCatalogService(db);

            var groups = await service.GetCustomGroupsAsync();
            Groups.Clear();
            foreach (var group in groups)
            {
                Groups.Add(group);
            }

            var tests = await service.GetAllTestsAsync();
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

            using var db = _dbFactory();
            var items = await db.CustomGroupItems.AsNoTracking()
                .Include(i => i.Test)
                .Where(i => i.CustomGroupId == SelectedGroup.CustomGroupId)
                .ToListAsync();

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
                using var db = _dbFactory();
                var service = new TestCatalogService(db);
                var group = await service.CreateCustomGroupAsync(new CustomGroup
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
                using var db = _dbFactory();
                var service = new TestCatalogService(db);
                var item = await service.AddCustomGroupItemAsync(new CustomGroupItem
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
                using var db = _dbFactory();
                var item = await db.CustomGroupItems.FirstAsync(i => i.CustomGroupItemId == SelectedItem.CustomGroupItemId);
                db.CustomGroupItems.Remove(item);
                await db.SaveChangesAsync();
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
