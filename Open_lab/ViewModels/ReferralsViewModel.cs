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
        private Referral? _selectedReferral;
        private string _statusMessage = string.Empty;

        public ReferralsViewModel(ITestCatalogService testCatalogService)
        {
            _testCatalogService = testCatalogService;
            Referrals = new ObservableCollection<Referral>();

            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedReferral != null);

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
                var referral = await _testCatalogService.CreateReferralAsync(new Referral
                {
                    Name = Name,
                    ReferralType = Type,
                    Phone = Phone,
                    City = City
                });

                Referrals.Add(referral);
                StatusMessage = "تم حفظ الجهة.";
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
