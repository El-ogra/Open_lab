using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class TestClassificationServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly TestClassificationService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public TestClassificationServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new TestClassificationService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetConsumptionReportAsync_Should_Aggregate_Consumption_Per_Reagent()
        {
            // Function: X.X — To Be Determined
            // Arrange
            // Act
            var reagent = new Reagent { Name = "R1", Unit = "ml", CurrentStock = 1000m };
            var test1 = new Test { Code = "T1", NameReport = "CBC", Price = 10m };
            var test2 = new Test { Code = "T2", NameReport = "GLU", Price = 15m };
            _db.Reagents.Add(reagent);
            _db.Tests.AddRange(test1, test2);
            await _db.SaveChangesAsync();

            _db.TestConsumptions.AddRange(
                new TestConsumption { TestId = test1.TestId, ReagentId = reagent.ReagentId, AmountPerTest = 5m },
                new TestConsumption { TestId = test2.TestId, ReagentId = reagent.ReagentId, AmountPerTest = 2m });

            var visit = new Visit { PatientId = 1, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.AddRange(
                new VisitTest { VisitId = visit.VisitId, TestId = test1.TestId, Price = 10m },
                new VisitTest { VisitId = visit.VisitId, TestId = test1.TestId, Price = 10m },
                new VisitTest { VisitId = visit.VisitId, TestId = test2.TestId, Price = 15m });
            await _db.SaveChangesAsync();

            var report = await _service.GetConsumptionReportAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            report.Should().ContainSingle();
            var row = report.Single();
            row.ReagentName.Should().Be("R1");
            row.Unit.Should().Be("ml");
            row.TotalConsumed.Should().Be(12m);
            row.TestCount.Should().Be(3);
        }

        [Fact]
        public async Task GetConsumptionReportAsync_When_NoVisitTestsInRange_Should_Return_Empty_FailureGuard()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var reagent = new Reagent { Name = "R2", Unit = "ml", CurrentStock = 500m };
            var test = new Test { Code = "TX", NameReport = "ALT", Price = 20m };
            _db.Reagents.Add(reagent);
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.TestConsumptions.Add(new TestConsumption { TestId = test.TestId, ReagentId = reagent.ReagentId, AmountPerTest = 1m });
            await _db.SaveChangesAsync();

            // Act
            var report = await _service.GetConsumptionReportAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            report.Should().BeEmpty();
        }

        [Fact]
        public async Task GetConsumptionReportAsync_With_ZeroAmountConsumption_Should_Keep_ReagentRow_WithZeroTotal_EdgeGuard()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var reagent = new Reagent { Name = "R-Zero", Unit = "ml", CurrentStock = 500m };
            var test = new Test { Code = "T-ZERO", NameReport = "AST", Price = 20m };
            _db.Reagents.Add(reagent);
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            _db.TestConsumptions.Add(new TestConsumption { TestId = test.TestId, ReagentId = reagent.ReagentId, AmountPerTest = 0m });
            var patient = new Patient { LabId = "LZ", FullName = "Patient Zero", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 20m });
            await _db.SaveChangesAsync();

            // Act
            var report = await _service.GetConsumptionReportAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            report.Should().ContainSingle();
            report.Single().ReagentName.Should().Be("R-Zero");
            report.Single().TotalConsumed.Should().Be(0m);
            report.Single().TestCount.Should().Be(1);
        }
    }
}
