using System;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class InvoiceServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly InvoiceService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public InvoiceServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new InvoiceService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task CreateOrUpdateInvoiceAsync_Should_Create_Invoice_With_Correct_Totals()
        {
            // Arrange
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 200m });
            await _db.SaveChangesAsync();

            // Act
            var invoice = await _service.CreateOrUpdateInvoiceAsync(visit.VisitId, 50m, 0m);

            // Assert
            invoice.Total.Should().Be(200m);
            invoice.Discount.Should().Be(50m);
            invoice.NetTotal.Should().Be(150m);
        }

        [Fact]
        public async Task AddPaymentAsync_InvalidAmount_Should_Throw()
        {
            // Arrange
            var invoice = new Invoice { VisitId = 1, Total = 100m, Discount = 0m, NetTotal = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddPaymentAsync(invoice.InvoiceId, 0m, 1);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task AddAdditionalChargeAsync_InvalidDescription_Should_Throw()
        {
            // Arrange
            var invoice = new Invoice { VisitId = 1, Total = 100m, Discount = 0m, NetTotal = 100m };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            // Act
            Func<Task> act = async () => await _service.AddAdditionalChargeAsync(invoice.InvoiceId, "   ", 10m);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task GetVisitTotalAsync_WithCharges_Returns_Sum()
        {
            // Arrange
            var patient = new Patient { LabId = "L1", FullName = "P", Gender = "Male" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Now };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId };
            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync();

            _db.VisitTests.Add(new VisitTest { VisitId = visit.VisitId, TestId = 1, Price = 150m });
            await _db.SaveChangesAsync();

            _db.AdditionalCharges.Add(new AdditionalCharge { InvoiceId = invoice.InvoiceId, Description = "Extra", Amount = 25m });
            await _db.SaveChangesAsync();

            // Act
            var total = await _service.GetVisitTotalAsync(visit.VisitId);

            // Assert
            total.Should().Be(175m);
        }
    }
}
