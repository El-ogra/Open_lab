using System;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class UserProductivityServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly UserProductivityService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public UserProductivityServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new UserProductivityService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task GetUserPerformanceAsync_Should_Count_Distinct_Verified_Tests_Per_User()
        {
            var user1 = new User { Username = "tech1" };
            var user2 = new User { Username = "tech2" };
            _db.Users.AddRange(user1, user2);
            await _db.SaveChangesAsync();

            var from = DateTime.Today.AddDays(-1);
            var to = DateTime.Today.AddDays(1);

            _db.ResultValues.AddRange(
                new ResultValue { VisitTestId = 100, ParameterId = 1, VerifiedAt = DateTime.Today, VerifiedBy = user1.UserId },
                new ResultValue { VisitTestId = 100, ParameterId = 2, VerifiedAt = DateTime.Today, VerifiedBy = user1.UserId },
                new ResultValue { VisitTestId = 101, ParameterId = 1, VerifiedAt = DateTime.Today, VerifiedBy = user1.UserId },
                new ResultValue { VisitTestId = 200, ParameterId = 1, VerifiedAt = DateTime.Today, VerifiedBy = user2.UserId },
                new ResultValue { VisitTestId = 300, ParameterId = 1, VerifiedAt = DateTime.Today.AddDays(-5), VerifiedBy = user2.UserId });
            await _db.SaveChangesAsync();

            var rows = await _service.GetUserPerformanceAsync(from, to);

            rows.Should().HaveCount(2);
            rows[0].Username.Should().Be("tech1");
            rows[0].CompletedTestsCount.Should().Be(2);
            rows[1].Username.Should().Be("tech2");
            rows[1].CompletedTestsCount.Should().Be(1);
        }
    }
}
