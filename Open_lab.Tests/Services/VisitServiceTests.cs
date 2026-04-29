using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class VisitServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly VisitService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public VisitServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new VisitService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task AddTestToVisitAsync_Should_Add_VisitTest_With_Price()
        {
            // Function: 1.3 — Add Tests to Patient
            // Arrange
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T1", NameReport = "Test", Price = 100m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            // Act
            var vt = await _service.AddTestToVisitAsync(visit.VisitId, test.TestId);

            // Assert
            vt.Should().NotBeNull();
            vt.Price.Should().Be(100m);
            vt.Status.Should().Be("Pending");
            var persisted = await _db.VisitTests.SingleAsync(x => x.VisitTestId == vt.VisitTestId);
            persisted.VisitId.Should().Be(visit.VisitId);
            persisted.TestId.Should().Be(test.TestId);
            persisted.Price.Should().Be(100m);
        }

        [Fact]
        public async Task AddTestToVisitAsync_SendOutTest_Should_Register_ExternalQueue()
        {
            // Function: 1.3 — Add Tests to Patient
            var patient = new Patient { LabId = "L-EXT", FullName = "Patient", Gender = "Male" };
            var referral = new Referral { Name = "Ref Lab", ReferralType = "ExternalLab" };
            _db.Patients.Add(patient);
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var visit = new Visit
            {
                PatientId = patient.PatientId,
                VisitDate = DateTime.Now,
                ReferralId = referral.ReferralId,
                AccountType = "Referral"
            };
            _db.Visits.Add(visit);

            var test = new Test
            {
                Code = "EXT1",
                NameReport = "Send Out Test",
                Price = 75m,
                IsSendOut = true
            };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visitTest = await _service.AddTestToVisitAsync(visit.VisitId, test.TestId);

            var queue = await _db.ExternalLabQueues.SingleAsync(q => q.VisitTestId == visitTest.VisitTestId);
            queue.ReferralId.Should().Be(referral.ReferralId);
            queue.Status.Should().Be("Pending");
        }

        [Fact]
        public async Task AddTestToVisitAsync_Duplicate_Should_Throw()
        {
            // Function: 1.3 — Add Tests to Patient
            // Arrange
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T1", NameReport = "Test", Price = 100m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 100m });
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddTestToVisitAsync(visit.VisitId, test.TestId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            var count = await _db.VisitTests.CountAsync(v => v.VisitId == visit.VisitId && v.TestId == test.TestId);
            count.Should().Be(1);
        }

        [Fact]
        public async Task AddTestToVisitAsync_Visit_Not_Found_Should_Throw()
        {
            // Function: 1.3 — Add Tests to Patient
            // Arrange - FAILURE test for AddTestToVisitAsync when visit not found
            var test = new Test { Code = "T1", NameReport = "Test", Price = 100m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddTestToVisitAsync(99999, test.TestId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Visit not found*");
        }

        [Fact]
        public async Task AddTestToVisitAsync_Visit_Closed_Should_Throw()
        {
            // Function: 1.3 — Add Tests to Patient
            // Arrange - FAILURE test for AddTestToVisitAsync when visit is closed
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Closed" };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var test = new Test { Code = "T1", NameReport = "Test", Price = 100m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddTestToVisitAsync(visit.VisitId, test.TestId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Closed visits cannot accept new tests*");
        }

        [Fact]
        public async Task AddTestToVisitAsync_Test_Not_Found_Should_Throw()
        {
            // Function: 1.3 — Add Tests to Patient
            // Arrange - FAILURE test for AddTestToVisitAsync when test not found
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddTestToVisitAsync(visit.VisitId, 99999);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Test not found*");
        }

        [Fact]
        public async Task RemoveVisitTestAsync_Verified_Should_Throw()
        {
            // Function: 1.4 — Delete Tests
            // Arrange
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 50m, Status = "Verified" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.RemoveVisitTestAsync(vt.VisitTestId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            var stillExists = await _db.VisitTests.AnyAsync(v => v.VisitTestId == vt.VisitTestId);
            stillExists.Should().BeTrue();
        }

        [Fact]
        public async Task RemoveVisitTestAsync_Pending_Should_Delete_Successfully()
        {
            // Function: 1.4 — Delete Tests
            // Arrange
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Open" };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 50m, Status = "Pending" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            await _service.RemoveVisitTestAsync(vt.VisitTestId);

            // Assert
            var deleted = await _db.VisitTests.AnyAsync(v => v.VisitTestId == vt.VisitTestId);
            deleted.Should().BeFalse();
        }

        [Fact]
        public async Task RemoveVisitTestAsync_Visit_Closed_Should_Throw()
        {
            // Function: 1.4 — Delete Tests
            // Arrange - FAILURE test for RemoveVisitTestAsync when visit is closed
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Closed" };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var vt = new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 50m, Status = "Pending" };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.RemoveVisitTestAsync(vt.VisitTestId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Closed visits cannot remove tests*");
            var stillExists = await _db.VisitTests.AnyAsync(v => v.VisitTestId == vt.VisitTestId);
            stillExists.Should().BeTrue();
        }

        [Fact]
        public async Task RemoveVisitTestAsync_VisitTest_Not_Found_Should_Not_Throw()
        {
            // Function: 1.4 — Delete Tests
            // Arrange - FAILURE test for RemoveVisitTestAsync when visit test not found
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Act
            await _service.RemoveVisitTestAsync(99999);

            // Assert - Should not throw, method returns silently
            var count = await _db.VisitTests.CountAsync();
            count.Should().Be(0);
        }

        [Fact]
        public async Task ResolveTestPriceAsync_With_Referral_PriceList_Should_Use_Contract_Price()
        {
            // Function: 1.3 — Add Tests to Patient
            // Arrange
            var referral = new Referral { Name = "Contract1" };
            _db.Referrals.Add(referral);
            var patient = new Patient { LabId = "L10", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            var test = new Test { Code = "T1", NameReport = "Test", Price = 200m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var pl = new PriceList { Name = "PL1", ReferralId = referral.ReferralId };
            _db.PriceLists.Add(pl);
            await _db.SaveChangesAsync();
            _db.PriceListItems.Add(new PriceListItem { PriceListId = pl.PriceListId, TestId = test.TestId, Price = 150m });
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, ReferralId = referral.ReferralId, AccountType = "Referral" };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Act
            var vt = await _service.AddTestToVisitAsync(visit.VisitId, test.TestId);

            // Assert
            vt.Price.Should().Be(150m); // Contract price instead of base 200m
        }

        [Fact]
        public async Task ResolveTestPriceAsync_With_DefaultPriceList_Should_Use_DefaultPrice()
        {
            // Function: 1.3 — Add Tests to Patient
            // Arrange
            var patient = new Patient { LabId = "L-DEF", FullName = "Default Price Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            var test = new Test { Code = "T-DEF", NameReport = "Default Test", Price = 300m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var defaultList = new PriceList { Name = "Default", IsDefault = true };
            _db.PriceLists.Add(defaultList);
            await _db.SaveChangesAsync();
            _db.PriceListItems.Add(new PriceListItem
            {
                PriceListId = defaultList.PriceListId,
                TestId = test.TestId,
                Price = 210m
            });
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, AccountType = "Cash" };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Act
            var visitTest = await _service.AddTestToVisitAsync(visit.VisitId, test.TestId);

            // Assert
            visitTest.Price.Should().Be(210m);
        }

        [Fact]
        public async Task ResolveTestPriceAsync_With_PhysicianAssigned_Should_Fallback_To_DefaultPrice_ProductionGap()
        {
            // Function: 1.3 — Add Tests to Patient
            // Arrange
            var physician = new Physician { FullName = "Dr. Sameh" };
            var patient = new Patient { LabId = "L-DR", FullName = "Doctor Pricing Patient", Gender = "Female" };
            _db.Physicians.Add(physician);
            _db.Patients.Add(patient);

            var test = new Test { Code = "T-DR", NameReport = "Doctor Pricing Test", Price = 400m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var defaultList = new PriceList { Name = "Default", IsDefault = true };
            _db.PriceLists.Add(defaultList);
            await _db.SaveChangesAsync();

            _db.PriceListItems.Add(new PriceListItem
            {
                PriceListId = defaultList.PriceListId,
                TestId = test.TestId,
                Price = 250m
            });
            await _db.SaveChangesAsync();

            var visit = new Visit
            {
                PatientId = patient.PatientId,
                VisitDate = DateTime.Now,
                PhysicianId = physician.PhysicianId,
                AccountType = "Cash"
            };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Act
            var visitTest = await _service.AddTestToVisitAsync(visit.VisitId, test.TestId);

            // Assert
            visitTest.Price.Should().Be(250m, "Doctor-specific pricing is not implemented in production, so default pricing is applied.");
        }

        [Fact]
        public async Task CreateAsync_With_Referral_Account_And_No_Referral_Should_Throw()
        {
            var patient = new Patient { LabId = "L-AR1", FullName = "Referral Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            Func<Task> act = async () => await _service.CreateAsync(new Visit
            {
                PatientId = patient.PatientId,
                VisitDate = DateTime.Now,
                AccountType = "Referral",
                ReferralId = null
            });

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*requires a referral*");
        }

        [Fact]
        public async Task CreateAsync_With_Referral_Account_Should_Persist_Referral_Binding()
        {
            var patient = new Patient { LabId = "L-AR2", FullName = "Referral Patient 2", Gender = "Female" };
            var referral = new Referral { Name = "Insurance-X", ReferralType = "Insurance" };
            _db.Patients.Add(patient);
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var created = await _service.CreateAsync(new Visit
            {
                PatientId = patient.PatientId,
                VisitDate = DateTime.Today,
                AccountType = "Referral",
                ReferralId = referral.ReferralId
            });

            created.AccountType.Should().Be("Referral");
            created.ReferralId.Should().Be(referral.ReferralId);
            created.Status.Should().Be("Open");

            var persisted = await _db.Visits.SingleAsync(v => v.VisitId == created.VisitId);
            persisted.PatientId.Should().Be(patient.PatientId);
            persisted.ReferralId.Should().Be(referral.ReferralId);
        }

        [Fact]
        public async Task AddCustomGroupToVisitAsync_Visit_Not_Found_Should_Throw()
        {
            // Function: 1.8 — Add Group of Tests
            // Arrange - FAILURE test for AddCustomGroupToVisitAsync when visit not found
            var group = new CustomGroup { CustomGroupId = 1, Name = "Group" };
            _db.CustomGroups.Add(group);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddCustomGroupToVisitAsync(99999, 1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Visit not found*");
        }

        [Fact]
        public async Task AddCustomGroupToVisitAsync_Visit_Closed_Should_Throw()
        {
            // Function: 1.8 — Add Group of Tests
            // Arrange - FAILURE test for AddCustomGroupToVisitAsync when visit is closed
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now, Status = "Closed" };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var group = new CustomGroup { CustomGroupId = 1, Name = "Group" };
            _db.CustomGroups.Add(group);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddCustomGroupToVisitAsync(visit.VisitId, 1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Closed visits cannot accept new tests*");
        }

        [Fact]
        public async Task AddCustomGroupToVisitAsync_CustomGroup_Not_Found_Should_Throw()
        {
            // Function: 1.8 — Add Group of Tests
            // Arrange - FAILURE test for AddCustomGroupToVisitAsync when custom group not found
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddCustomGroupToVisitAsync(visit.VisitId, 99999);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Custom group not found*");
        }

        [Fact]
        public async Task AddCustomGroupToVisitAsync_CustomGroup_Has_No_Tests_Should_Throw()
        {
            // Function: 1.8 — Add Group of Tests
            // Arrange - FAILURE test for AddCustomGroupToVisitAsync when custom group has no tests
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var group = new CustomGroup { CustomGroupId = 1, Name = "Empty Group" };
            _db.CustomGroups.Add(group);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddCustomGroupToVisitAsync(visit.VisitId, 1);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Custom group has no tests*");
        }

        [Fact]
        public async Task CreateAsync_When_PatientNotFound_Should_Throw_FailureGuard()
        {
            // Arrange
            var visit = new Visit
            {
                PatientId = 99999,
                VisitDate = DateTime.Now,
                AccountType = "Cash"
            };

            // Act
            Func<Task> act = async () => await _service.CreateAsync(visit);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Patient not found*");
        }

        [Fact]
        public async Task CreateAsync_When_VisitDateDefault_Should_Set_Date_And_OpenStatus_EdgeGuard()
        {
            // Arrange
            var patient = new Patient { LabId = "L-E1", FullName = "Edge Patient", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            // Act
            var created = await _service.CreateAsync(new Visit
            {
                PatientId = patient.PatientId,
                VisitDate = default,
                AccountType = ""
            });

            // Assert
            created.VisitDate.Should().NotBe(default);
            created.AccountType.Should().Be("Cash");
            created.Status.Should().Be("Open");
        }

        [Fact]
        public async Task GetByPatientIdAsync_When_MultipleVisits_Should_Return_DescendingByDate_EdgeGuard()
        {
            // Arrange
            var patient = new Patient { LabId = "L-E2", FullName = "Order Patient", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            _db.Visits.AddRange(
                new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(-3) },
                new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(-1) });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetByPatientIdAsync(patient.PatientId);

            // Assert
            rows.Should().HaveCount(2);
            rows[0].VisitDate.Should().BeAfter(rows[1].VisitDate);
        }
    }
}
