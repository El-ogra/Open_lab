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
    public class ExternalSettlementServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly ExternalSettlementService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public ExternalSettlementServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new ExternalSettlementService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetTotalProfitAsync_Should_Calculate_PatientPrice_Minus_CostPrice_LogicGuard()
        {
            // Arrange - Function 2.13
            var referral = new Referral { Name = "External Lab A" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            var test = new Test { NameReceipt = "CBC", CostPrice = 30m, Price = 100m }; // Note: visit price overrides test price
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();

            var visit = new Visit { VisitDate = DateTime.Today };
            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            var visitTest = new VisitTest { VisitId = visit.VisitId, TestId = test.TestId, Price = 100m }; // Patient pays 100
            _db.VisitTests.Add(visitTest);
            await _db.SaveChangesAsync();

            // Lab queue item
            _db.ExternalLabQueues.Add(new ExternalLabQueue 
            { 
                VisitTestId = visitTest.VisitTestId, 
                ReferralId = referral.ReferralId, 
                Status = "Shipped" 
            });
            await _db.SaveChangesAsync();

            // Act
            var profit = await _service.GetTotalProfitAsync(referral.ReferralId);

            // Assert - Logic Guard: Verify profit = 100 - 30 = 70 (2.13)
            profit.Should().Be(70m);
        }

        [Fact]
        public async Task CreateSettlementAsync_Should_Update_Balance_LogicGuard()
        {
            // Arrange - Function 2.13
            var referral = new Referral { Name = "Lab B" };
            _db.Referrals.Add(referral);
            await _db.SaveChangesAsync();

            // Add a test with cost 50
            var test = new Test { CostPrice = 50m };
            _db.Tests.Add(test);
            await _db.SaveChangesAsync();
            var vt = new VisitTest { VisitId = 1, TestId = test.TestId, Price = 100m };
            _db.VisitTests.Add(vt);
            await _db.SaveChangesAsync();
            _db.ExternalLabQueues.Add(new ExternalLabQueue { VisitTestId = vt.VisitTestId, ReferralId = referral.ReferralId, Status = "Shipped" });
            await _db.SaveChangesAsync();

            // Act - Pay 20
            var settlement = await _service.CreateSettlementAsync(referral.ReferralId, 20m, "Payment 1");

            // Assert - Logic Guard: Initial Cost 50, Paid 20, Balance 30
            settlement.TotalCost.Should().Be(50m);
            settlement.AmountPaid.Should().Be(20m);
            settlement.Balance.Should().Be(30m);
            
            var history = await _service.GetSettlementHistoryAsync(referral.ReferralId);
            history.Should().HaveCount(1);
            history.First().Note.Should().Be("Payment 1");
        }
    }
}
