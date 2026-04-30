using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    /// <summary>
    /// Additional unit tests for Module 12 — Contracts & Referrals (جهات التعاقد والإحالة).
    /// Covers Functions 12.1 → 12.9 at the ViewModel layer (ReferralsViewModel,
    /// PhysicianViewModel, ContractInvoiceViewModel) with Success / Failure / Edge scenarios.
    /// All dependencies are mocked using Moq per project standards.
    /// </summary>
    public class Module12ViewModelTests_Additional : IDisposable
    {
        public Module12ViewModelTests_Additional()
        {
            AppSessionTestHelper.ResetToAdmin();
        }

        public void Dispose() => AppSessionTestHelper.Reset();

        // ==================================================================================
        // 12.1 — Create Contract Entity (إنشاء جهة تعاقد) — ReferralsViewModel
        // ==================================================================================

        [Fact]
        public async Task CreateContractEntity_WithValidCompanyData_ShouldCallServiceAndAddToList_SuccessGuard()
        {
            // Function: 12.1 — Create Contract Entity
            // Arrange
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            catalogMock.Setup(s => s.CreateReferralAsync(It.IsAny<Referral>()))
                .ReturnsAsync((Referral r) =>
                {
                    r.ReferralId = 100;
                    return r;
                });

            var viewModel = new ReferralsViewModel(catalogMock.Object);
            await Task.Delay(50);
            viewModel.Name = "Insurance Co.";
            viewModel.Type = "Insurance";
            viewModel.Phone = "01000000000";
            viewModel.City = "Cairo";
            viewModel.DiscountPercentage = 20m;
            viewModel.CommissionPercentage = 10m;

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            catalogMock.Verify(s => s.CreateReferralAsync(It.Is<Referral>(r =>
                r.Name == "Insurance Co." &&
                r.ReferralType == "Insurance" &&
                r.DiscountPercentage == 20m &&
                r.CommissionPercentage == 10m)), Times.Once);
            viewModel.Referrals.Should().ContainSingle(r => r.Name == "Insurance Co.");
            viewModel.StatusMessage.Should().Contain("تم حفظ الجهة");
        }

        [Fact]
        public async Task CreateContractEntity_WithEmptyName_ShouldNotCallServiceAndShowError_FailureGuard()
        {
            // Function: 12.1 — Create Contract Entity
            // Arrange
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            var viewModel = new ReferralsViewModel(catalogMock.Object);
            await Task.Delay(50);
            viewModel.Name = "";
            viewModel.Type = "Company";

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            catalogMock.Verify(s => s.CreateReferralAsync(It.IsAny<Referral>()), Times.Never);
            viewModel.StatusMessage.Should().NotBeEmpty();
            viewModel.StatusMessage.Should().Contain("الاسم والنوع");
        }

        [Fact]
        public async Task CreateContractEntity_WhenServiceThrowsDuplicate_ShouldShowErrorMessage_FailureGuard()
        {
            // Function: 12.1 — Create Contract Entity
            // Arrange
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            catalogMock.Setup(s => s.CreateReferralAsync(It.IsAny<Referral>()))
                .ThrowsAsync(new InvalidOperationException("Referral already exists."));
            var viewModel = new ReferralsViewModel(catalogMock.Object);
            await Task.Delay(50);
            viewModel.Name = "Existing Co";
            viewModel.Type = "Company";

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Contain("Referral already exists");
        }

        [Fact]
        public async Task CreateContractEntity_WhenUserHasNoEditPermission_ShouldDisableSaveCommand_FailureGuard()
        {
            // Function: 12.1 — Create Contract Entity (security)
            // Arrange
            AppSessionTestHelper.ResetToUserWithPermissions(); // no permissions
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            var viewModel = new ReferralsViewModel(catalogMock.Object);
            await Task.Delay(50);

            // Act
            var canExecute = viewModel.SaveCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse();
        }

        // ==================================================================================
        // 12.3 — Set Entity Discount (ضبط خصم الجهة)
        // ==================================================================================

        [Fact]
        public async Task SetEntityDiscount_WithValidPercentage_ShouldCallUpdate_SuccessGuard()
        {
            // Function: 12.3 — Set Entity Discount
            // Arrange
            var existing = new Referral
            {
                ReferralId = 5,
                Name = "Existing",
                ReferralType = "Company",
                DiscountPercentage = 0m,
                CommissionPercentage = 0m
            };
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral> { existing });
            catalogMock.Setup(s => s.UpdateReferralAsync(It.IsAny<Referral>())).Returns(Task.CompletedTask);
            var viewModel = new ReferralsViewModel(catalogMock.Object);
            await Task.Delay(80);

            viewModel.SelectedReferral = existing;
            viewModel.DiscountPercentage = 30m;

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            catalogMock.Verify(s => s.UpdateReferralAsync(It.Is<Referral>(r =>
                r.ReferralId == 5 && r.DiscountPercentage == 30m)), Times.Once);
            viewModel.StatusMessage.Should().Contain("تم تحديث");
        }

        [Fact]
        public async Task SetEntityDiscount_WithDiscountAbove100_ShouldShowValidationMessage_FailureGuard()
        {
            // Function: 12.3 — Set Entity Discount
            // Arrange
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            var viewModel = new ReferralsViewModel(catalogMock.Object);
            await Task.Delay(50);
            viewModel.Name = "Co";
            viewModel.Type = "Company";
            viewModel.DiscountPercentage = 150m;

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            catalogMock.Verify(s => s.CreateReferralAsync(It.IsAny<Referral>()), Times.Never);
            viewModel.StatusMessage.Should().Contain("نسبة الخصم");
        }

        [Fact]
        public async Task SetEntityDiscount_WithExactly100Percent_ShouldAllowSave_EdgeGuard()
        {
            // Function: 12.3 — Set Entity Discount (BR-ACC-004 boundary)
            // Arrange
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            catalogMock.Setup(s => s.CreateReferralAsync(It.IsAny<Referral>()))
                .ReturnsAsync((Referral r) => { r.ReferralId = 1; return r; });
            var viewModel = new ReferralsViewModel(catalogMock.Object);
            await Task.Delay(50);
            viewModel.Name = "Edge Co";
            viewModel.Type = "Company";
            viewModel.DiscountPercentage = 100m;

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            catalogMock.Verify(s => s.CreateReferralAsync(It.Is<Referral>(r => r.DiscountPercentage == 100m)), Times.Once);
            viewModel.StatusMessage.Should().Contain("تم حفظ");
        }

        // ==================================================================================
        // 12.4 — Set Entity Commission (ضبط عمولة الجهة) — BR-ACC-008
        // ==================================================================================

        [Fact]
        public async Task SetEntityCommission_WithValidPercentage_ShouldCallCreate_SuccessGuard_BR_ACC_008()
        {
            // Function: 12.4 — Set Entity Commission (BR-ACC-008)
            // Arrange
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            catalogMock.Setup(s => s.CreateReferralAsync(It.IsAny<Referral>()))
                .ReturnsAsync((Referral r) => { r.ReferralId = 1; return r; });
            var viewModel = new ReferralsViewModel(catalogMock.Object);
            await Task.Delay(50);
            viewModel.Name = "CommCo";
            viewModel.Type = "Company";
            viewModel.CommissionPercentage = 15m;

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            catalogMock.Verify(s => s.CreateReferralAsync(It.Is<Referral>(r => r.CommissionPercentage == 15m)), Times.Once);
        }

        [Fact]
        public async Task SetEntityCommission_WithCommissionAbove100_ShouldShowValidationMessage_FailureGuard_BR_ACC_008()
        {
            // Function: 12.4 — Set Entity Commission (BR-ACC-008)
            // Arrange
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            var viewModel = new ReferralsViewModel(catalogMock.Object);
            await Task.Delay(50);
            viewModel.Name = "CommErr";
            viewModel.Type = "Company";
            viewModel.DiscountPercentage = 0m;
            viewModel.CommissionPercentage = 250m;

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            catalogMock.Verify(s => s.CreateReferralAsync(It.IsAny<Referral>()), Times.Never);
            viewModel.StatusMessage.Should().Contain("نسبة العمولة");
        }

        [Fact]
        public async Task SetEntityCommission_WithExactly100Percent_ShouldAllow_EdgeGuard_BR_ACC_008()
        {
            // Function: 12.4 — Set Entity Commission (BR-ACC-008 boundary)
            // Arrange
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            catalogMock.Setup(s => s.CreateReferralAsync(It.IsAny<Referral>()))
                .ReturnsAsync((Referral r) => { r.ReferralId = 2; return r; });
            var viewModel = new ReferralsViewModel(catalogMock.Object);
            await Task.Delay(50);
            viewModel.Name = "Comm100";
            viewModel.Type = "Company";
            viewModel.CommissionPercentage = 100m;

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            catalogMock.Verify(s => s.CreateReferralAsync(It.Is<Referral>(r => r.CommissionPercentage == 100m)), Times.Once);
        }

        // ==================================================================================
        // 12.6 — Add Referring Physician (إضافة طبيب محيل) — PhysicianViewModel
        // ==================================================================================

        [Fact]
        public async Task AddReferringPhysician_WithValidFullName_ShouldCallCreateAndShowStatus_SuccessGuard()
        {
            // Function: 12.6 — Add Referring Physician
            // Arrange
            var physicianMock = new Mock<IPhysicianService>();
            var catalogMock = new Mock<ITestCatalogService>();
            physicianMock.Setup(s => s.GetActiveAsync()).ReturnsAsync(new List<Physician>());
            catalogMock.Setup(s => s.GetPriceListsAsync()).ReturnsAsync(new List<PriceList>());
            physicianMock.Setup(s => s.CreateAsync(It.IsAny<Physician>()))
                .ReturnsAsync((Physician p) => { p.PhysicianId = 5; return p; });
            var viewModel = new PhysicianViewModel(physicianMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.FullName = "Dr. New";
            viewModel.Specialty = "Cardio";
            viewModel.Phone = "0100";
            viewModel.CommissionPercentage = 10m;

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            physicianMock.Verify(s => s.CreateAsync(It.Is<Physician>(p =>
                p.FullName == "Dr. New" && p.Specialty == "Cardio" && p.CommissionPercentage == 10m)), Times.Once);
            viewModel.StatusMessage.Should().Contain("تم إنشاء الطبيب");
        }

        [Fact]
        public async Task AddReferringPhysician_WithEmptyFullName_ShouldDisableSaveCommand_FailureGuard()
        {
            // Function: 12.6 — Add Referring Physician
            // Arrange
            var physicianMock = new Mock<IPhysicianService>();
            var catalogMock = new Mock<ITestCatalogService>();
            physicianMock.Setup(s => s.GetActiveAsync()).ReturnsAsync(new List<Physician>());
            catalogMock.Setup(s => s.GetPriceListsAsync()).ReturnsAsync(new List<PriceList>());
            var viewModel = new PhysicianViewModel(physicianMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.FullName = "  ";

            // Act
            var canExecute = viewModel.SaveCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse();
        }

        [Fact]
        public async Task AddReferringPhysician_WhenServiceThrowsDuplicate_ShouldSetErrorMessage_FailureGuard()
        {
            // Function: 12.6 — Add Referring Physician
            // Arrange
            var physicianMock = new Mock<IPhysicianService>();
            var catalogMock = new Mock<ITestCatalogService>();
            physicianMock.Setup(s => s.GetActiveAsync()).ReturnsAsync(new List<Physician>());
            catalogMock.Setup(s => s.GetPriceListsAsync()).ReturnsAsync(new List<PriceList>());
            physicianMock.Setup(s => s.CreateAsync(It.IsAny<Physician>()))
                .ThrowsAsync(new InvalidOperationException("Physician already exists."));
            var viewModel = new PhysicianViewModel(physicianMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.FullName = "Dr. Dup";

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            viewModel.StatusMessage.Should().Contain("Physician already exists");
        }

        [Fact]
        public async Task AddReferringPhysician_LoadCommand_ShouldPopulateActivePhysicians_SuccessGuard()
        {
            // Function: 12.6 — Add Referring Physician
            // Arrange
            var physicianMock = new Mock<IPhysicianService>();
            var catalogMock = new Mock<ITestCatalogService>();
            physicianMock.SetupSequence(s => s.GetActiveAsync())
                .ReturnsAsync(new List<Physician>())
                .ReturnsAsync(new List<Physician>
                {
                    new() { PhysicianId = 1, FullName = "Dr. A", IsActive = true },
                    new() { PhysicianId = 2, FullName = "Dr. B", IsActive = true }
                });
            catalogMock.Setup(s => s.GetPriceListsAsync()).ReturnsAsync(new List<PriceList>());
            var viewModel = new PhysicianViewModel(physicianMock.Object, catalogMock.Object);
            await Task.Delay(50);

            // Act
            viewModel.LoadPhysiciansCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.Physicians.Should().HaveCount(2);
            viewModel.StatusMessage.Should().Contain("تم تحميل 2 طبيب");
        }

        [Fact]
        public async Task AddReferringPhysician_NewCommand_ShouldClearAllFields_EdgeGuard()
        {
            // Function: 12.6 — Add Referring Physician (form reset)
            // Arrange
            var physicianMock = new Mock<IPhysicianService>();
            var catalogMock = new Mock<ITestCatalogService>();
            physicianMock.Setup(s => s.GetActiveAsync()).ReturnsAsync(new List<Physician>());
            catalogMock.Setup(s => s.GetPriceListsAsync()).ReturnsAsync(new List<PriceList>());
            var viewModel = new PhysicianViewModel(physicianMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.FullName = "Dr. X";
            viewModel.Specialty = "Lab";
            viewModel.Phone = "555";
            viewModel.PriceListId = 3;
            viewModel.CommissionPercentage = 25m;

            // Act
            viewModel.NewCommand.Execute(null);

            // Assert
            viewModel.FullName.Should().BeEmpty();
            viewModel.Specialty.Should().BeNull();
            viewModel.Phone.Should().BeNull();
            viewModel.PriceListId.Should().BeNull();
            viewModel.CommissionPercentage.Should().BeNull();
            viewModel.IsActive.Should().BeTrue();
        }

        // ==================================================================================
        // 12.7 — Set Physician Price List (ضبط قائمة أسعار الطبيب) — PhysicianViewModel
        // ==================================================================================

        [Fact]
        public async Task SetPhysicianPriceList_WhenSelectedExisting_ShouldCallUpdateWithPriceListId_SuccessGuard()
        {
            // Function: 12.7 — Set Physician Price List
            // Arrange
            var existing = new Physician
            {
                PhysicianId = 7,
                FullName = "Dr. Existing",
                IsActive = true,
                PriceListId = null
            };
            var physicianMock = new Mock<IPhysicianService>();
            var catalogMock = new Mock<ITestCatalogService>();
            physicianMock.Setup(s => s.GetActiveAsync()).ReturnsAsync(new List<Physician> { existing });
            catalogMock.Setup(s => s.GetPriceListsAsync())
                .ReturnsAsync(new List<PriceList> { new() { PriceListId = 4, Name = "Doctor PL" } });
            physicianMock.Setup(s => s.UpdateAsync(It.IsAny<Physician>()))
                .ReturnsAsync((Physician p) => p);
            var viewModel = new PhysicianViewModel(physicianMock.Object, catalogMock.Object);
            await Task.Delay(80);

            viewModel.SelectedPhysician = existing;
            viewModel.PriceListId = 4;

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            physicianMock.Verify(s => s.UpdateAsync(It.Is<Physician>(p =>
                p.PhysicianId == 7 && p.PriceListId == 4)), Times.Once);
            viewModel.StatusMessage.Should().Contain("تم تحديث");
        }

        [Fact]
        public async Task SetPhysicianPriceList_WhenServiceThrows_ShouldSetErrorMessage_FailureGuard()
        {
            // Function: 12.7 — Set Physician Price List
            // Arrange
            var existing = new Physician { PhysicianId = 8, FullName = "Dr. Err", IsActive = true };
            var physicianMock = new Mock<IPhysicianService>();
            var catalogMock = new Mock<ITestCatalogService>();
            physicianMock.Setup(s => s.GetActiveAsync()).ReturnsAsync(new List<Physician> { existing });
            catalogMock.Setup(s => s.GetPriceListsAsync()).ReturnsAsync(new List<PriceList>());
            physicianMock.Setup(s => s.UpdateAsync(It.IsAny<Physician>()))
                .ThrowsAsync(new InvalidOperationException("update-failed"));
            var viewModel = new PhysicianViewModel(physicianMock.Object, catalogMock.Object);
            await Task.Delay(80);
            viewModel.SelectedPhysician = existing;
            viewModel.PriceListId = 9;

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            viewModel.StatusMessage.Should().Contain("update-failed");
        }

        [Fact]
        public async Task SetPhysicianPriceList_WithNullPriceList_ShouldRemoveAssignment_EdgeGuard()
        {
            // Function: 12.7 — Set Physician Price List
            // Arrange
            var existing = new Physician
            {
                PhysicianId = 9,
                FullName = "Dr. Detach",
                IsActive = true,
                PriceListId = 11
            };
            var physicianMock = new Mock<IPhysicianService>();
            var catalogMock = new Mock<ITestCatalogService>();
            physicianMock.Setup(s => s.GetActiveAsync()).ReturnsAsync(new List<Physician> { existing });
            catalogMock.Setup(s => s.GetPriceListsAsync()).ReturnsAsync(new List<PriceList>());
            physicianMock.Setup(s => s.UpdateAsync(It.IsAny<Physician>())).ReturnsAsync((Physician p) => p);
            var viewModel = new PhysicianViewModel(physicianMock.Object, catalogMock.Object);
            await Task.Delay(80);
            viewModel.SelectedPhysician = existing;
            viewModel.PriceListId = null;

            // Act
            viewModel.SaveCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            physicianMock.Verify(s => s.UpdateAsync(It.Is<Physician>(p =>
                p.PhysicianId == 9 && p.PriceListId == null)), Times.Once);
        }

        // ==================================================================================
        // 12.6 — Search Physician (read operation, additional)
        // ==================================================================================

        [Fact]
        public async Task SearchPhysician_WithValidTerm_ShouldReplaceCollection_SuccessGuard()
        {
            // Function: 12.6 — Add Referring Physician (search action)
            // Arrange
            var physicianMock = new Mock<IPhysicianService>();
            var catalogMock = new Mock<ITestCatalogService>();
            physicianMock.Setup(s => s.GetActiveAsync()).ReturnsAsync(new List<Physician>());
            catalogMock.Setup(s => s.GetPriceListsAsync()).ReturnsAsync(new List<PriceList>());
            physicianMock.Setup(s => s.SearchAsync("ali"))
                .ReturnsAsync(new List<Physician>
                {
                    new() { PhysicianId = 50, FullName = "Dr. Ali" }
                });
            var viewModel = new PhysicianViewModel(physicianMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.SearchTerm = "ali";

            // Act
            viewModel.SearchCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.Physicians.Should().ContainSingle(p => p.FullName == "Dr. Ali");
            viewModel.StatusMessage.Should().Contain("تم العثور على 1");
        }

        [Fact]
        public async Task SearchPhysician_WithEmptySearchTerm_ShouldDisableSearchCommand_EdgeGuard()
        {
            // Function: 12.6 — Add Referring Physician
            // Arrange
            var physicianMock = new Mock<IPhysicianService>();
            var catalogMock = new Mock<ITestCatalogService>();
            physicianMock.Setup(s => s.GetActiveAsync()).ReturnsAsync(new List<Physician>());
            catalogMock.Setup(s => s.GetPriceListsAsync()).ReturnsAsync(new List<PriceList>());
            var viewModel = new PhysicianViewModel(physicianMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.SearchTerm = "";

            // Act
            var canExecute = viewModel.SearchCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse();
        }

        // ==================================================================================
        // 12.8 — Generate Contract Invoice (إصدار فاتورة التعاقد) — ContractInvoiceViewModel
        // ==================================================================================

        [Fact]
        public async Task GenerateContractInvoice_WithValidDataAndPendingInvoices_ShouldCallCreateService_SuccessGuard()
        {
            // Function: 12.8 — Generate Contract Invoice
            // Arrange
            var contractMock = new Mock<IContractInvoiceService>();
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());

            contractMock.Setup(s => s.GetPendingInvoicesAsync(7, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<BulkClaimRow>
                {
                    new() { InvoiceId = 1, NetTotal = 100m, PatientName = "P1" },
                    new() { InvoiceId = 2, NetTotal = 200m, PatientName = "P2" }
                });
            contractMock.Setup(s => s.CreateContractInvoiceAsync(7, "INV-001", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(555);
            contractMock.Setup(s => s.GetContractInvoicesAsync(7))
                .ReturnsAsync(new List<ContractInvoice>
                {
                    new() { ContractInvoiceId = 555, ReferralId = 7, InvoiceNumber = "INV-001" }
                });

            var viewModel = new ContractInvoiceViewModel(contractMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.SelectedReferralId = 7;
            viewModel.InvoiceNumber = "INV-001";

            viewModel.LoadPendingCommand.Execute(null);
            await Task.Delay(100);

            // Act
            viewModel.CreateInvoiceCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            contractMock.Verify(s => s.CreateContractInvoiceAsync(7, "INV-001", It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            viewModel.InvoiceNumber.Should().BeEmpty();
        }

        [Fact]
        public async Task GenerateContractInvoice_WithoutInvoiceNumber_ShouldDisableCreateCommand_FailureGuard()
        {
            // Function: 12.8 — Generate Contract Invoice
            // Arrange
            var contractMock = new Mock<IContractInvoiceService>();
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            contractMock.Setup(s => s.GetPendingInvoicesAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<BulkClaimRow> { new() { InvoiceId = 1, NetTotal = 50m } });
            var viewModel = new ContractInvoiceViewModel(contractMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.SelectedReferralId = 1;
            viewModel.InvoiceNumber = "";

            // Act
            var canExecute = viewModel.CreateInvoiceCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse();
        }

        [Fact]
        public async Task GenerateContractInvoice_WhenServiceThrows_ShouldSetErrorMessage_FailureGuard()
        {
            // Function: 12.8 — Generate Contract Invoice
            // Arrange
            var contractMock = new Mock<IContractInvoiceService>();
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            contractMock.Setup(s => s.GetPendingInvoicesAsync(8, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<BulkClaimRow> { new() { InvoiceId = 1, NetTotal = 50m } });
            contractMock.Setup(s => s.CreateContractInvoiceAsync(8, It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ThrowsAsync(new Exception("لا توجد فواتير معلقة لهذه الفترة"));

            var viewModel = new ContractInvoiceViewModel(contractMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.SelectedReferralId = 8;
            viewModel.InvoiceNumber = "INV-FAIL";
            viewModel.LoadPendingCommand.Execute(null);
            await Task.Delay(100);

            // Act
            viewModel.CreateInvoiceCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            viewModel.StatusMessage.Should().Contain("لا توجد فواتير معلقة");
        }

        [Fact]
        public async Task GenerateContractInvoice_LoadPending_ShouldComputeTotalSum_SuccessGuard()
        {
            // Function: 12.8 — Generate Contract Invoice (aggregation pre-display)
            // Arrange
            var contractMock = new Mock<IContractInvoiceService>();
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            contractMock.Setup(s => s.GetPendingInvoicesAsync(2, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<BulkClaimRow>
                {
                    new() { InvoiceId = 11, NetTotal = 100m },
                    new() { InvoiceId = 12, NetTotal = 250m },
                    new() { InvoiceId = 13, NetTotal = 75m }
                });
            var viewModel = new ContractInvoiceViewModel(contractMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.SelectedReferralId = 2;

            // Act
            viewModel.LoadPendingCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.PendingInvoices.Should().HaveCount(3);
            viewModel.TotalPending.Should().Be(425m);
            viewModel.StatusMessage.Should().Contain("تم تحميل 3");
        }

        [Fact]
        public async Task GenerateContractInvoice_LoadPending_WithEmptyResults_ShouldKeepListEmpty_EdgeGuard()
        {
            // Function: 12.8 — Generate Contract Invoice
            // Arrange
            var contractMock = new Mock<IContractInvoiceService>();
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            contractMock.Setup(s => s.GetPendingInvoicesAsync(3, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<BulkClaimRow>());
            var viewModel = new ContractInvoiceViewModel(contractMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.SelectedReferralId = 3;

            // Act
            viewModel.LoadPendingCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.PendingInvoices.Should().BeEmpty();
            viewModel.TotalPending.Should().Be(0m);
        }

        [Fact]
        public async Task GenerateContractInvoice_LoadReferrals_ShouldFilterContractTypesOnly_SuccessGuard()
        {
            // Function: 12.8 — Generate Contract Invoice (filter contract entities)
            // Arrange
            var contractMock = new Mock<IContractInvoiceService>();
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>
            {
                new() { ReferralId = 1, Name = "Comp1", ReferralType = "Company" },
                new() { ReferralId = 2, Name = "Ins1", ReferralType = "Insurance" },
                new() { ReferralId = 3, Name = "Hosp1", ReferralType = "Hospital" },
                new() { ReferralId = 4, Name = "Lab1", ReferralType = "ExternalLab" }
            });
            var viewModel = new ContractInvoiceViewModel(contractMock.Object, catalogMock.Object);

            // Act
            viewModel.LoadReferralsCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.Referrals.Should().HaveCount(3);
            viewModel.Referrals.Should().NotContain(r => r.ReferralId == 4); // ExternalLab excluded
        }

        // ==================================================================================
        // 12.9 — Settle Contract Account (تسوية حساب التعاقد) — ContractInvoiceViewModel
        // ==================================================================================

        [Fact]
        public async Task SettleContractAccount_WithSelectedUnpaidInvoice_ShouldCallServiceAndShowStatus_SuccessGuard()
        {
            // Function: 12.9 — Settle Contract Account
            // Arrange
            var contractMock = new Mock<IContractInvoiceService>();
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());

            var contractInvoice = new ContractInvoice { ContractInvoiceId = 99, ReferralId = 4, IsPaid = false };
            contractMock.Setup(s => s.SettleContractInvoiceAsync(99))
                .ReturnsAsync(new ContractInvoice { ContractInvoiceId = 99, ReferralId = 4, IsPaid = true });
            contractMock.Setup(s => s.GetContractInvoicesAsync(4))
                .ReturnsAsync(new List<ContractInvoice>
                {
                    new() { ContractInvoiceId = 99, ReferralId = 4, IsPaid = true }
                });

            var viewModel = new ContractInvoiceViewModel(contractMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.SelectedReferralId = 4;
            viewModel.SelectedContractInvoice = contractInvoice;

            // Act
            viewModel.SettleSelectedCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            contractMock.Verify(s => s.SettleContractInvoiceAsync(99), Times.Once);
            viewModel.StatusMessage.Should().Contain("تم تسوية");
        }

        [Fact]
        public async Task SettleContractAccount_WhenInvoiceAlreadyPaid_ShouldDisableSettleCommand_FailureGuard()
        {
            // Function: 12.9 — Settle Contract Account
            // Arrange
            var contractMock = new Mock<IContractInvoiceService>();
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            var viewModel = new ContractInvoiceViewModel(contractMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.SelectedContractInvoice = new ContractInvoice { ContractInvoiceId = 5, IsPaid = true };

            // Act
            var canExecute = viewModel.SettleSelectedCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse();
        }

        [Fact]
        public async Task SettleContractAccount_WithNoSelectedInvoice_ShouldDisableSettleCommand_EdgeGuard()
        {
            // Function: 12.9 — Settle Contract Account
            // Arrange
            var contractMock = new Mock<IContractInvoiceService>();
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            var viewModel = new ContractInvoiceViewModel(contractMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.SelectedContractInvoice = null;

            // Act
            var canExecute = viewModel.SettleSelectedCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse();
        }

        [Fact]
        public async Task SettleContractAccount_WhenServiceThrowsNotFound_ShouldSetErrorMessage_FailureGuard()
        {
            // Function: 12.9 — Settle Contract Account
            // Arrange
            var contractMock = new Mock<IContractInvoiceService>();
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            contractMock.Setup(s => s.SettleContractInvoiceAsync(33))
                .ThrowsAsync(new InvalidOperationException("فاتورة التعاقد غير موجودة"));

            var viewModel = new ContractInvoiceViewModel(contractMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.SelectedReferralId = 6;
            viewModel.SelectedContractInvoice = new ContractInvoice { ContractInvoiceId = 33, IsPaid = false };

            // Act
            viewModel.SettleSelectedCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            viewModel.StatusMessage.Should().Contain("فاتورة التعاقد غير موجودة");
        }

        [Fact]
        public async Task SettleContractAccount_LoadHistory_ShouldPopulateHistoricalInvoices_SuccessGuard()
        {
            // Function: 12.9 — Settle Contract Account (history)
            // Arrange
            var contractMock = new Mock<IContractInvoiceService>();
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            contractMock.Setup(s => s.GetContractInvoicesAsync(11))
                .ReturnsAsync(new List<ContractInvoice>
                {
                    new() { ContractInvoiceId = 1, ReferralId = 11, InvoiceNumber = "H-1", IsPaid = true },
                    new() { ContractInvoiceId = 2, ReferralId = 11, InvoiceNumber = "H-2", IsPaid = false }
                });
            var viewModel = new ContractInvoiceViewModel(contractMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.SelectedReferralId = 11;

            // Act
            viewModel.LoadHistoryCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.ContractInvoices.Should().HaveCount(2);
            viewModel.StatusMessage.Should().Contain("تم تحميل 2");
        }

        [Fact]
        public async Task SettleContractAccount_LoadHistory_WithoutSelectedReferral_ShouldDisableCommand_EdgeGuard()
        {
            // Function: 12.9 — Settle Contract Account
            // Arrange
            var contractMock = new Mock<IContractInvoiceService>();
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());
            var viewModel = new ContractInvoiceViewModel(contractMock.Object, catalogMock.Object);
            await Task.Delay(50);
            viewModel.SelectedReferralId = null;

            // Act
            var canExecute = viewModel.LoadHistoryCommand.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse();
        }

        // ==================================================================================
        // 12.5 — Assign Patient to Contract (ربط المريض بجهة التعاقد)
        // Note: This function is operationalized by setting Visit.ReferralId. The UI layer
        // handles this through PatientRegistration / Billing, but ReferralsViewModel manages
        // the contract entity itself. We cover the data-binding integrity here through
        // the model coverage tests in Module12ServiceTests_Additional.
        // ==================================================================================

        [Fact]
        public async Task DeleteContractEntity_WithSelectedReferral_ShouldCallServiceAndRemove_SuccessGuard()
        {
            // Function: 12.1 — Create Contract Entity (delete is the inverse operation)
            // Arrange
            var referral = new Referral { ReferralId = 33, Name = "ToDelete", ReferralType = "Company" };
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral> { referral });
            catalogMock.Setup(s => s.DeleteReferralAsync(33)).Returns(Task.CompletedTask);

            var viewModel = new ReferralsViewModel(catalogMock.Object);
            await Task.Delay(80);
            viewModel.SelectedReferral = referral;

            // Act
            viewModel.DeleteCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            catalogMock.Verify(s => s.DeleteReferralAsync(33), Times.Once);
            viewModel.Referrals.Should().NotContain(r => r.ReferralId == 33);
            viewModel.StatusMessage.Should().Be("تم حذف الجهة.");
        }

        [Fact]
        public async Task DeleteContractEntity_WhenInUse_ShouldShowErrorMessage_FailureGuard()
        {
            // Function: 12.1 — Create Contract Entity (delete protection)
            // Arrange
            var referral = new Referral { ReferralId = 44, Name = "InUse", ReferralType = "Company" };
            var catalogMock = new Mock<ITestCatalogService>();
            catalogMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral> { referral });
            catalogMock.Setup(s => s.DeleteReferralAsync(44))
                .ThrowsAsync(new InvalidOperationException("Cannot delete referral in use."));

            var viewModel = new ReferralsViewModel(catalogMock.Object);
            await Task.Delay(80);
            viewModel.SelectedReferral = referral;

            // Act
            viewModel.DeleteCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            viewModel.StatusMessage.Should().Contain("Cannot delete referral in use");
        }
    }
}
