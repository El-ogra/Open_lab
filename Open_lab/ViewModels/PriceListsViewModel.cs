using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class PriceListsViewModel : BaseViewModel
    {
        private readonly ITestCatalogService _testCatalogService;
        private string _listName = string.Empty;
        private bool _isDefault;
        private PriceList? _selectedPriceList;
        private Test? _selectedTest;
        private decimal _price;
        private string _statusMessage = string.Empty;

        public PriceListsViewModel(ITestCatalogService testCatalogService)
        {
            _testCatalogService = testCatalogService;
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
            var lists = await _testCatalogService.GetPriceListsAsync();
            PriceLists.Clear();
            foreach (var list in lists)
            {
                PriceLists.Add(list);
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
            Items.Clear();
            if (SelectedPriceList == null)
            {
                return;
            }

            var items = await _testCatalogService.GetPriceListItemsAsync(SelectedPriceList.PriceListId);
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
                var list = await _testCatalogService.CreatePriceListAsync(new PriceList
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
                var item = await _testCatalogService.AddPriceListItemAsync(new PriceListItem
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
                await _testCatalogService.DeletePriceListItemAsync(SelectedItem.PriceListItemId);
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
