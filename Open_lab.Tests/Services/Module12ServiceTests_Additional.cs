using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    /// <summary>
    /// Additional unit tests for Module 12 — Contracts & Referrals (جهات التعاقد والإحالة).
    /// Covers Functions 12.1 → 12.9 at the Service layer with Success / Failure / Edge scenarios.
    /// Uses real ContractInvoiceService, PhysicianService, and TestCatalogService against
    /// EF Core In-Memory provider to comply with the project testing standards.
    /// </summary>
    public class Module12ServiceTests_Additional : IDisposable
    {
        private readonly OpenLabDbContext _db;
        private readonly ContractInvoiceService _contractService;
        private readonly PhysicianService _physicianService;
        private readonly TestCatalogService _catalogService;
        private readonly PriceResolutionService _priceResolutionService;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public Module12ServiceTests_Additional()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _contractService = new ContractInvoiceService(_db);
            _physicianService = new PhysicianService(_db);
            _catalogService = new TestCatalogService(_db);
            _priceResolutionService = new PriceResolutionService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        // ==================================================================================
        // 12.1 — Create Contract Entity (إنشاء جهة تعاقد)
        // ==================================================================================

        [Fact]
        public async Task CreateContractEntity_WithValidCompanyData_ShouldPersistAndReturnEntityId_SuccessGuard()
        {
            // Function: 12.1 — Create Contract Entity
            // Arrange
            var referral = new Referral
            {
                Name = "شركة المستقبل للتأمين",
                ReferralType = "Insurance",
                Phone = "01234567890",
                City = "Cairo",
                DiscountPercentage = 15m,
                CommissionPercentage = 5m
            };

            // Act
            var created = await _catalogService.CreateReferralAsync(referral);

            // Assert
            created.ReferralId.Should().BeGreaterThan(0);
            var saved = await _db.Referrals.FindAsync(created.ReferralId);
            saved.Should().NotBeNull();
            saved!.Name.Should().Be("شركة المستقبل للتأمين");
            saved.ReferralType.Should().Be("Insurance");
            saved.Phone.Should().Be("01234567890");
            saved.City.Should().Be("Cairo");
            saved.DiscountPercentage.Should().Be(15m);
            saved.CommissionPercentage.Should().Be(5m);
        }

        [Fact]
        public async Task CreateContractEntity_WithEmptyName_ShouldThrowArgumentException_FailureGuard()
        {
            // Function: 12.1 — Create Contract Entity
            // Arrange
            var referral = new Referral { Name = "", ReferralType = "Company" };

            // Act
            Func<Task> act = async () => await _catalogService.CreateReferralAsync(referral);

            // Assert
            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.Which.Message.Should().Contain("name");
        }

        [Fact]
        public async Task CreateContractEntity_WithEmptyType_ShouldThrowArgumentException_FailureGuard()
        {
            // Function: 12.1 — Create Contract Entity
            // Arrange
            var referral = new Referral { Name = "Hospital A", ReferralType = "" };

            // Act
            Func<Task> act = async () => await _catalogService.CreateReferralAsync(referral);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task CreateContractEntity_WithDuplicateNameAndType_ShouldThrowInvalidOperation_FailureGuard()
        {
            // Function: 12.1 — Create Contract Entity
            // Arrange
            _db.Referrals.Add(new Referral { Name = "Acme", ReferralType = "Company" });
            await _db.SaveChangesAsync();
            var duplicate = new Referral { Name = "Acme", ReferralType = "Company" };

            // Act
            Func<Task> act = async () => await _catalogService.CreateReferralAsync(duplicate);

            // Assert
            var ex = await act.Should().ThrowAsync<InvalidOperationException>();
            ex.Which.Message.Should().Contain("already exists");
        }

        [Fact]
        public async Task CreateContractEntity_WithWhitespacePhoneAndCity_ShouldNormalizeToNull_EdgeGuard()
        {
            // Function: 12.1 — Create Contract Entity
            // Arrange
            var referral = new Referral
            {
                Name = "Edge Co",
                ReferralType = "Company",
                Phone = "   ",
                City = "   "
            };

            // Act
            var created = await _catalogService.CreateReferralAsync(referral);

            // Assert
            var saved = await _db.Referrals.FindAsync(created.ReferralId);
            saved.Should().NotBeNull();
            saved!.Phone.Should().BeNull();
            saved.City.Should().BeNull();
        }

        [Fact]
        public async Task CreateContractEntity_WithNullReferral_ShouldThrowArgumentException_FailureGuard()
        {
            // Function: 12.1 — Create Contract Entity
            // Arrange
            Referral? referral = null;

            // Act
            Func<Task> act = async () => await _catalogService.CreateReferralAsync(referral!);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        // ==================================================================================
        // 12.2 — Link Price List to Entity (ربط قائمة أسعار بالجهة)
        // ==================================================================================

        [Fact]
        public async Task LinkPriceListToEntity_WithValidReferralAndPriceList_ShouldPersistAssociation_SuccessGuard()
        {
            // Function: 12.2 — Link Price List to Entity
            // Arrange
            var referral = new Referral { Name = "Insurer", ReferralType = "Insurance" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var priceList = new PriceList { Name = "Insurer Discount List", ReferralId = referral.ReferralId };

            // Act
            var created = await _catalogService.CreatePriceListAsync(priceList);

            // Assert
            created.PriceListId.Should().BeGreaterThan(0);
            var saved = await _db.PriceLists.FirstOrDefaultAsync(p => p.PriceListId == created.PriceListId);
            saved.Should().NotBeNull();
            saved!.ReferralId.Should().Be(referral.ReferralId);
            saved.Name.Should().Be("Insurer Discount List");
        }

        [Fact]
        public async Task LinkPriceListToEntity_WithEmptyName_ShouldThrowArgumentException_FailureGuard()
        {
            // Function: 12.2 — Link Price List to Entity
            // Arrange
            var referral = new Referral { Name = "Insurer2", ReferralType = "Insurance" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();
            var priceList = new PriceList { Name = "", ReferralId = referral.ReferralId };

            // Act
            Func<Task> act = async () => await _catalogService.CreatePriceListAsync(priceList);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task LinkPriceListToEntity_WithNullReferralId_ShouldStillCreateGlobalList_EdgeGuard()
        {
            // Function: 12.2 — Link Price List to Entity
            // Arrange
            var priceList = new PriceList { Name = "Public List", ReferralId = null };

            // Act
            var created = await _catalogService.CreatePriceListAsync(priceList);

            // Assert
            created.PriceListId.Should().BeGreaterThan(0);
            var saved = await _db.PriceLists.FindAsync(created.PriceListId);
            saved.Should().NotBeNull();
            saved!.ReferralId.Should().BeNull();
        }

        [Fact]
        public async Task LinkPriceListToEntity_WhenResolvingPrice_ShouldApplyReferralPriceList_BR_ACC_002()
        {
            // Function: 12.2 — Link Price List to Entity (BR-ACC-002 Referral price list priority)
            // Arrange
            var referral = new Referral { Name = "RefBR2", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T001", NameReport = "CBC", NameReceipt = "CBC", Price = 100m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var priceList = new PriceList { Name = "RefBR2 PL", ReferralId = referral.ReferralId };
            _db.PriceLists.Add(priceList);
            await _db.SaveChangesAsync();

            _db.PriceListItems.Add(new PriceListItem { PriceListId = priceList.PriceListId, TestId = test.TestId, Price = 70m });
            await _db.SaveChangesAsync();

            // Act
            var (price, sourceType, _) = await _priceResolutionService.GetPriceSourceAsync(test.TestId, null, referral.ReferralId);

            // Assert
            price.Should().Be(70m);
            sourceType.Should().Be("ReferralPriceList");
        }

        // ==================================================================================
        // 12.3 — Set Entity Discount (ضبط خصم الجهة)
        // ==================================================================================

        [Fact]
        public async Task SetEntityDiscount_WithValidPercentage_ShouldUpdateReferralDiscount_SuccessGuard()
        {
            // Function: 12.3 — Set Entity Discount
            // Arrange
            var referral = new Referral { Name = "DiscountCo", ReferralType = "Company", DiscountPercentage = 0m };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            referral.DiscountPercentage = 25m;

            // Act
            await _catalogService.UpdateReferralAsync(referral);

            // Assert
            var saved = await _db.Referrals.FindAsync(referral.ReferralId);
            saved.Should().NotBeNull();
            saved!.DiscountPercentage.Should().Be(25m);
        }

        [Fact]
        public async Task SetEntityDiscount_WithDiscountAbove100_ShouldThrowArgumentException_FailureGuard()
        {
            // Function: 12.3 — Set Entity Discount
            // Arrange
            var referral = new Referral { Name = "DiscOver", ReferralType = "Company", DiscountPercentage = 5m };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            referral.DiscountPercentage = 150m;

            // Act
            Func<Task> act = async () => await _catalogService.UpdateReferralAsync(referral);

            // Assert
            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.Which.Message.Should().Contain("Discount");
        }

        [Fact]
        public async Task SetEntityDiscount_WithNegativeDiscount_ShouldThrowArgumentException_FailureGuard()
        {
            // Function: 12.3 — Set Entity Discount
            // Arrange
            var referral = new Referral { Name = "DiscNeg", ReferralType = "Company", DiscountPercentage = 5m };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            referral.DiscountPercentage = -1m;

            // Act
            Func<Task> act = async () => await _catalogService.UpdateReferralAsync(referral);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task SetEntityDiscount_WithZeroDiscount_ShouldAllow_EdgeGuard()
        {
            // Function: 12.3 — Set Entity Discount
            // Arrange
            var referral = new Referral { Name = "DiscZero", ReferralType = "Company", DiscountPercentage = 30m };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            referral.DiscountPercentage = 0m;

            // Act
            await _catalogService.UpdateReferralAsync(referral);

            // Assert
            var saved = await _db.Referrals.FindAsync(referral.ReferralId);
            saved!.DiscountPercentage.Should().Be(0m);
        }

        [Fact]
        public async Task SetEntityDiscount_WithExactly100Percent_ShouldAllow_EdgeGuard()
        {
            // Function: 12.3 — Set Entity Discount (BR-ACC-004 boundary value)
            // Arrange
            var referral = new Referral { Name = "Disc100", ReferralType = "Company", DiscountPercentage = 0m };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            referral.DiscountPercentage = 100m;

            // Act
            await _catalogService.UpdateReferralAsync(referral);

            // Assert
            var saved = await _db.Referrals.FindAsync(referral.ReferralId);
            saved!.DiscountPercentage.Should().Be(100m);
        }

        // ==================================================================================
        // 12.4 — Set Entity Commission (ضبط عمولة الجهة) — BR-ACC-008
        // ==================================================================================

        [Fact]
        public async Task SetEntityCommission_WithValidPercentage_ShouldPersistCommission_SuccessGuard_BR_ACC_008()
        {
            // Function: 12.4 — Set Entity Commission (BR-ACC-008)
            // Arrange
            var referral = new Referral { Name = "CommCo", ReferralType = "Company", CommissionPercentage = 0m };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            referral.CommissionPercentage = 12m;

            // Act
            await _catalogService.UpdateReferralAsync(referral);

            // Assert
            var saved = await _db.Referrals.FindAsync(referral.ReferralId);
            saved.Should().NotBeNull();
            saved!.CommissionPercentage.Should().Be(12m);
        }

        [Fact]
        public async Task SetEntityCommission_WithCommissionAbove100_ShouldThrowArgumentException_FailureGuard_BR_ACC_008()
        {
            // Function: 12.4 — Set Entity Commission (BR-ACC-008)
            // Arrange
            var referral = new Referral { Name = "CommOver", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            referral.CommissionPercentage = 105m;

            // Act
            Func<Task> act = async () => await _catalogService.UpdateReferralAsync(referral);

            // Assert
            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.Which.Message.Should().Contain("Commission");
        }

        [Fact]
        public async Task SetEntityCommission_WithNegativeCommission_ShouldThrowArgumentException_FailureGuard()
        {
            // Function: 12.4 — Set Entity Commission
            // Arrange
            var referral = new Referral { Name = "CommNeg", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            referral.CommissionPercentage = -5m;

            // Act
            Func<Task> act = async () => await _catalogService.UpdateReferralAsync(referral);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task SetEntityCommission_WithExactly100Percent_ShouldAllow_EdgeGuard_BR_ACC_008()
        {
            // Function: 12.4 — Set Entity Commission (BR-ACC-008 boundary value)
            // Arrange
            var referral = new Referral { Name = "Comm100", ReferralType = "Company", CommissionPercentage = 0m };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            referral.CommissionPercentage = 100m;

            // Act
            await _catalogService.UpdateReferralAsync(referral);

            // Assert
            var saved = await _db.Referrals.FindAsync(referral.ReferralId);
            saved!.CommissionPercentage.Should().Be(100m);
        }

        [Fact]
        public async Task SetEntityCommission_WithZeroCommission_ShouldAllow_EdgeGuard()
        {
            // Function: 12.4 — Set Entity Commission
            // Arrange
            var referral = new Referral { Name = "CommZero", ReferralType = "Company", CommissionPercentage = 50m };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            referral.CommissionPercentage = 0m;

            // Act
            await _catalogService.UpdateReferralAsync(referral);

            // Assert
            var saved = await _db.Referrals.FindAsync(referral.ReferralId);
            saved!.CommissionPercentage.Should().Be(0m);
        }

        // ==================================================================================
        // 12.5 — Assign Patient to Contract (ربط المريض بجهة التعاقد)
        // ==================================================================================

        [Fact]
        public async Task AssignPatientToContract_WithValidReferralOnVisit_ShouldPersistLink_SuccessGuard()
        {
            // Function: 12.5 — Assign Patient to Contract
            // Arrange
            var referral = new Referral { Name = "AssignRef", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "L-A1", FullName = "Patient A", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = referral.ReferralId };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Act
            var loaded = await _db.Visits.Include(v => v.Referral).FirstOrDefaultAsync(v => v.VisitId == visit.VisitId);

            // Assert
            loaded.Should().NotBeNull();
            loaded!.ReferralId.Should().Be(referral.ReferralId);
            loaded.Referral.Should().NotBeNull();
            loaded.Referral!.Name.Should().Be("AssignRef");
        }

        [Fact]
        public async Task AssignPatientToContract_WhenReferralHasPriceList_ShouldApplyContractPricing_SuccessGuard_BR_ACC_002()
        {
            // Function: 12.5 — Assign Patient to Contract (BR-ACC-002 contract pricing)
            // Arrange
            var referral = new Referral { Name = "ContractPrice", ReferralType = "Insurance" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "TX", NameReport = "Test X", NameReceipt = "TX", Price = 200m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var contractPriceList = new PriceList { Name = "ContractPL", ReferralId = referral.ReferralId };
            _db.PriceLists.Add(contractPriceList);
            await _db.SaveChangesAsync();

            _db.PriceListItems.Add(new PriceListItem { PriceListId = contractPriceList.PriceListId, TestId = test.TestId, Price = 120m });
            await _db.SaveChangesAsync();

            // Act
            var price = await _priceResolutionService.ResolvePriceAsync(test.TestId, null, referral.ReferralId);

            // Assert
            price.Should().Be(120m);
        }

        [Fact]
        public async Task AssignPatientToContract_WithoutReferralAssignment_ShouldUseBasePrice_EdgeGuard()
        {
            // Function: 12.5 — Assign Patient to Contract
            // Arrange
            var test = new Test { Code = "TY", NameReport = "Test Y", NameReceipt = "TY", Price = 80m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            // Act - patient with no referral
            var (price, sourceType, _) = await _priceResolutionService.GetPriceSourceAsync(test.TestId, null, null);

            // Assert
            price.Should().Be(80m);
            sourceType.Should().Be("TestBasePrice");
        }

        // ==================================================================================
        // 12.6 — Add Referring Physician (إضافة طبيب محيل)
        // ==================================================================================

        [Fact]
        public async Task AddReferringPhysician_WithValidData_ShouldCreateAndReturnDoctorId_SuccessGuard()
        {
            // Function: 12.6 — Add Referring Physician
            // Arrange
            var physician = new Physician
            {
                FullName = "د. أحمد محمد",
                Phone = "01099887766",
                Specialty = "Internal Medicine",
                Address = "Cairo",
                IsActive = true,
                CommissionPercentage = 8m
            };

            // Act
            var created = await _physicianService.CreateAsync(physician);

            // Assert
            created.PhysicianId.Should().BeGreaterThan(0);
            var saved = await _db.Physicians.FindAsync(created.PhysicianId);
            saved.Should().NotBeNull();
            saved!.FullName.Should().Be("د. أحمد محمد");
            saved.Specialty.Should().Be("Internal Medicine");
            saved.Phone.Should().Be("01099887766");
            saved.IsActive.Should().BeTrue();
            saved.CommissionPercentage.Should().Be(8m);
        }

        [Fact]
        public async Task AddReferringPhysician_WithEmptyFullName_ShouldThrowArgumentException_FailureGuard()
        {
            // Function: 12.6 — Add Referring Physician
            // Arrange
            var physician = new Physician { FullName = "" };

            // Act
            Func<Task> act = async () => await _catalogService.CreatePhysicianAsync(physician);

            // Assert
            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.Which.Message.Should().Contain("full name");
        }

        [Fact]
        public async Task AddReferringPhysician_WithNegativeCommission_ShouldThrowArgumentException_FailureGuard()
        {
            // Function: 12.6 — Add Referring Physician
            // Arrange
            var physician = new Physician { FullName = "Dr. Negative", CommissionPercentage = -3m };

            // Act
            Func<Task> act = async () => await _catalogService.CreatePhysicianAsync(physician);

            // Assert
            var ex = await act.Should().ThrowAsync<ArgumentException>();
            ex.Which.Message.Should().Contain("Commission");
        }

        [Fact]
        public async Task AddReferringPhysician_WithDuplicateNameAndPhone_ShouldThrowInvalidOperation_FailureGuard()
        {
            // Function: 12.6 — Add Referring Physician
            // Arrange
            _db.Physicians.Add(new Physician { FullName = "Dr. Same", Phone = "0100" });
            await _db.SaveChangesAsync();
            var dup = new Physician { FullName = "Dr. Same", Phone = "0100" };

            // Act
            Func<Task> act = async () => await _catalogService.CreatePhysicianAsync(dup);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task AddReferringPhysician_WithMinimalData_ShouldDefaultIsActiveTrue_EdgeGuard()
        {
            // Function: 12.6 — Add Referring Physician
            // Arrange
            var physician = new Physician { FullName = "Dr. Minimal" };

            // Act
            var created = await _physicianService.CreateAsync(physician);

            // Assert
            var saved = await _db.Physicians.FindAsync(created.PhysicianId);
            saved.Should().NotBeNull();
            saved!.IsActive.Should().BeTrue();
            saved.Phone.Should().BeNull();
            saved.Specialty.Should().BeNull();
        }

        // ==================================================================================
        // 12.7 — Set Physician Price List (ضبط قائمة أسعار الطبيب)
        // ==================================================================================

        [Fact]
        public async Task SetPhysicianPriceList_WithValidPriceList_ShouldPersistLink_SuccessGuard()
        {
            // Function: 12.7 — Set Physician Price List
            // Arrange
            var priceList = new PriceList { Name = "Doctor PL" };
            _db.PriceLists.Add(priceList);
            await _db.SaveChangesAsync();

            var physician = new Physician { FullName = "Dr. PL" };
            _db.Physicians.Add(physician);
            await _db.SaveChangesAsync();

            physician.PriceListId = priceList.PriceListId;

            // Act
            await _physicianService.UpdateAsync(physician);

            // Assert
            var saved = await _db.Physicians.Include(p => p.PriceList).FirstOrDefaultAsync(p => p.PhysicianId == physician.PhysicianId);
            saved.Should().NotBeNull();
            saved!.PriceListId.Should().Be(priceList.PriceListId);
            saved.PriceList.Should().NotBeNull();
            saved.PriceList!.Name.Should().Be("Doctor PL");
        }

        [Fact]
        public async Task SetPhysicianPriceList_PhysicianPriceShouldOverrideContractPrice_BR_ACC_001()
        {
            // Function: 12.7 — Set Physician Price List (BR-ACC-001 physician price priority over contract)
            // Note: According to PriceResolutionService priority: Physician (1) > Referral (2) > Base (3)
            // Arrange
            var referral = new Referral { Name = "RefForOverride", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "TZ", NameReport = "Test Z", NameReceipt = "TZ", Price = 300m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            // Referral price list (lower priority)
            var refPL = new PriceList { Name = "RefPL", ReferralId = referral.ReferralId };
            _db.PriceLists.Add(refPL);
            await _db.SaveChangesAsync();
            _db.PriceListItems.Add(new PriceListItem { PriceListId = refPL.PriceListId, TestId = test.TestId, Price = 250m });

            // Physician price list (higher priority)
            var docPL = new PriceList { Name = "DocPL" };
            _db.PriceLists.Add(docPL);
            await _db.SaveChangesAsync();
            _db.PriceListItems.Add(new PriceListItem { PriceListId = docPL.PriceListId, TestId = test.TestId, Price = 200m });
            await _db.SaveChangesAsync();

            var physician = new Physician { FullName = "Dr. Priority", PriceListId = docPL.PriceListId };
            _db.Physicians.Add(physician);
            await _db.SaveChangesAsync();

            // Act - both physician and referral provided; physician should win in current implementation
            var (price, sourceType, _) = await _priceResolutionService.GetPriceSourceAsync(
                test.TestId, physician.PhysicianId, referral.ReferralId);

            // Assert
            price.Should().Be(200m);
            sourceType.Should().Be("PhysicianPriceList");
        }

        [Fact]
        public async Task SetPhysicianPriceList_WithNullPriceList_ShouldFallbackToBasePrice_EdgeGuard()
        {
            // Function: 12.7 — Set Physician Price List
            // Arrange
            var test = new Test { Code = "TF", NameReport = "Fallback", NameReceipt = "TF", Price = 50m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var physician = new Physician { FullName = "Dr. NoPL", PriceListId = null };
            _db.Physicians.Add(physician);
            await _db.SaveChangesAsync();

            // Act
            var (price, sourceType, _) = await _priceResolutionService.GetPriceSourceAsync(
                test.TestId, physician.PhysicianId, null);

            // Assert
            price.Should().Be(50m);
            sourceType.Should().Be("TestBasePrice");
        }

        [Fact]
        public async Task SetPhysicianPriceList_WhenChangedToNull_ShouldRemoveAssociation_EdgeGuard()
        {
            // Function: 12.7 — Set Physician Price List
            // Arrange
            var pl = new PriceList { Name = "Temp" };
            _db.PriceLists.Add(pl);
            await _db.SaveChangesAsync();

            var physician = new Physician { FullName = "Dr. Detach", PriceListId = pl.PriceListId };
            _db.Physicians.Add(physician);
            await _db.SaveChangesAsync();

            physician.PriceListId = null;

            // Act
            await _physicianService.UpdateAsync(physician);

            // Assert
            var saved = await _db.Physicians.FindAsync(physician.PhysicianId);
            saved!.PriceListId.Should().BeNull();
        }

        // ==================================================================================
        // 12.8 — Generate Contract Invoice (إصدار فاتورة التعاقد)
        // ==================================================================================

        [Fact]
        public async Task GenerateContractInvoice_WithMultiplePendingInvoices_ShouldAggregateAllAmounts_SuccessGuard()
        {
            // Function: 12.8 — Generate Contract Invoice
            // Arrange
            var referral = new Referral { Name = "GenInv", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "GI-1", FullName = "P-GI", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var v1 = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = referral.ReferralId };
            var v2 = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(1), ReferralId = referral.ReferralId };
            var v3 = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(2), ReferralId = referral.ReferralId };
            _db.Visits.AddRange(v1, v2, v3);
            await _db.SaveChangesAsync();

            _db.Invoices.AddRange(
                new Invoice { VisitId = v1.VisitId, Total = 100m, Discount = 10m, NetTotal = 90m },
                new Invoice { VisitId = v2.VisitId, Total = 200m, Discount = 20m, NetTotal = 180m },
                new Invoice { VisitId = v3.VisitId, Total = 300m, Discount = 30m, NetTotal = 270m });
            await _db.SaveChangesAsync();

            // Act
            var contractId = await _contractService.CreateContractInvoiceAsync(
                referral.ReferralId, "CONT-AGG", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(3));

            // Assert
            var contract = await _db.ContractInvoices.FindAsync(contractId);
            contract.Should().NotBeNull();
            contract!.TotalAmount.Should().Be(600m);
            contract.DiscountAmount.Should().Be(60m);
            contract.NetAmount.Should().Be(540m);
            contract.IsPaid.Should().BeFalse();

            // All invoices linked
            var linked = await _db.Invoices.CountAsync(i => i.ContractInvoiceId == contractId);
            linked.Should().Be(3);
        }

        [Fact]
        public async Task GenerateContractInvoice_WithNoPendingInvoicesInPeriod_ShouldThrow_FailureGuard()
        {
            // Function: 12.8 — Generate Contract Invoice
            // Arrange
            var referral = new Referral { Name = "EmptyPeriod", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _contractService.CreateContractInvoiceAsync(
                referral.ReferralId, "CONT-EMPTY", DateTime.Today.AddDays(-1), DateTime.Today);

            // Assert
            var ex = await act.Should().ThrowAsync<Exception>();
            ex.Which.Message.Should().Contain("لا توجد فواتير معلقة");
        }

        [Fact]
        public async Task GenerateContractInvoice_WithNonExistentReferral_ShouldThrow_FailureGuard()
        {
            // Function: 12.8 — Generate Contract Invoice
            // Act
            Func<Task> act = async () => await _contractService.CreateContractInvoiceAsync(
                999999, "CONT-NF", DateTime.Today.AddDays(-1), DateTime.Today);

            // Assert
            var ex = await act.Should().ThrowAsync<Exception>();
            ex.Which.Message.Should().Contain("الجهة غير موجودة");
        }

        [Fact]
        public async Task GenerateContractInvoice_ShouldOnlyIncludeUnassignedInvoices_EdgeGuard()
        {
            // Function: 12.8 — Generate Contract Invoice
            // Arrange
            var referral = new Referral { Name = "UnAssigned", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "U-1", FullName = "U", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var v1 = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = referral.ReferralId };
            var v2 = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(1), ReferralId = referral.ReferralId };
            _db.Visits.AddRange(v1, v2);
            await _db.SaveChangesAsync();

            // First invoice is already part of an existing contract
            var existingContract = new ContractInvoice
            {
                ReferralId = referral.ReferralId,
                InvoiceNumber = "OLD",
                DateFrom = DateTime.Today,
                DateTo = DateTime.Today,
                TotalAmount = 100m,
                NetAmount = 100m,
                CreatedAt = DateTime.Now
            };
            _db.ContractInvoices.Add(existingContract);
            await _db.SaveChangesAsync();

            _db.Invoices.AddRange(
                new Invoice { VisitId = v1.VisitId, Total = 100m, Discount = 0m, NetTotal = 100m, ContractInvoiceId = existingContract.ContractInvoiceId },
                new Invoice { VisitId = v2.VisitId, Total = 50m, Discount = 0m, NetTotal = 50m, ContractInvoiceId = null });
            await _db.SaveChangesAsync();

            // Act
            var newContractId = await _contractService.CreateContractInvoiceAsync(
                referral.ReferralId, "CONT-FILTER", DateTime.Today.AddDays(-1), DateTime.Today.AddDays(2));

            // Assert
            var newContract = await _db.ContractInvoices.FindAsync(newContractId);
            newContract.Should().NotBeNull();
            newContract!.TotalAmount.Should().Be(50m);
            newContract.NetAmount.Should().Be(50m);
        }

        [Fact]
        public async Task GenerateContractInvoice_OutsideDateRange_ShouldNotIncludeOlderInvoices_EdgeGuard()
        {
            // Function: 12.8 — Generate Contract Invoice
            // Arrange
            var referral = new Referral { Name = "RangeRef", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var patient = new Patient { LabId = "R-1", FullName = "R", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visitInRange = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = referral.ReferralId };
            var visitOutOfRange = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(-30), ReferralId = referral.ReferralId };
            _db.Visits.AddRange(visitInRange, visitOutOfRange);
            await _db.SaveChangesAsync();

            _db.Invoices.AddRange(
                new Invoice { VisitId = visitInRange.VisitId, Total = 100m, Discount = 0m, NetTotal = 100m },
                new Invoice { VisitId = visitOutOfRange.VisitId, Total = 999m, Discount = 0m, NetTotal = 999m });
            await _db.SaveChangesAsync();

            // Act
            var contractId = await _contractService.CreateContractInvoiceAsync(
                referral.ReferralId, "CONT-RANGE", DateTime.Today.AddDays(-7), DateTime.Today.AddDays(1));

            // Assert
            var contract = await _db.ContractInvoices.FindAsync(contractId);
            contract.Should().NotBeNull();
            contract!.TotalAmount.Should().Be(100m);
            contract.NetAmount.Should().Be(100m);
        }

        // ==================================================================================
        // 12.9 — Settle Contract Account (تسوية حساب التعاقد)
        // ==================================================================================

        [Fact]
        public async Task SettleContractAccount_WithValidContractId_ShouldMarkAsPaidAndPersist_SuccessGuard()
        {
            // Function: 12.9 — Settle Contract Account
            // Arrange
            var referral = new Referral { Name = "SettleRef", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var contract = new ContractInvoice
            {
                ReferralId = referral.ReferralId,
                InvoiceNumber = "S-1",
                DateFrom = DateTime.Today.AddDays(-5),
                DateTo = DateTime.Today,
                TotalAmount = 1000m,
                DiscountAmount = 100m,
                NetAmount = 900m,
                IsPaid = false,
                CreatedAt = DateTime.Now
            };
            _db.ContractInvoices.Add(contract);
            await _db.SaveChangesAsync();

            // Act
            var settled = await _contractService.SettleContractInvoiceAsync(contract.ContractInvoiceId);

            // Assert
            settled.IsPaid.Should().BeTrue();
            var saved = await _db.ContractInvoices.FindAsync(contract.ContractInvoiceId);
            saved.Should().NotBeNull();
            saved!.IsPaid.Should().BeTrue();
            saved.NetAmount.Should().Be(900m);
            saved.InvoiceNumber.Should().Be("S-1");
        }

        [Fact]
        public async Task SettleContractAccount_WithNonExistentContractId_ShouldThrowInvalidOperation_FailureGuard()
        {
            // Function: 12.9 — Settle Contract Account
            // Act
            Func<Task> act = async () => await _contractService.SettleContractInvoiceAsync(999999);

            // Assert
            var ex = await act.Should().ThrowAsync<InvalidOperationException>();
            ex.Which.Message.Should().Contain("فاتورة التعاقد غير موجودة");
        }

        [Fact]
        public async Task SettleContractAccount_WhenAlreadyPaid_ShouldRemainPaid_EdgeGuard()
        {
            // Function: 12.9 — Settle Contract Account
            // Arrange - already paid contract
            var referral = new Referral { Name = "AlreadyPaid", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var contract = new ContractInvoice
            {
                ReferralId = referral.ReferralId,
                InvoiceNumber = "AP-1",
                DateFrom = DateTime.Today.AddDays(-5),
                DateTo = DateTime.Today,
                TotalAmount = 500m,
                DiscountAmount = 0m,
                NetAmount = 500m,
                IsPaid = true,
                CreatedAt = DateTime.Now
            };
            _db.ContractInvoices.Add(contract);
            await _db.SaveChangesAsync();

            // Act
            var settled = await _contractService.SettleContractInvoiceAsync(contract.ContractInvoiceId);

            // Assert - operation idempotent (still paid, no exception)
            settled.IsPaid.Should().BeTrue();
            settled.NetAmount.Should().Be(500m);
        }

        [Fact]
        public async Task SettleContractAccount_ShouldOnlyChangePaidFlag_NotFinancialAmounts_EdgeGuard()
        {
            // Function: 12.9 — Settle Contract Account (financial integrity)
            // Arrange
            var referral = new Referral { Name = "IntegrityRef", ReferralType = "Company" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var contract = new ContractInvoice
            {
                ReferralId = referral.ReferralId,
                InvoiceNumber = "INT-1",
                DateFrom = DateTime.Today.AddDays(-10),
                DateTo = DateTime.Today,
                TotalAmount = 5000m,
                DiscountAmount = 500m,
                NetAmount = 4500m,
                IsPaid = false,
                CreatedAt = DateTime.Now
            };
            _db.ContractInvoices.Add(contract);
            await _db.SaveChangesAsync();

            // Act
            var settled = await _contractService.SettleContractInvoiceAsync(contract.ContractInvoiceId);

            // Assert - financial fields untouched
            settled.IsPaid.Should().BeTrue();
            settled.TotalAmount.Should().Be(5000m);
            settled.DiscountAmount.Should().Be(500m);
            settled.NetAmount.Should().Be(4500m);
            settled.InvoiceNumber.Should().Be("INT-1");
        }

        [Fact]
        public async Task GetContractInvoicesAsync_FilterByReferral_ShouldOnlyReturnReferralInvoices_SuccessGuard()
        {
            // Function: 12.9 — Settle Contract Account (history retrieval)
            // Arrange
            var refA = new Referral { Name = "RefA", ReferralType = "Company" };
            var refB = new Referral { Name = "RefB", ReferralType = "Company" };
            _db.Referrals.AddRange(refA, refB);
            await _db.SaveChangesAsync();

            _db.ContractInvoices.AddRange(
                new ContractInvoice { ReferralId = refA.ReferralId, InvoiceNumber = "A-1", DateFrom = DateTime.Today.AddDays(-3), DateTo = DateTime.Today, TotalAmount = 10m, NetAmount = 10m, CreatedAt = DateTime.Now },
                new ContractInvoice { ReferralId = refA.ReferralId, InvoiceNumber = "A-2", DateFrom = DateTime.Today.AddDays(-2), DateTo = DateTime.Today, TotalAmount = 20m, NetAmount = 20m, CreatedAt = DateTime.Now },
                new ContractInvoice { ReferralId = refB.ReferralId, InvoiceNumber = "B-1", DateFrom = DateTime.Today.AddDays(-1), DateTo = DateTime.Today, TotalAmount = 30m, NetAmount = 30m, CreatedAt = DateTime.Now });
            await _db.SaveChangesAsync();

            // Act
            var refAInvoices = await _contractService.GetContractInvoicesAsync(refA.ReferralId);

            // Assert
            refAInvoices.Should().HaveCount(2);
            refAInvoices.Should().OnlyContain(i => i.ReferralId == refA.ReferralId);
            refAInvoices.Select(i => i.InvoiceNumber).Should().BeEquivalentTo(new[] { "A-1", "A-2" });
        }
    }
}
