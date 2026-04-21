using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    /// <summary>
    /// ViewModel for managing physicians (doctors).
    /// </summary>
    public class PhysicianViewModel : BaseViewModel
    {
        private readonly IPhysicianService _physicianService;
        private readonly ITestCatalogService _testCatalogService;
        private Physician? _selectedPhysician;
        private string _fullName = string.Empty;
        private string? _phone;
        private string? _specialty;
        private string? _address;
        private bool _isActive = true;
        private int? _priceListId;
        private decimal? _commissionPercentage;
        private string _statusMessage = string.Empty;
        private string _searchTerm = string.Empty;

        public PhysicianViewModel(IPhysicianService physicianService, ITestCatalogService testCatalogService)
        {
            _physicianService = physicianService;
            _testCatalogService = testCatalogService;
            Physicians = new ObservableCollection<Physician>();
            PriceLists = new ObservableCollection<PriceList>();

            LoadPhysiciansCommand = new RelayCommand(async _ => await LoadPhysiciansAsync());
            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => CanSave());
            NewCommand = new RelayCommand(_ => New());
            SearchCommand = new RelayCommand(async _ => await SearchAsync(), _ => !string.IsNullOrWhiteSpace(SearchTerm));

            _ = LoadPhysiciansAsync();
            _ = LoadPriceListsAsync();
        }

        public ObservableCollection<Physician> Physicians { get; }
        public ObservableCollection<PriceList> PriceLists { get; }

        public Physician? SelectedPhysician
        {
            get => _selectedPhysician;
            set
            {
                if (SetProperty(ref _selectedPhysician, value) && value != null)
                {
                    LoadFromModel(value);
                }
            }
        }

        public string FullName
        {
            get => _fullName;
            set { if (SetProperty(ref _fullName, value)) (SaveCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        public string? Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string? Specialty
        {
            get => _specialty;
            set => SetProperty(ref _specialty, value);
        }

        public string? Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public int? PriceListId
        {
            get => _priceListId;
            set => SetProperty(ref _priceListId, value);
        }

        public decimal? CommissionPercentage
        {
            get => _commissionPercentage;
            set => SetProperty(ref _commissionPercentage, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public string SearchTerm
        {
            get => _searchTerm;
            set { if (SetProperty(ref _searchTerm, value)) (SearchCommand as RelayCommand)?.RaiseCanExecuteChanged(); }
        }

        public ICommand LoadPhysiciansCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand SearchCommand { get; }

        private async Task LoadPhysiciansAsync()
        {
            try
            {
                var physicians = await _physicianService.GetActiveAsync();
                Physicians.Clear();
                foreach (var physician in physicians)
                {
                    Physicians.Add(physician);
                }
                StatusMessage = $"تم تحميل {Physicians.Count} طبيب.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadPriceListsAsync()
        {
            try
            {
                var priceLists = await _testCatalogService.GetPriceListsAsync();
                PriceLists.Clear();
                foreach (var pl in priceLists)
                {
                    PriceLists.Add(pl);
                }
            }
            catch (Exception)
            {
                // Silently handle - price lists are optional
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                var model = new Physician
                {
                    PhysicianId = SelectedPhysician?.PhysicianId ?? 0,
                    FullName = FullName,
                    Phone = Phone,
                    Specialty = Specialty,
                    Address = Address,
                    IsActive = IsActive,
                    PriceListId = PriceListId,
                    CommissionPercentage = CommissionPercentage
                };

                if (model.PhysicianId == 0)
                {
                    await _physicianService.CreateAsync(model);
                    StatusMessage = "تم إنشاء الطبيب بنجاح.";
                }
                else
                {
                    await _physicianService.UpdateAsync(model);
                    StatusMessage = "تم تحديث الطبيب بنجاح.";
                }

                await LoadPhysiciansAsync();
                New();
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(FullName);
        }

        private void New()
        {
            SelectedPhysician = null;
            FullName = string.Empty;
            Phone = null;
            Specialty = null;
            Address = null;
            IsActive = true;
            PriceListId = null;
            CommissionPercentage = null;
        }

        private void LoadFromModel(Physician physician)
        {
            FullName = physician.FullName;
            Phone = physician.Phone;
            Specialty = physician.Specialty;
            Address = physician.Address;
            IsActive = physician.IsActive;
            PriceListId = physician.PriceListId;
            CommissionPercentage = physician.CommissionPercentage;
        }

        private async Task SearchAsync()
        {
            try
            {
                var results = await _physicianService.SearchAsync(SearchTerm);
                Physicians.Clear();
                foreach (var physician in results)
                {
                    Physicians.Add(physician);
                }
                StatusMessage = $"تم العثور على {Physicians.Count} نتيجة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
