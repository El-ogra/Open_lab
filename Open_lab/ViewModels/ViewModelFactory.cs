using System;
using Microsoft.Extensions.DependencyInjection;

namespace Open_lab.ViewModels
{
    public class ViewModelFactory : IViewModelFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ViewModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public BaseViewModel Create(NavigationTarget target, Action? onLoginSuccess = null)
        {
            return target switch
            {
                NavigationTarget.Home => CreateViewModel<HomeViewModel>(),
                NavigationTarget.Login => CreateLoginViewModel(onLoginSuccess),
                NavigationTarget.Dashboard => CreateViewModel<DashboardViewModel>(),
                NavigationTarget.PatientRegistration => CreateViewModel<PatientRegistrationViewModel>(),
                NavigationTarget.PatientTestsSelection => CreateViewModel<PatientTestsSelectionViewModel>(),
                NavigationTarget.PatientBilling => CreateViewModel<PatientBillingViewModel>(),
                NavigationTarget.PatientBillingByDate => CreateViewModel<PatientBillingByDateViewModel>(),
                NavigationTarget.ResultsEntry => CreateViewModel<ResultsEntryViewModel>(),
                NavigationTarget.ReportViewer => CreateViewModel<ReportViewerViewModel>(),
                NavigationTarget.PatientSearch => CreateViewModel<PatientSearchViewModel>(),
                NavigationTarget.PatientHistory => CreateViewModel<PatientHistoryViewModel>(),
                NavigationTarget.WorkSheetByPatient => CreateViewModel<WorkSheetByPatientViewModel>(),
                NavigationTarget.WorkSheetByTest => CreateViewModel<WorkSheetByTestViewModel>(),
                NavigationTarget.TestCatalog => CreateViewModel<TestCatalogViewModel>(),
                NavigationTarget.ReferenceRanges => CreateViewModel<ReferenceRangesViewModel>(),
                NavigationTarget.TestComments => CreateViewModel<TestCommentsViewModel>(),
                NavigationTarget.PriceLists => CreateViewModel<PriceListsViewModel>(),
                NavigationTarget.CustomGroups => CreateViewModel<CustomGroupsViewModel>(),
                NavigationTarget.Referrals => CreateViewModel<ReferralsViewModel>(),
                NavigationTarget.UsersPermissions => CreateViewModel<UsersPermissionsViewModel>(),
                NavigationTarget.Statistics => CreateViewModel<StatisticsViewModel>(),
                NavigationTarget.SystemSettings => CreateViewModel<SystemSettingsViewModel>(),
                NavigationTarget.BackupRestore => CreateViewModel<BackupRestoreViewModel>(),
                NavigationTarget.AttendanceLog => CreateViewModel<AttendanceLogViewModel>(),
                NavigationTarget.AccountsTreasury => CreateViewModel<AccountsTreasuryViewModel>(),
                NavigationTarget.Delivery => CreateViewModel<DeliveryViewModel>(),
                NavigationTarget.SampleCollection => CreateViewModel<SampleCollectionViewModel>(),
                NavigationTarget.CultureSensitivity => CreateViewModel<CultureSensitivityViewModel>(),
                NavigationTarget.ReceiptPrinting => CreateViewModel<ReceiptPrintingViewModel>(),
                NavigationTarget.CombinedReport => CreateViewModel<CombinedReportViewModel>(),
                NavigationTarget.BlankReport => CreateViewModel<BlankReportViewModel>(),
                NavigationTarget.Constants => CreateViewModel<ConstantsViewModel>(),
                NavigationTarget.CompareWithHistory => CreateViewModel<CompareWithHistoryViewModel>(),
                NavigationTarget.GroupWorksheet => CreateViewModel<GroupWorksheetViewModel>(),
                NavigationTarget.TestClassificationLog => CreateViewModel<TestClassificationLogViewModel>(),
                NavigationTarget.ExternalLabManagement => CreateViewModel<ExternalLabManagementViewModel>(),
                NavigationTarget.AttendanceReport => CreateViewModel<AttendanceReportViewModel>(),
                NavigationTarget.ContractInvoice => CreateViewModel<ContractInvoiceViewModel>(),
                NavigationTarget.UserActivityLog => CreateViewModel<UserActivityLogViewModel>(),
                NavigationTarget.SystemUsageMonitor => CreateViewModel<SystemUsageMonitorViewModel>(),
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unsupported navigation target.")
            };
        }

        private BaseViewModel CreateLoginViewModel(Action? onLoginSuccess)
        {
            if (onLoginSuccess is null)
            {
                throw new InvalidOperationException("Login navigation requires a success callback.");
            }

            return ActivatorUtilities.CreateInstance<LoginViewModel>(_serviceProvider, onLoginSuccess);
        }

        private TViewModel CreateViewModel<TViewModel>() where TViewModel : BaseViewModel
            => ActivatorUtilities.CreateInstance<TViewModel>(_serviceProvider);
    }
}
