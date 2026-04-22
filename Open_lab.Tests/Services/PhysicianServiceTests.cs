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
    public class PhysicianServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly PhysicianService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public PhysicianServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new PhysicianService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        // 12.6 - Referring Doctor Creation - CRUD Logic Validation Tests
        [Fact]
        public async Task CreateAsync_Should_Create_Physician_With_All_Fields()
        {
            var physician = new Physician
            {
                FullName = "Dr. John Smith",
                Phone = "1234567890",
                Specialty = "Cardiology",
                Address = "123 Medical St",
                IsActive = true,
                CommissionPercentage = 10m
            };

            var created = await _service.CreateAsync(physician);

            created.PhysicianId.Should().BeGreaterThan(0);
            var saved = await _db.Physicians.FindAsync(created.PhysicianId);
            saved.Should().NotBeNull();
            saved!.FullName.Should().Be("Dr. John Smith");
            saved.Phone.Should().Be("1234567890");
            saved.Specialty.Should().Be("Cardiology");
            saved.Address.Should().Be("123 Medical St");
            saved.IsActive.Should().BeTrue();
            saved.CommissionPercentage.Should().Be(10m);
        }

        [Fact]
        public async Task CreateAsync_Should_Create_Physician()
        {
            var physician = new Physician { FullName = "Dr. Smith", CommissionPercentage = 10m };
            var created = await _service.CreateAsync(physician);
            created.PhysicianId.Should().BeGreaterThan(0);
            var saved = await _db.Physicians.FindAsync(created.PhysicianId);
            saved.Should().NotBeNull();
            saved!.FullName.Should().Be("Dr. Smith");
        }

        [Fact]
        public async Task CreateAsync_WithPriceList_Should_Link_PriceList()
        {
            var priceList = new PriceList { Name = "Doctor PL" };
            _db.PriceLists.Add(priceList);
            await _db.SaveChangesAsync();

            var physician = new Physician
            {
                FullName = "Dr. Jane Doe",
                PriceListId = priceList.PriceListId,
                CommissionPercentage = 15m
            };

            var created = await _service.CreateAsync(physician);

            var saved = await _db.Physicians.Include(p => p.PriceList).FirstOrDefaultAsync(p => p.PhysicianId == created.PhysicianId);
            saved.Should().NotBeNull();
            saved!.PriceListId.Should().Be(priceList.PriceListId);
            saved.PriceList.Should().NotBeNull();
            saved.PriceList!.Name.Should().Be("Doctor PL");
        }

        // 12.7 - Doctor Pricing Assignment - Pricing Assignment Logic Tests
        [Fact]
        public async Task UpdateAsync_Should_Update_All_Fields()
        {
            var physician = new Physician
            {
                FullName = "Dr. Old Name",
                Phone = "1111111111",
                Specialty = "Old Specialty",
                IsActive = false,
                CommissionPercentage = 5m
            };
            _db.Physicians.Add(physician);
            await _db.SaveChangesAsync();

            // Update the same tracked entity instead of creating a new one
            physician.FullName = "Dr. New Name";
            physician.Phone = "2222222222";
            physician.Specialty = "New Specialty";
            physician.Address = "New Address";
            physician.IsActive = true;
            physician.CommissionPercentage = 20m;

            await _service.UpdateAsync(physician);

            var saved = await _db.Physicians.FindAsync(physician.PhysicianId);
            saved.Should().NotBeNull();
            saved!.FullName.Should().Be("Dr. New Name");
            saved.Phone.Should().Be("2222222222");
            saved.Specialty.Should().Be("New Specialty");
            saved.Address.Should().Be("New Address");
            saved.IsActive.Should().BeTrue();
            saved.CommissionPercentage.Should().Be(20m);
        }

        [Fact]
        public async Task UpdateAsync_Should_Assign_PriceList()
        {
            var priceList = new PriceList { Name = "PL" };
            _db.PriceLists.Add(priceList);
            await _db.SaveChangesAsync();

            var physician = new Physician { FullName = "Dr. Jones" };
            _db.Physicians.Add(physician);
            await _db.SaveChangesAsync();

            physician.PriceListId = priceList.PriceListId;
            await _service.UpdateAsync(physician);

            var saved = await _db.Physicians.FindAsync(physician.PhysicianId);
            saved!.PriceListId.Should().Be(priceList.PriceListId);
        }

        [Fact]
        public async Task UpdateAsync_Should_Remove_PriceList_When_Set_To_Null()
        {
            var priceList = new PriceList { Name = "Old PL" };
            _db.PriceLists.Add(priceList);
            await _db.SaveChangesAsync();

            var physician = new Physician
            {
                FullName = "Dr. Johnson",
                PriceListId = priceList.PriceListId,
                CommissionPercentage = 12m
            };
            _db.Physicians.Add(physician);
            await _db.SaveChangesAsync();

            physician.PriceListId = null;
            await _service.UpdateAsync(physician);

            var saved = await _db.Physicians.FindAsync(physician.PhysicianId);
            saved.Should().NotBeNull();
            saved!.PriceListId.Should().BeNull();
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_CommissionPercentage()
        {
            var physician = new Physician
            {
                FullName = "Dr. Williams",
                CommissionPercentage = 10m
            };
            _db.Physicians.Add(physician);
            await _db.SaveChangesAsync();

            physician.CommissionPercentage = 25m;
            await _service.UpdateAsync(physician);

            var saved = await _db.Physicians.FindAsync(physician.PhysicianId);
            saved.Should().NotBeNull();
            saved!.CommissionPercentage.Should().Be(25m);
        }
    }
}
