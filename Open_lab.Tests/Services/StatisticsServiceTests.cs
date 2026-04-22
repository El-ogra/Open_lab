using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class StatisticsServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly StatisticsService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public StatisticsServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new StatisticsService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetReferralsAsync_Should_Return_List_LogicGuard()
        {
            // Refactored to Logic Guard - verifies data integrity
            _db.Referrals.Add(new Referral { Name = "Ref A" });
            _db.Referrals.Add(new Referral { Name = "Ref B" });
            await _db.SaveChangesAsync();

            var list = await _service.GetReferralsAsync();

            // Assert - Logic Guard: Verify exact data returned
            list.Should().HaveCount(2);
            list.Should().Contain(r => r.Name == "Ref A");
            list.Should().Contain(r => r.Name == "Ref B");
        }

        [Fact]
        public async Task GetMonthlyAnalysisAsync_Should_Return_12_Months_With_Correct_Counts_LogicGuard()
        {
            // Refactored to Logic Guard - verifies aggregation accuracy
            var patient = new Patient { LabId = "L1", FullName = "P1", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = new DateTime(DateTime.Today.Year, 1, 15) };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 10m });
            await _db.SaveChangesAsync();

            var result = await _service.GetMonthlyAnalysisAsync(DateTime.Today.Year);

            // Assert - Logic Guard: Verify exact aggregation
            result.Should().HaveCount(12);
            var jan = result.First(r => r.Month == 1);
            jan.VisitCount.Should().Be(1);
            jan.TestCount.Should().Be(1);

            // Verify other months have zero counts
            var feb = result.First(r => r.Month == 2);
            feb.VisitCount.Should().Be(0);
            feb.TestCount.Should().Be(0);
        }

        [Fact]
        public async Task GetTop10TestsAsync_Should_Group_By_TestName_LogicGuard()
        {
            // Refactored to Logic Guard - verifies grouping accuracy
            var patient = new Patient { LabId = "L2", FullName = "P2", Gender = "Female" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var t1 = new Test { NameReport = "CBC", Code = "C" };
            var t2 = new Test { NameReport = "GLU", Code = "G" };
            _db.Tests.AddRange(t1, t2);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = t1.TestId, Price = 10m });
            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = t1.TestId, Price = 10m });
            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = t2.TestId, Price = 5m });
            await _db.SaveChangesAsync();

            var from = DateTime.Today.AddDays(-1);
            var to = DateTime.Today.AddDays(1);
            var top = await _service.GetTop10TestsAsync(from, to);

            // Assert - Logic Guard: Verify exact grouping and ordering
            top.Should().HaveCount(2);
            top.First().TestName.Should().Be("CBC");
            top.First().DemandCount.Should().Be(2);
            top.First().TotalRevenue.Should().Be(20m);

            top[1].TestName.Should().Be("GLU");
            top[1].DemandCount.Should().Be(1);
            top[1].TotalRevenue.Should().Be(5m);
        }

        [Fact]
        public async Task GetSampleCountPerYear_Should_Return_Yearly_Ranges_LogicGuard()
        {
            // Refactored to Logic Guard - verifies aggregation accuracy
            var patient = new Patient { LabId = "L3", FullName = "P3", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 1m });
            await _db.SaveChangesAsync();

            var rows = await _service.GetSampleCountPerYearAsync(1);

            // Assert - Logic Guard: Verify exact counts
            rows.Should().NotBeNull();
            rows.Should().HaveCountGreaterOrEqualTo(1);
            var currentYear = rows.FirstOrDefault(r => r.Year == DateTime.Today.Year);
            currentYear.Should().NotBeNull();
            currentYear!.SamplesCount.Should().Be(1);
        }

        [Fact]
        public async Task GetSnapshotAsync_Should_Calculate_Gender_And_Referral_Stats()
        {
            // Arrange
            var referral = new Referral { Name = "Ref1" };
            _db.Referrals.Add(referral);
            var patient = new Patient { FullName = "P1", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, ReferralId = referral.ReferralId };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, NetTotal = 100, Paid = 40 };
            _db.Invoices.Add(invoice);
            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 100 });
            await _db.SaveChangesAsync();

            // Act
            var snapshot = await _service.GetSnapshotAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), "الكل", null);

            // Assert
            snapshot.Summary.VisitCount.Should().Be(1);
            snapshot.Summary.TotalRevenue.Should().Be(100);
            snapshot.ByGender.Should().Contain(g => g.Gender == "Male" && g.VisitsCount == 1);
            snapshot.ByReferral.Should().Contain(r => r.ReferralName == "Ref1" && r.Revenue == 100);
        }

        // 2.10 - Financial Inventory Report - Aggregation Accuracy Tests
        [Fact]
        public async Task GetSnapshotAsync_Should_Calculate_Correct_Aggregates()
        {
            // Arrange
            var patient1 = new Patient { FullName = "P1", Gender = "Male" };
            var patient2 = new Patient { FullName = "P2", Gender = "Female" };
            _db.Patients.AddRange(patient1, patient2);
            await _db.SaveChangesAsync();

            var visit1 = new Visit { PatientId = patient1.PatientId, VisitDate = DateTime.Today };
            var visit2 = new Visit { PatientId = patient2.PatientId, VisitDate = DateTime.Today };
            _db.Visits.AddRange(visit1, visit2);
            await _db.SaveChangesAsync();

            var invoice1 = new Invoice { VisitId = visit1.VisitId, NetTotal = 150m, Paid = 100m };
            var invoice2 = new Invoice { VisitId = visit2.VisitId, NetTotal = 200m, Paid = 50m };
            _db.Invoices.AddRange(invoice1, invoice2);
            await _db.SaveChangesAsync();

            // Act
            var snapshot = await _service.GetSnapshotAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), "الكل", null);

            // Assert - Logic Guard: Verify exact aggregation
            snapshot.Summary.VisitCount.Should().Be(2);
            snapshot.Summary.TotalRevenue.Should().Be(350m); // 150 + 200
            snapshot.Summary.TotalPaid.Should().Be(150m); // 100 + 50
        }

        [Fact]
        public async Task GetSnapshotAsync_EmptyPeriod_Should_Return_Zeros()
        {
            // Arrange - No data in the specified period
            var patient = new Patient { FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today.AddDays(-10) };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            // Act - Query for future period
            var snapshot = await _service.GetSnapshotAsync(DateTime.Today.AddDays(5), DateTime.Today.AddDays(10), "الكل", null);

            // Assert - Logic Guard: Verify zero aggregation for empty period
            snapshot.Summary.VisitCount.Should().Be(0);
            snapshot.Summary.TotalRevenue.Should().Be(0m);
            snapshot.Summary.TotalPaid.Should().Be(0m);
        }

        // 2.11 - Branch Inventory - Aggregation Accuracy Tests
        // Note: GetSnapshotAsync_Should_Calculate_Gender_And_Referral_Stats already covers this
    }
}
