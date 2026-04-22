using System;
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
    public class UserAdminServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly UserAdminService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public UserAdminServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new UserAdminService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task CreateUserAsync_Should_Create_And_Hash_Password()
        {
            var user = new User { Username = "newuser", FullName = "New" };
            var created = await _service.CreateUserAsync(user, "pass123");
            created.UserId.Should().BeGreaterThan(0);
            created.PasswordHash.Should().NotBeNullOrWhiteSpace();
            created.Salt.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task CreateUserAsync_Duplicate_Should_Throw()
        {
            _db.Users.Add(new User { Username = "exist" });
            await _db.SaveChangesAsync();

            Func<Task> act = async () => await _service.CreateUserAsync(new User { Username = "exist" }, "p");
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task DeleteUserAsync_Admin_Should_Throw()
        {
            _db.Users.Add(new User { Username = "admin" });
            await _db.SaveChangesAsync();

            Func<Task> act = async () => await _service.DeleteUserAsync(_db.Users.First(u => u.Username == "admin").UserId);
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CreateRoleAsync_Duplicate_Should_Throw()
        {
            _db.Roles.Add(new Role { RoleName = "Role1" });
            await _db.SaveChangesAsync();

            Func<Task> act = async () => await _service.CreateRoleAsync("Role1");
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task AssignSingleRoleAsync_Should_Assign()
        {
            var user = new User { Username = "u1" };
            var role = new Role { RoleName = "r1" };
            _db.Users.Add(user);
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();

            await _service.AssignSingleRoleAsync(user.UserId, role.RoleId);
            var links = await _db.UserRoles.Where(ur => ur.UserId == user.UserId).ToListAsync();
            links.Should().ContainSingle().Which.RoleId.Should().Be(role.RoleId);
        }
    }
}
