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
    public class AccountsTreasuryServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly AccountsTreasuryService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public AccountsTreasuryServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new AccountsTreasuryService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetSnapshotAsync_Overall_Should_Aggregate_Invoices_And_Payments_LogicGuard()
        {
            // Arrange - Function 2.10
            var branch = new Branch { Name = "Main" };
            _db.Branches.Add(branch);
            var patient = new Patient { FullName = "P1" };
            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = patient.PatientId, VisitDate = DateTime.Today, BranchId = branch.BranchId };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var invoice = new Invoice { VisitId = visit.VisitId, Total = 500, NetTotal = 450, Discount = 50, Paid = 100, Balance = 350 };
            _db.Invoices.Add(invoice);
            
            var payment = new Payment { InvoiceId = invoice.InvoiceId, Amount = 100, PaymentDate = DateTime.Today };
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // Act
            var snapshot = await _service.GetSnapshotAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert - Logic Guard: Verify exact aggregation (2.10)
            snapshot.TotalInvoiced.Should().Be(450);
            snapshot.TotalPaid.Should().Be(100);
            snapshot.TotalDiscount.Should().Be(50);
            snapshot.TotalBalance.Should().Be(350);
        }

        [Fact]
        public async Task GetSnapshotAsync_With_BranchFilter_Should_Isolate_Branch_Data_LogicGuard()
        {
            // Arrange - Function 2.11
            var b1 = new Branch { Name = "B1" };
            var b2 = new Branch { Name = "B2" };
            _db.Branches.AddRange(b1, b2);
            var p = new Patient { FullName = "P" };
            _db.Patients.Add(p);
            await _db.SaveChangesAsync();

            // Visit for Branch 1
            var v1 = new Visit { PatientId = p.PatientId, VisitDate = DateTime.Today, BranchId = b1.BranchId };
            _db.Visits.Add(v1);
            await _db.SaveChangesAsync();
            _db.Invoices.Add(new Invoice { VisitId = v1.VisitId, NetTotal = 100 });

            // Visit for Branch 2
            var v2 = new Visit { PatientId = p.PatientId, VisitDate = DateTime.Today, BranchId = b2.BranchId };
            _db.Visits.Add(v2);
            await _db.SaveChangesAsync();
            _db.Invoices.Add(new Invoice { VisitId = v2.VisitId, NetTotal = 500, BranchId = b2.BranchId });

            await _db.SaveChangesAsync();

            // Act - Fetch ONLY Branch 1
            var snapshot = await _service.GetSnapshotAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), b1.BranchId);

            // Assert - Logic Guard: Verify isolation (2.11)
            snapshot.TotalInvoiced.Should().Be(100); // Should NOT include 500 from B2
            snapshot.ByBranch.Should().HaveCount(1);
            snapshot.ByBranch.First().BranchName.Should().Be("B1");
        }

        [Fact]
        public async Task GetSnapshotAsync_Should_Calculate_Doctor_Commissions_LogicGuard()
        {
            // Arrange - Function 2.12
            var doctor = new Physician { FullName = "Dr. Ahmed", CommissionPercentage = 10m };
            _db.Physicians.Add(doctor);
            var p = new Patient { FullName = "P" };
            _db.Patients.Add(p);
            await _db.SaveChangesAsync();

            var visit = new Visit { PatientId = p.PatientId, VisitDate = DateTime.Today, PhysicianId = doctor.PhysicianId };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            _db.Invoices.Add(new Invoice { VisitId = visit.VisitId, NetTotal = 1000m });
            await _db.SaveChangesAsync();

            // Act
            var snapshot = await _service.GetSnapshotAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert - Logic Guard: Verify 10% commission (2.12)
            var docRow = snapshot.ByDoctor.FirstOrDefault(d => d.DoctorName == "Dr. Ahmed");
            docRow.Should().NotBeNull();
            docRow!.CommissionAmount.Should().Be(100m); // 10% of 1000
        }

        [Fact]
        public async Task GetSnapshotAsync_WithInvalidDates_ShouldThrowException_FailureGuard()
        {
            // Arrange
            var from = DateTime.Today;
            var to = DateTime.Today.AddDays(-1); // to is before from
            
            // Act
            Func<Task> act = async () => await _service.GetSnapshotAsync(from, to);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*تاريخ البداية*");
        }
    }
}
