using System;
using System.Threading.Tasks;
using FluentAssertions;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Xunit;

namespace Open_lab.Tests.Services
{
    public class SettingsServiceTests : IDisposable
    {
        private readonly Open_lab.Data.OpenLabDbContext _db;
        private readonly SettingsService _service;
        private readonly string _dbName = Guid.NewGuid().ToString();

        public SettingsServiceTests()
        {
            _db = InMemoryDbContextFactory.Create(_dbName);
            _service = new SettingsService(_db);
        }

        public void Dispose()
        {
            _db.Database.EnsureDeleted();
            _db.Dispose();
        }

        [Fact]
        public async Task SetAndGetString_Should_Return_Value()
        {
            await _service.SetSettingAsync("Key1", "Value1", "desc");
            var value = await _service.GetStringAsync("Key1");
            value.Should().Be("Value1");
        }

        [Fact]
        public async Task GetString_Default_When_Not_Exists()
        {
            var value = await _service.GetStringAsync("NoKey", "Def");
            value.Should().Be("Def");
        }

        [Fact]
        public async Task SetAndGetInt_Should_Parse()
        {
            await _service.SetSettingAsync("IntKey", 42);
            var v = await _service.GetIntAsync("IntKey", 0);
            v.Should().Be(42);
        }

        [Fact]
        public async Task GetInt_Default_On_ParseFail()
        {
            await _service.SetSettingAsync("BadInt", "notint");
            var v = await _service.GetIntAsync("BadInt", 7);
            v.Should().Be(7);
        }

        [Fact]
        public async Task SetAndGetDecimal_Should_Parse()
        {
            await _service.SetSettingAsync("DKey", 1.23m);
            var v = await _service.GetDecimalAsync("DKey", 0);
            v.Should().Be(1.23m);
        }

        [Fact]
        public async Task SetAndGetBool_Should_Parse()
        {
            await _service.SetSettingAsync("BKey", true);
            var v = await _service.GetBoolAsync("BKey", false);
            v.Should().BeTrue();
        }

        private class SampleDto { public string? Name { get; set; } public int Age { get; set; } }

        [Fact]
        public async Task SetAndGetJson_Should_Serialize_And_Deserialize()
        {
            var dto = new SampleDto { Name = "X", Age = 5 };
            await _service.SetJsonAsync("JsonKey", dto);
            var res = await _service.GetJsonAsync<SampleDto>("JsonKey");
            res.Should().NotBeNull();
            res!.Name.Should().Be("X");
            res.Age.Should().Be(5);
        }

        [Fact]
        public async Task GetJson_Returns_Null_On_Invalid_Json()
        {
            await _service.SetSettingAsync("JsonKey2", "not-json");
            var res = await _service.GetJsonAsync<SampleDto>("JsonKey2");
            res.Should().BeNull();
        }

        [Fact]
        public async Task DeleteSetting_Should_Remove()
        {
            await _service.SetSettingAsync("KDel", "V");
            var before = await _service.GetStringAsync("KDel");
            before.Should().Be("V");
            await _service.DeleteSettingAsync("KDel");
            var after = await _service.GetStringAsync("KDel", null);
            after.Should().BeNull();
        }

        [Fact]
        public async Task PrinterDefaults_Should_Work()
        {
            var def = await _service.GetDefaultPrinterAsync();
            def.Should().NotBeNull();
            await _service.SetDefaultPrinterAsync("MyPrinter");
            var now = await _service.GetDefaultPrinterAsync();
            now.Should().Be("MyPrinter");

            await _service.SetReceiptPrinterAsync("Receipt1");
            var r = await _service.GetReceiptPrinterAsync();
            r.Should().Be("Receipt1");

            await _service.SetReportPrinterAsync("Report1");
            var rp = await _service.GetReportPrinterAsync();
            rp.Should().Be("Report1");
        }

        [Fact]
        public async Task MarginSettings_Should_Get_And_Set()
        {
            var leftDefault = await _service.GetLeftMarginAsync();
            leftDefault.Should().Be(0);
            await _service.SetLeftMarginAsync(2.5m);
            var left = await _service.GetLeftMarginAsync();
            left.Should().Be(2.5m);

            var rightDefault = await _service.GetRightMarginAsync();
            rightDefault.Should().Be(0);
            await _service.SetRightMarginAsync(1.1m);
            var right = await _service.GetRightMarginAsync();
            right.Should().Be(1.1m);
        }
    }
}
