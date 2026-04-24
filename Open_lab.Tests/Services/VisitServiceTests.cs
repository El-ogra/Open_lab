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
        public async Task RemoveVisitTestAsync_Verified_Should_Throw()
        {
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
        public async Task ResolveTestPriceAsync_With_Referral_PriceList_Should_Use_Contract_Price()
        {
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
    }
}
