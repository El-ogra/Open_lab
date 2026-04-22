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
        public async Task GetReferralsAsync_Should_Return_List()
        {
            _db.Referrals.Add(new Referral { Name = "Ref A" });
            _db.Referrals.Add(new Referral { Name = "Ref B" });
            await _db.SaveChangesAsync();

            var list = await _service.GetReferralsAsync();
            list.Select(r => r.Name).Should().Contain(new[] { "Ref A", "Ref B" });
        }

        [Fact]
        public async Task GetMonthlyAnalysisAsync_Should_Return_12_Months_With_Correct_Counts()
        {
            var patient = new Patient { LabId = "L1", FullName = "P1", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = new DateTime(DateTime.Today.Year, 1, 15) };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 10m });
            await _db.SaveChangesAsync();

            var result = await _service.GetMonthlyAnalysisAsync(DateTime.Today.Year);
            result.Should().HaveCount(12);
            var jan = result.First(r => r.Month == 1);
            jan.VisitCount.Should().Be(1);
            jan.TestCount.Should().Be(1);
        }

        [Fact]
        public async Task GetTop10TestsAsync_Should_Group_By_TestName()
        {
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
            top.First().TestName.Should().Be("CBC");
            top.First().DemandCount.Should().Be(2);
        }

        [Fact]
        public async Task GetSampleCountPerYear_Should_Return_Yearly_Ranges()
        {
            var patient = new Patient { LabId = "L3", FullName = "P3", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 1m });
            await _db.SaveChangesAsync();

            var rows = await _service.GetSampleCountPerYearAsync(1);
            rows.Should().NotBeNull();
            rows.Any(r => r.SamplesCount >= 1).Should().BeTrue();
        }
    }
}
