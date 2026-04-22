using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class ReferralsViewModel : BaseViewModel
    {
        private readonly ITestCatalogService _testCatalogService;
        private string _name = string.Empty;
        private string _type = string.Empty;
        private string? _phone;
        private string? _city;
        private decimal _discountPercentage;
        private decimal _commissionPercentage;
        private Referral? _selectedReferral;
        private string _statusMessage = string.Empty;

        public ReferralsViewModel(ITestCatalogService testCatalogService)
        {
            _testCatalogService = testCatalogService;
            Referrals = new ObservableCollection<Referral>();

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit));
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit) && SelectedReferral != null);

            _ = LoadAsync();
        }

        public ObservableCollection<Referral> Referrals { get; }

        public Referral? SelectedReferral
        {
            get => _selectedReferral;
            set
            {
                if (SetProperty(ref _selectedReferral, value))
                {
                    if (value != null)
                    {
                        Name = value.Name;
                        Type = value.ReferralType;
                        Phone = value.Phone;
                        City = value.City;
                        DiscountPercentage = value.DiscountPercentage;
                        CommissionPercentage = value.CommissionPercentage;
                    }
                    (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public string? Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string? City
        {
            get => _city;
            set => SetProperty(ref _city, value);
        }

        public decimal DiscountPercentage
        {
            get => _discountPercentage;
            set => SetProperty(ref _discountPercentage, value);
        }

        public decimal CommissionPercentage
        {
            get => _commissionPercentage;
            set => SetProperty(ref _commissionPercentage, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }

        private async Task LoadAsync()
        {
            var referrals = await _testCatalogService.GetReferralsAsync();
            Referrals.Clear();
            foreach (var referral in referrals)
            {
                Referrals.Add(referral);
            }
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Type))
            {
                StatusMessage = "يرجى إدخال الاسم والنوع.";
                return;
            }

            try
            {
                if (DiscountPercentage < 0 || DiscountPercentage > 100)
                {
                    StatusMessage = "نسبة الخصم يجب أن تكون بين 0 و 100.";
                    return;
                }

                if (CommissionPercentage < 0 || CommissionPercentage > 100)
                {
                    StatusMessage = "نسبة العمولة يجب أن تكون بين 0 و 100.";
                    return;
                }

                var model = new Referral
                {
                    ReferralId = SelectedReferral?.ReferralId ?? 0,
                    Name = Name,
                    ReferralType = Type,
                    Phone = Phone,
                    City = City,
                    DiscountPercentage = DiscountPercentage,
                    CommissionPercentage = CommissionPercentage
                };

                if (model.ReferralId == 0)
                {
                    var created = await _testCatalogService.CreateReferralAsync(model);
                    Referrals.Add(created);
                    StatusMessage = "تم حفظ الجهة.";
                }
                else
                {
                    await _testCatalogService.UpdateReferralAsync(model);
                    await LoadAsync();
                    StatusMessage = "تم تحديث بيانات الجهة.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedReferral == null)
            {
                return;
            }

            try
            {
                await _testCatalogService.DeleteReferralAsync(SelectedReferral.ReferralId);
                Referrals.Remove(SelectedReferral);
                SelectedReferral = null;
                StatusMessage = "تم حذف الجهة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}

