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
        private readonly IPrintService _printService;
        private string _listName = string.Empty;
        private bool _isDefault;
        private Referral? _selectedReferral;
        private PriceList? _selectedPriceList;
        private Test? _selectedTest;
        private PriceListItem? _selectedItem;
        private decimal _price;
        private string _statusMessage = string.Empty;

        public PriceListsViewModel(ITestCatalogService testCatalogService, IPrintService printService)
        {
            _testCatalogService = testCatalogService;
            _printService = printService;
            PriceLists = new ObservableCollection<PriceList>();
            Items = new ObservableCollection<PriceListItem>();
            Tests = new ObservableCollection<Test>();
            Referrals = new ObservableCollection<Referral>();

            LoadCommand = new RelayCommand(async _ => await LoadAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.TestsView));
            SaveListCommand = new RelayCommand(async _ => await SaveListAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.TestsEdit));
            UpdateListCommand = new RelayCommand(async _ => await UpdateListAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.TestsEdit) && SelectedPriceList != null);
            AddItemCommand = new RelayCommand(async _ => await AddItemAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.TestsEdit) && SelectedPriceList != null && SelectedTest != null);
            UpdateItemCommand = new RelayCommand(async _ => await UpdateItemAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.TestsEdit) && SelectedItem != null);
            DeleteItemCommand = new RelayCommand(async _ => await DeleteItemAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.TestsEdit) && SelectedItem != null);
            PrintListCommand = new RelayCommand(async _ => await PrintListAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.TestsView) && SelectedPriceList != null && Items.Count > 0);

            _ = LoadAsync();
        }

        public ObservableCollection<PriceList> PriceLists { get; }
        public ObservableCollection<PriceListItem> Items { get; }
        public ObservableCollection<Test> Tests { get; }
        public ObservableCollection<Referral> Referrals { get; }

        public PriceListItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SetProperty(ref _selectedItem, value))
                {
                    if (value != null)
                    {
                        SelectedTest = value.Test;
                        Price = value.Price;
                    }

                    (DeleteItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (UpdateItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public PriceList? SelectedPriceList
        {
            get => _selectedPriceList;
            set
            {
                if (SetProperty(ref _selectedPriceList, value))
                {
                    if (value != null)
                    {
                        ListName = value.Name;
                        IsDefault = value.IsDefault;
                        SelectedReferral = value.ReferralId.HasValue ? FindReferral(value.ReferralId.Value) : null;
                    }

                    (AddItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (UpdateListCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (PrintListCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    _ = LoadItemsAsync();
                }
            }
        }

        public Test? SelectedTest
        {
            get => _selectedTest;
            set
            {
                if (SetProperty(ref _selectedTest, value))
                {
                    if (value != null && SelectedItem == null)
                    {
                        Price = value.Price;
                    }

                    (AddItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public Referral? SelectedReferral
        {
            get => _selectedReferral;
            set => SetProperty(ref _selectedReferral, value);
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
        public ICommand UpdateListCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand UpdateItemCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand PrintListCommand { get; }

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

            var referrals = await _testCatalogService.GetReferralsAsync();
            Referrals.Clear();
            Referrals.Add(new Referral { ReferralId = 0, Name = "عام (بدون جهة)", ReferralType = "General" });
            foreach (var referral in referrals)
            {
                Referrals.Add(referral);
            }

            if (SelectedPriceList != null)
            {
                await LoadItemsAsync();
            }

            (PrintListCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private async Task LoadItemsAsync()
        {
            Items.Clear();
            if (SelectedPriceList == null)
            {
                (PrintListCommand as RelayCommand)?.RaiseCanExecuteChanged();
                return;
            }

            var items = await _testCatalogService.GetPriceListItemsAsync(SelectedPriceList.PriceListId);
            foreach (var item in items)
            {
                Items.Add(item);
            }

            (PrintListCommand as RelayCommand)?.RaiseCanExecuteChanged();
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
                    IsDefault = IsDefault,
                    ReferralId = NormalizeReferralId(SelectedReferral)
                });

                await LoadAsync();
                SelectedPriceList = list;
                StatusMessage = "تم حفظ قائمة الأسعار.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task UpdateListAsync()
        {
            if (SelectedPriceList == null)
            {
                StatusMessage = "يرجى تحديد قائمة لتحديثها.";
                return;
            }

            try
            {
                await _testCatalogService.UpdatePriceListAsync(new PriceList
                {
                    PriceListId = SelectedPriceList.PriceListId,
                    Name = ListName,
                    IsDefault = IsDefault,
                    ReferralId = NormalizeReferralId(SelectedReferral)
                });

                await LoadAsync();
                SelectedPriceList = FindPriceList(SelectedPriceList.PriceListId);
                StatusMessage = "تم تحديث قائمة الأسعار.";
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
                    Price = Price > 0 ? Price : SelectedTest.Price
                });

                item.Test = SelectedTest;
                Items.Add(item);
                (PrintListCommand as RelayCommand)?.RaiseCanExecuteChanged();
                StatusMessage = "تمت إضافة عنصر التسعير.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task UpdateItemAsync()
        {
            if (SelectedItem == null)
            {
                return;
            }

            try
            {
                await _testCatalogService.UpdatePriceListItemAsync(new PriceListItem
                {
                    PriceListItemId = SelectedItem.PriceListItemId,
                    Price = Price
                });

                SelectedItem.Price = Price;
                await LoadItemsAsync();
                StatusMessage = "تم تحديث السعر.";
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
                (PrintListCommand as RelayCommand)?.RaiseCanExecuteChanged();
                StatusMessage = "تم حذف عنصر التسعير.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task PrintListAsync()
        {
            if (SelectedPriceList == null)
            {
                return;
            }

            try
            {
                var lines = new ObservableCollection<string>
                {
                    $"اسم القائمة: {SelectedPriceList.Name}",
                    $"الجهة: {SelectedReferral?.Name ?? "عام (بدون جهة)"}",
                    $"افتراضية: {(SelectedPriceList.IsDefault ? "نعم" : "لا")}",
                    string.Empty,
                    "العناصر:"
                };

                foreach (var item in Items)
                {
                    var testName = item.Test?.NameReport ?? item.Test?.NameReceipt ?? $"Test#{item.TestId}";
                    lines.Add($"- {testName}: {item.Price:N2}");
                }

                await _printService.PrintTextReportAsync("قائمة الأسعار", lines, $"PriceList_{SelectedPriceList.PriceListId}");
                StatusMessage = "تم إرسال قائمة الأسعار للطباعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private int? NormalizeReferralId(Referral? referral)
        {
            if (referral == null || referral.ReferralId <= 0)
            {
                return null;
            }

            return referral.ReferralId;
        }

        private Referral? FindReferral(int referralId)
        {
            foreach (var referral in Referrals)
            {
                if (referral.ReferralId == referralId)
                {
                    return referral;
                }
            }

            return null;
        }

        private PriceList? FindPriceList(int priceListId)
        {
            foreach (var list in PriceLists)
            {
                if (list.PriceListId == priceListId)
                {
                    return list;
                }
            }

            return null;
        }
    }
}
