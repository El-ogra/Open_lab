using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class ReportServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly ReportService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public ReportServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new ReportService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task Function_4_4_Composite_Report_Structure_Validation()
        {
            // Requirement 4.4: Support composite reports with multiple tests
            var visitId = 50;
            var patient = new Patient { PatientId = 5, FullName = "Bob" };
            _db.Patients.Add(patient);
            _db.Visits.Add(new Visit { VisitId = visitId, PatientId = 5, Patient = patient });
            
            _db.Tests.Add(new Test { TestId = 1, NameReport = "Test A" });
            _db.Tests.Add(new Test { TestId = 2, NameReport = "Test B" });
            
            _db.VisitTests.Add(new VisitTest { VisitTestId = 100, VisitId = visitId, TestId = 1 });
            _db.VisitTests.Add(new VisitTest { VisitTestId = 101, VisitId = visitId, TestId = 2 });
            
            await _db.SaveChangesAsync();

            // Act
            var report = await _service.GetVisitReportAsync(visitId);

            // Assert
            report.Should().NotBeNull();
            report!.Tests.Should().HaveCount(2);
            report.Tests.Select(t => t.Test.NameReport).Should().Contain(new[] { "Test A", "Test B" });
        }

        [Fact]
        public async Task Function_4_5_Report_Manual_Ordering_Integrity()
        {
            // Requirement 4.5: Manual/Fixed ordering of tests in report
            var visitId = 60;
            _db.Patients.Add(new Patient { PatientId = 6, FullName = "Charlie" });
            _db.Visits.Add(new Visit { VisitId = visitId, PatientId = 6 });

            _db.Tests.Add(new Test { TestId = 1, Code = "T1", NameReport = "Last", ReportOrder = 50 });
            _db.Tests.Add(new Test { TestId = 2, Code = "T2", NameReport = "First", ReportOrder = 1 });

            _db.VisitTests.Add(new VisitTest { VisitTestId = 200, VisitId = visitId, TestId = 1 });
            _db.VisitTests.Add(new VisitTest { VisitTestId = 201, VisitId = visitId, TestId = 2 });

            await _db.SaveChangesAsync();

            // Act
            var report = await _service.GetVisitReportAsync(visitId);

            // Assert
            report!.Tests[0].Test.ReportOrder.Should().Be(1);
            report!.Tests[1].Test.ReportOrder.Should().Be(50);
        }

        [Fact]
        public async Task Function_4_9_Historical_History_Comparison_Validation()
        {
            // Requirement 4.9: Compare current results with patient history
            var patientId = 100;
            _db.Patients.Add(new Patient { PatientId = patientId, FullName = "History Patient" });

            var pId = 55;
            _db.Tests.Add(new Test { TestId = 10, NameReport = "Glucose" });
            _db.TestParameters.Add(new TestParameter { ParameterId = pId, TestId = 10, Name = "GLU" });

            // Previous Result (1 month ago)
            var prevVisit = new Visit { VisitId = 1, PatientId = patientId, VisitDate = DateTime.Now.AddMonths(-1) };
            var prevVT = new VisitTest { VisitTestId = 1, VisitId = 1, TestId = 10 };
            _db.Visits.Add(prevVisit);
            _db.VisitTests.Add(prevVT);
            _db.ResultValues.Add(new ResultValue { VisitTestId = 1, ParameterId = pId, Value = "95" });

            // Current Result
            var currVisit = new Visit { VisitId = 2, PatientId = patientId, VisitDate = DateTime.Now };
            var currVT = new VisitTest { VisitTestId = 2, VisitId = 2, TestId = 10 };
            _db.Visits.Add(currVisit);
            _db.VisitTests.Add(currVT);
            _db.ResultValues.Add(new ResultValue { VisitTestId = 2, ParameterId = pId, Value = "110" });

            await _db.SaveChangesAsync();

            // Act
            var report = await _service.GetVisitReportAsync(2);

            // Assert - Verify Function 4.9 logic
            var gluResult = report!.Tests[0].Results[0];
            gluResult.Result.Value.Should().Be("110");
            gluResult.PreviousValue.Should().Be("95");
            gluResult.PreviousDate.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPatientHistoryAsync_Patient_Not_Found_Should_Throw()
        {
            // Arrange - FAILURE test for GetPatientHistoryAsync when patient not found
            // Act
            Func<Task> act = async () => await _service.GetPatientHistoryAsync(99999, null, null);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Patient not found*");
        }

        [Fact]
        public async Task GetPatientHistoryAsync_From_Greater_Than_To_Should_Return_Empty()
        {
            // Arrange - FAILURE test for GetPatientHistoryAsync when From > To
            var patient = new Patient { PatientId = 100, FullName = "Test Patient" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = 100, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var from = DateTime.Now.AddDays(10);
            var to = DateTime.Now.AddDays(-10);

            // Act
            var result = await _service.GetPatientHistoryAsync(100, from, to);

            // Assert
            result.Should().NotBeNull();
            result.Patient.FullName.Should().Be("Test Patient");
            result.Visits.Should().BeEmpty();
        }
    }
}
