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

            var row = await _db.SystemSettings.SingleAsync(s => s.SettingKey == "Key1");
            row.SettingType.Should().Be("String");
            row.Description.Should().Be("desc");
            row.SettingValue.Should().Be("Value1");
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

            var raw = await _service.GetStringAsync("BadInt");
            raw.Should().Be("notint");
        }

        [Fact]
        public async Task SetAndGetDecimal_Should_Parse()
        {
            await _service.SetSettingAsync("DKey", 1.23m);
            var v = await _service.GetDecimalAsync("DKey", 0);
            v.Should().Be(1.23m);

            var row = await _db.SystemSettings.SingleAsync(s => s.SettingKey == "DKey");
            row.SettingType.Should().Be("Decimal");
        }

        [Fact]
        public async Task SetAndGetBool_Should_Parse()
        {
            await _service.SetSettingAsync("BKey", true);
            var v = await _service.GetBoolAsync("BKey", false);
            v.Should().BeTrue();

            var row = await _db.SystemSettings.SingleAsync(s => s.SettingKey == "BKey");
            row.SettingType.Should().Be("Bool");
            row.SettingValue.Should().Be("True");
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

            var exists = await _db.SystemSettings.AnyAsync(s => s.SettingKey == "KDel");
            exists.Should().BeFalse();
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

            var rows = await _db.SystemSettings
                .Where(s => s.SettingKey == "Printer.Default" || s.SettingKey == "Printer.Receipt" || s.SettingKey == "Printer.Report")
                .ToListAsync();
            rows.Should().HaveCount(3);
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

            var leftRaw = await _service.GetStringAsync("Margin.Left");
            var rightRaw = await _service.GetStringAsync("Margin.Right");
            leftRaw.Should().Be("2.5");
            rightRaw.Should().Be("1.1");
        }

        [Fact]
        public async Task HeaderFooter_Settings_Should_Work()
        {
            // 13.3 Configure Header/Footer
            await _service.SetSettingAsync("Print.Header", "Lab Name");
            await _service.SetSettingAsync("Print.Footer", "Page {0}");
            
            var header = await _service.GetStringAsync("Print.Header");
            var footer = await _service.GetStringAsync("Print.Footer");
            
            header.Should().Be("Lab Name");
            footer.Should().Be("Page {0}");

            var headerRow = await _db.SystemSettings.SingleAsync(s => s.SettingKey == "Print.Header");
            var footerRow = await _db.SystemSettings.SingleAsync(s => s.SettingKey == "Print.Footer");
            headerRow.SettingType.Should().Be("String");
            footerRow.SettingType.Should().Be("String");
        }

        [Fact]
        public async Task DefaultAccountType_Setting_Should_Work()
        {
            // 13.4 Set Default Account Type
            await _service.SetSettingAsync("Visit.DefaultAccountType", "Referral");
            var type = await _service.GetStringAsync("Visit.DefaultAccountType");
            type.Should().Be("Referral");

            var row = await _db.SystemSettings.SingleAsync(s => s.SettingKey == "Visit.DefaultAccountType");
            row.SettingValue.Should().Be("Referral");
        }

        [Fact]
        public async Task InvoiceSettings_Should_Work()
        {
            // 13.6 Set Invoice Settings
            await _service.SetSettingAsync("Invoice.ShowLogo", true);
            await _service.SetSettingAsync("Invoice.Currency", "EGP");
            
            var showLogo = await _service.GetBoolAsync("Invoice.ShowLogo");
            var currency = await _service.GetStringAsync("Invoice.Currency");
            
            showLogo.Should().BeTrue();
            currency.Should().Be("EGP");

            var logoType = await _db.SystemSettings
                .Where(s => s.SettingKey == "Invoice.ShowLogo")
                .Select(s => s.SettingType)
                .SingleAsync();
            logoType.Should().Be("Bool");
        }
    }
}
