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
    public class PriceListsViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private string _listName = string.Empty;
        private bool _isDefault;
        private PriceList? _selectedPriceList;
        private Test? _selectedTest;
        private decimal _price;
        private string _statusMessage = string.Empty;

        public PriceListsViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
            PriceLists = new ObservableCollection<PriceList>();
            Items = new ObservableCollection<PriceListItem>();
            Tests = new ObservableCollection<Test>();

            LoadCommand = new RelayCommand(async _ => await LoadAsync());
            SaveListCommand = new RelayCommand(async _ => await SaveListAsync());
            AddItemCommand = new RelayCommand(async _ => await AddItemAsync(), _ => SelectedPriceList != null && SelectedTest != null);
            DeleteItemCommand = new RelayCommand(async _ => await DeleteItemAsync(), _ => SelectedItem != null);

            _ = LoadAsync();
        }

        public ObservableCollection<PriceList> PriceLists { get; }
        public ObservableCollection<PriceListItem> Items { get; }
        public ObservableCollection<Test> Tests { get; }

        public PriceListItem? SelectedItem { get; set; }

        public PriceList? SelectedPriceList
        {
            get => _selectedPriceList;
            set
            {
                if (SetProperty(ref _selectedPriceList, value))
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

        public string ListName
        {
            get => _listName;
            set => SetProperty(ref _listName, value);
        }

        public bool IsDefault
        {
            get => _isDefault;
            set => SetProperty(ref _isDefault, value);
        }

        public decimal Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand SaveListCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand DeleteItemCommand { get; }

        private async Task LoadAsync()
        {
            using var db = _dbFactory();
            var service = new TestCatalogService(db);

            var lists = await service.GetPriceListsAsync();
            PriceLists.Clear();
            foreach (var list in lists)
            {
                PriceLists.Add(list);
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
            Items.Clear();
            if (SelectedPriceList == null)
            {
                return;
            }

            using var db = _dbFactory();
            var items = await db.PriceListItems.AsNoTracking()
                .Include(i => i.Test)
                .Where(i => i.PriceListId == SelectedPriceList.PriceListId)
                .ToListAsync();

            foreach (var item in items)
            {
                Items.Add(item);
            }
        }

        private async Task SaveListAsync()
        {
            if (string.IsNullOrWhiteSpace(ListName))
            {
                StatusMessage = "أدخل اسم القائمة.";
                return;
            }

            try
            {
                using var db = _dbFactory();
                var service = new TestCatalogService(db);
                var list = await service.CreatePriceListAsync(new PriceList
                {
                    Name = ListName,
                    IsDefault = IsDefault
                });

                PriceLists.Add(list);
                StatusMessage = "تم إنشاء القائمة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task AddItemAsync()
        {
            if (SelectedPriceList == null || SelectedTest == null)
            {
                return;
            }

            try
            {
                using var db = _dbFactory();
                var service = new TestCatalogService(db);
                var item = await service.AddPriceListItemAsync(new PriceListItem
                {
                    PriceListId = SelectedPriceList.PriceListId,
                    TestId = SelectedTest.TestId,
                    Price = Price
                });

                item.Test = SelectedTest;
                Items.Add(item);
                StatusMessage = "تمت إضافة التحليل للقائمة.";
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
                var item = await db.PriceListItems.FirstAsync(i => i.PriceListItemId == SelectedItem.PriceListItemId);
                db.PriceListItems.Remove(item);
                await db.SaveChangesAsync();
                Items.Remove(SelectedItem);
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
