using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.Integration
{
    public class FullReauditIntegrationTests
    {
        private static readonly Regex ModuleRegex = new(@"^##\s+(\d+)\.\s+", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex FunctionRegex = new(@"^####\s+(\d+\.\d+)\s+", RegexOptions.Multiline | RegexOptions.Compiled);

        [Fact]
        public void OpenLabModulesDocumentation_Should_Define_13_Modules_And_97_Functions()
        {
            // Function: 13.8 — Set System Password
            // Arrange
            // Act
            var markdown = File.ReadAllText(GetModulesDocPath());
            var moduleCount = ModuleRegex.Matches(markdown).Count;
            var functionIds = ExtractFunctionIds(markdown);

            // Assert
            moduleCount.Should().Be(13);
            functionIds.Should().HaveCount(97);
            functionIds.Distinct().Should().HaveCount(97);
        }

        [Fact]
        public void Every_Documented_Function_Should_Have_Model_Service_ViewModel_Mapping()
        {
            // Function: 13.8 — Set System Password
            // Arrange
            // Act
            var documentedIds = ExtractFunctionIds(File.ReadAllText(GetModulesDocPath()));
            var mapped = BuildCases();

            // Assert
            mapped.Select(x => x.FunctionId).Should().BeEquivalentTo(documentedIds);
            mapped.Should().OnlyContain(x => x.ModelType.Namespace != null && x.ModelType.Namespace.StartsWith("Open_lab.Models"));
            mapped.Should().OnlyContain(x => x.ServiceType.Namespace != null && x.ServiceType.Namespace.StartsWith("Open_lab.Services"));
            mapped.Should().OnlyContain(x => x.ViewModelType.Namespace != null && x.ViewModelType.Namespace.StartsWith("Open_lab.ViewModels"));
        }

        [Fact]
        public void Every_Function_Mapping_Should_Have_Service_Interface_And_ViewModel_Commands()
        {
            // Function: 13.8 — Set System Password
            // Arrange
            // Act
            foreach (var item in BuildCases())
            {
                var interfaceType = item.ServiceType.GetInterface($"I{item.ServiceType.Name}");
                // Assert
                interfaceType.Should().NotBeNull($"{item.FunctionId} requires a service contract");

                var serviceMethods = item.ServiceType
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName)
                    .ToList();
                serviceMethods.Should().NotBeEmpty($"{item.FunctionId} requires service behavior");

                var commandMembers = item.ViewModelType
                    .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                    .Where(m => m.Name.EndsWith("Command", StringComparison.Ordinal))
                    .ToList();
                commandMembers.Should().NotBeEmpty($"{item.FunctionId} requires ViewModel command surface");
            }
        }

        [Fact]
        public void Every_Function_Mapping_Should_Have_Service_And_ViewModel_Test_Evidence()
        {
            // Function: 13.8 — Set System Password
            // Arrange
            // Act
            var serviceTestTexts = Directory.GetFiles(Path.Combine(GetRepositoryRoot(), "Open_lab.Tests", "Services"), "*.cs", SearchOption.AllDirectories)
                .Select(File.ReadAllText)
                .ToList();
            var viewModelTestTexts = Directory.GetFiles(Path.Combine(GetRepositoryRoot(), "Open_lab.Tests", "ViewModels"), "*.cs", SearchOption.AllDirectories)
                .Select(File.ReadAllText)
                .ToList();

            foreach (var item in BuildCases())
            {
                // Assert
                serviceTestTexts.Any(t => t.Contains(item.ServiceType.Name, StringComparison.Ordinal)).Should()
                    .BeTrue($"{item.FunctionId} requires service test evidence for {item.ServiceType.Name}");
                viewModelTestTexts.Any(t => t.Contains(item.ViewModelType.Name, StringComparison.Ordinal)).Should()
                    .BeTrue($"{item.FunctionId} requires viewmodel test evidence for {item.ViewModelType.Name}");
            }
        }

        private static List<string> ExtractFunctionIds(string markdown)
        {
            return FunctionRegex.Matches(markdown)
                .Select(m => m.Groups[1].Value)
                .Distinct()
                .OrderBy(ParseFunctionId)
                .ToList();
        }

        private static int ParseFunctionId(string id)
        {
            var parts = id.Split('.');
            return int.Parse(parts[0]) * 100 + int.Parse(parts[1]);
        }

        private static string GetModulesDocPath()
        {
            return Path.Combine(GetRepositoryRoot(), "Open_lab", "Docs", "Open_lab_Modules_Documentation.md");
        }

        private static string GetRepositoryRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (File.Exists(Path.Combine(dir.FullName, "Open_lab.sln")))
                {
                    return dir.FullName;
                }

                dir = dir.Parent;
            }

            throw new DirectoryNotFoundException("Could not locate repository root.");
        }

        private static List<FunctionLayerCase> BuildCases()
        {
            var list = new List<FunctionLayerCase>();

            // 1.x Patient Management
            Add(list, 1, 1, typeof(Patient), typeof(PatientService), typeof(PatientRegistrationViewModel));
            Add(list, 1, 2, typeof(Patient), typeof(PatientService), typeof(PatientRegistrationViewModel));
            Add(list, 1, 3, typeof(Visit), typeof(VisitService), typeof(PatientTestsSelectionViewModel));
            Add(list, 1, 4, typeof(VisitTest), typeof(VisitService), typeof(PatientTestsSelectionViewModel));
            Add(list, 1, 5, typeof(Patient), typeof(PatientSearchService), typeof(PatientSearchViewModel));
            Add(list, 1, 6, typeof(MedicalHistory), typeof(VisitService), typeof(PatientHistoryViewModel));
            Add(list, 1, 7, typeof(MedicalHistory), typeof(PatientService), typeof(PatientRegistrationViewModel));
            Add(list, 1, 8, typeof(CustomGroup), typeof(VisitService), typeof(PatientTestsSelectionViewModel));

            // 2.x Finance
            AddRange(list, 2, 1, 9, typeof(Invoice), typeof(InvoiceService), typeof(PatientBillingViewModel));
            Add(list, 2, 10, typeof(Invoice), typeof(AccountsTreasuryService), typeof(AccountsTreasuryViewModel));
            Add(list, 2, 11, typeof(Branch), typeof(AccountsTreasuryService), typeof(AccountsTreasuryViewModel));
            Add(list, 2, 12, typeof(Physician), typeof(AccountsTreasuryService), typeof(AccountsTreasuryViewModel));
            Add(list, 2, 13, typeof(ExternalLabSettlement), typeof(ExternalSettlementService), typeof(ExternalLabManagementViewModel));

            // 3.x Test Catalog
            AddRange(list, 3, 1, 9, typeof(Test), typeof(TestCatalogService), typeof(TestCatalogViewModel));

            // 4.x Results and Reports
            Add(list, 4, 1, typeof(ResultValue), typeof(ResultsService), typeof(ResultsEntryViewModel));
            Add(list, 4, 2, typeof(ResultValue), typeof(ResultsService), typeof(ResultsEntryViewModel));
            Add(list, 4, 3, typeof(ResultValue), typeof(ResultsService), typeof(ResultsEntryViewModel));
            Add(list, 4, 4, typeof(ResultValue), typeof(ReportService), typeof(CombinedReportViewModel));
            Add(list, 4, 5, typeof(Test), typeof(ReportService), typeof(ReportViewerViewModel));
            Add(list, 4, 6, typeof(ResultValue), typeof(ReportService), typeof(ReportViewerViewModel));
            Add(list, 4, 7, typeof(ResultValue), typeof(PrintService), typeof(ReportViewerViewModel));
            Add(list, 4, 8, typeof(ResultValue), typeof(PrintService), typeof(BlankReportViewModel));
            Add(list, 4, 9, typeof(ResultValue), typeof(CompareWithHistoryService), typeof(CompareWithHistoryViewModel));

            // 5.x Culture & Sensitivity
            AddRange(list, 5, 1, 7, typeof(Culture), typeof(CultureSensitivityService), typeof(CultureSensitivityViewModel));

            // 6.x Sample Collection
            Add(list, 6, 1, typeof(SampleCollection), typeof(SampleCollectionService), typeof(SampleCollectionViewModel));
            Add(list, 6, 2, typeof(SampleCollection), typeof(SampleCollectionService), typeof(SampleCollectionViewModel));
            Add(list, 6, 3, typeof(SampleCollection), typeof(SampleTrackingService), typeof(SampleCollectionViewModel));
            Add(list, 6, 4, typeof(SampleCollection), typeof(SampleCollectionService), typeof(SampleCollectionViewModel));

            // 7.x Worksheets
            Add(list, 7, 1, typeof(Visit), typeof(WorksheetService), typeof(WorkSheetByPatientViewModel));
            Add(list, 7, 2, typeof(VisitTest), typeof(WorksheetService), typeof(WorkSheetByTestViewModel));
            Add(list, 7, 3, typeof(TestGroup), typeof(GroupWorksheetService), typeof(GroupWorksheetViewModel));
            Add(list, 7, 4, typeof(TestConsumption), typeof(TestClassificationService), typeof(TestClassificationLogViewModel));

            // 8.x External Lab
            Add(list, 8, 1, typeof(ExternalLabQueue), typeof(ExternalLabService), typeof(ExternalLabManagementViewModel));
            Add(list, 8, 2, typeof(ExternalLabQueue), typeof(ExternalLabService), typeof(ExternalLabManagementViewModel));
            Add(list, 8, 3, typeof(ShipmentManifest), typeof(ExternalLabService), typeof(ExternalLabManagementViewModel));
            Add(list, 8, 4, typeof(ExternalLabQueue), typeof(ExternalLabService), typeof(ExternalLabManagementViewModel));
            Add(list, 8, 5, typeof(ResultValue), typeof(ExternalLabService), typeof(ExternalLabManagementViewModel));
            Add(list, 8, 6, typeof(ResultValue), typeof(PrintService), typeof(ExternalLabManagementViewModel));
            Add(list, 8, 7, typeof(ExternalLabSettlement), typeof(ExternalSettlementService), typeof(ExternalLabManagementViewModel));

            // 9.x Statistics
            AddRange(list, 9, 1, 5, typeof(Visit), typeof(StatisticsService), typeof(StatisticsViewModel));
            Add(list, 9, 6, typeof(ResultValue), typeof(UserProductivityService), typeof(StatisticsViewModel));

            // 10.x Users & Security
            Add(list, 10, 1, typeof(User), typeof(UserAdminService), typeof(UsersPermissionsViewModel));
            Add(list, 10, 2, typeof(RolePermission), typeof(UserAdminService), typeof(UsersPermissionsViewModel));
            Add(list, 10, 3, typeof(User), typeof(UserAdminService), typeof(UsersPermissionsViewModel));
            Add(list, 10, 4, typeof(AttendanceLog), typeof(AttendanceService), typeof(AttendanceLogViewModel));
            Add(list, 10, 5, typeof(AttendanceLog), typeof(AttendanceService), typeof(AttendanceLogViewModel));
            Add(list, 10, 6, typeof(AuditLog), typeof(UserActivityService), typeof(UserActivityLogViewModel));
            Add(list, 10, 7, typeof(AttendanceLog), typeof(SystemMonitorService), typeof(SystemUsageMonitorViewModel));
            Add(list, 10, 8, typeof(User), typeof(AuthService), typeof(LoginViewModel));

            // 11.x Attendance
            Add(list, 11, 1, typeof(AttendanceLog), typeof(AttendanceService), typeof(AttendanceLogViewModel));
            Add(list, 11, 2, typeof(AttendanceLog), typeof(AttendanceService), typeof(AttendanceLogViewModel));
            Add(list, 11, 3, typeof(AttendanceLog), typeof(AttendanceService), typeof(AttendanceReportViewModel));
            Add(list, 11, 4, typeof(AttendanceLog), typeof(TardinessService), typeof(AttendanceReportViewModel));
            Add(list, 11, 5, typeof(AttendanceLog), typeof(AttendanceService), typeof(AttendanceReportViewModel));

            // 12.x Contracts & Referrals
            Add(list, 12, 1, typeof(Referral), typeof(TestCatalogService), typeof(ReferralsViewModel));
            Add(list, 12, 2, typeof(PriceList), typeof(TestCatalogService), typeof(PriceListsViewModel));
            Add(list, 12, 3, typeof(Referral), typeof(TestCatalogService), typeof(ReferralsViewModel));
            Add(list, 12, 4, typeof(Referral), typeof(TestCatalogService), typeof(ReferralsViewModel));
            Add(list, 12, 5, typeof(Visit), typeof(VisitService), typeof(PatientTestsSelectionViewModel));
            Add(list, 12, 6, typeof(Physician), typeof(PhysicianService), typeof(PhysicianViewModel));
            Add(list, 12, 7, typeof(Physician), typeof(PhysicianService), typeof(PhysicianViewModel));
            Add(list, 12, 8, typeof(ContractInvoice), typeof(ContractInvoiceService), typeof(ContractInvoiceViewModel));
            Add(list, 12, 9, typeof(ContractInvoice), typeof(ContractInvoiceService), typeof(ContractInvoiceViewModel));

            // 13.x Settings
            AddRange(list, 13, 1, 6, typeof(Setting), typeof(SettingsService), typeof(SettingsViewModel));
            Add(list, 13, 7, typeof(Setting), typeof(BackupRestoreService), typeof(BackupRestoreViewModel));
            Add(list, 13, 8, typeof(Setting), typeof(SystemSettingsService), typeof(SystemSettingsViewModel));

            return list.OrderBy(x => ParseFunctionId(x.FunctionId)).ToList();
        }

        private static void AddRange(List<FunctionLayerCase> list, int module, int start, int end, Type modelType, Type serviceType, Type viewModelType)
        {
            for (var i = start; i <= end; i++)
            {
                Add(list, module, i, modelType, serviceType, viewModelType);
            }
        }

        private static void Add(List<FunctionLayerCase> list, int module, int function, Type modelType, Type serviceType, Type viewModelType)
        {
            list.Add(new FunctionLayerCase($"{module}.{function}", modelType, serviceType, viewModelType));
        }

        private sealed record FunctionLayerCase(string FunctionId, Type ModelType, Type ServiceType, Type ViewModelType);
    }
}
