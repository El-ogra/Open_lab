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

        [Fact]
        public async Task GetUserPerformanceAsync_When_NoVerificationsInRange_Should_Return_Empty_FailureGuard()
        {
            // Arrange
            var user = new User { Username = "tech-empty" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            _db.ResultValues.Add(new ResultValue
            {
                VisitTestId = 1,
                ParameterId = 1,
                VerifiedAt = DateTime.Today.AddDays(-30),
                VerifiedBy = user.UserId
            });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetUserPerformanceAsync(DateTime.Today.AddDays(-2), DateTime.Today.AddDays(-1));

            // Assert
            rows.Should().BeEmpty();
        }

        [Fact]
        public async Task GetUserPerformanceAsync_When_VerifierMissingFromUsers_Should_Label_As_Unknown_EdgeGuard()
        {
            // Arrange
            _db.ResultValues.Add(new ResultValue
            {
                VisitTestId = 7,
                ParameterId = 1,
                VerifiedAt = DateTime.Today,
                VerifiedBy = 9999
            });
            await _db.SaveChangesAsync();

            // Act
            var rows = await _service.GetUserPerformanceAsync(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));

            // Assert
            rows.Should().ContainSingle();
            rows[0].Username.Should().Be("Unknown");
            rows[0].CompletedTestsCount.Should().Be(1);
        }
    }
}
