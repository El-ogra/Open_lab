using System;
using System.Windows.Input;
using Open_lab.ViewModels;

namespace Open_lab.ViewModels.Accounts
{
    public class AccountsModuleViewModel : BaseViewModel
    {
        private readonly Action<string> _openPlaceholder;

        public AccountsModuleViewModel(Action<string> openPlaceholder)
        {
            _openPlaceholder = openPlaceholder ?? throw new ArgumentNullException(nameof(openPlaceholder));

            OpenInventoryCommand = new RelayCommand(_ => _openPlaceholder("الجرد وحساب الدرج"));
            OpenExternalSamplesCommand = new RelayCommand(_ => _openPlaceholder("العينات المرسلة للخارج"));
            OpenCashTransactionCommand = new RelayCommand(_ => _openPlaceholder("صرف وإيداع نقدية"));
            OpenCompanyAccountsCommand = new RelayCommand(_ => _openPlaceholder("حساب شركات ومندوبين"));
        }

        public ICommand OpenInventoryCommand { get; }
        public ICommand OpenExternalSamplesCommand { get; }
        public ICommand OpenCashTransactionCommand { get; }
        public ICommand OpenCompanyAccountsCommand { get; }
    }
}
