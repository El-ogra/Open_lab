using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;

namespace Open_lab.Tests.ViewModels
{
    public class PatientBillingViewModelTests : IDisposable
    {
        private readonly Mock<IInvoiceService> _invoiceServiceMock;
        private readonly PatientBillingViewModel _viewModel;

        public PatientBillingViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _invoiceServiceMock = new Mock<IInvoiceService>();
            _viewModel = new PatientBillingViewModel(_invoiceServiceMock.Object);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public void Commands_When_Admin_Should_Be_Enabled()
        {
            _viewModel.LoadVisitCommand.CanExecute(null).Should().BeTrue();
            _viewModel.SaveInvoiceCommand.CanExecute(null).Should().BeTrue();
            _viewModel.AddPaymentCommand.CanExecute(null).Should().BeFalse();
        }

        [Fact]
        public async Task LoadVisitAsync_With_VisitId_Zero_Should_Set_StatusMessage()
        {
            _viewModel.VisitId = 0;
            await _viewModel.InvokePrivateAsync("LoadVisitAsync");
            _viewModel.StatusMessage.Should().Contain("يرجى إدخال رقم الزيارة");
        }

        [Fact]
        public async Task LoadVisitAsync_With_Existing_Invoice_Should_Load_Data()
        {
            _viewModel.VisitId = 10;
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(500);
            var invoice = new Invoice
            {
                InvoiceId = 100, Total = 500, Discount = 50, Paid = 200, NetTotal = 450, Balance = 250
            };
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.GetPaymentsAsync(100)).ReturnsAsync(new List<Payment>());
            _invoiceServiceMock.Setup(x => x.GetAdditionalChargesAsync(100)).ReturnsAsync(new List<AdditionalCharge>());

            await _viewModel.InvokePrivateAsync("LoadVisitAsync");

            _viewModel.Total.Should().Be(500);
            _viewModel.Discount.Should().Be(50);
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
        }

        [Fact]
        public async Task SaveInvoiceAsync_With_VisitId_Zero_Should_Set_StatusMessage()
        {
            _viewModel.VisitId = 0;
            await _viewModel.InvokePrivateAsync("SaveInvoiceAsync");
            _viewModel.StatusMessage.Should().Contain("يرجى إدخال رقم الزيارة");
        }

        [Fact]
        public async Task SaveInvoiceAsync_With_Valid_Visit_Should_Save()
        {
            _viewModel.VisitId = 10;
            _viewModel.Discount = 25;
            var invoice = new Invoice { InvoiceId = 100, Total = 500, NetTotal = 475 };
            _invoiceServiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(10, 25, 0)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.GetPaymentsAsync(100)).ReturnsAsync(new List<Payment>());

            await _viewModel.InvokePrivateAsync("SaveInvoiceAsync");

            _viewModel.StatusMessage.Should().Contain("تم حفظ الفاتورة");
        }

        [Fact]
        public async Task AddPaymentAsync_With_Paid_Zero_Should_Set_StatusMessage()
        {
            _viewModel.VisitId = 10;
            _viewModel.Paid = 0;
            await _viewModel.InvokePrivateAsync("AddPaymentAsync");
            _viewModel.StatusMessage.Should().Contain("أدخل قيمة المدفوع");
        }

        [Fact]
        public async Task DeletePaymentAsync_With_Null_SelectedPayment_Should_Return()
        {
            await _viewModel.InvokePrivateAsync("DeletePaymentAsync");
            _invoiceServiceMock.Verify(x => x.DeletePaymentAsync(It.IsAny<int>(), It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task DeletePaymentAsync_With_Valid_SelectedPayment_Should_Delete()
        {
            _viewModel.SelectedPayment = new InvoicePaymentRow { PaymentId = 3 };
            _viewModel.VisitId = 10;
            var invoice = new Invoice { InvoiceId = 100 };
            _invoiceServiceMock.Setup(x => x.DeletePaymentAsync(3, It.IsAny<string?>())).Returns(Task.CompletedTask);
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(500);
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.GetPaymentsAsync(100)).ReturnsAsync(new List<Payment>());
            _invoiceServiceMock.Setup(x => x.GetAdditionalChargesAsync(100)).ReturnsAsync(new List<AdditionalCharge>());

            await _viewModel.InvokePrivateAsync("DeletePaymentAsync");

            _invoiceServiceMock.Verify(x => x.DeletePaymentAsync(3, It.IsAny<string?>()), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تم حذف الدفعة");
        }

        [Fact]
        public async Task AddChargeAsync_With_VisitId_Zero_Should_Return()
        {
            _viewModel.VisitId = 0;
            await _viewModel.InvokePrivateAsync("AddChargeAsync");
            _invoiceServiceMock.Verify(x => x.AddAdditionalChargeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
        }

        [Fact]
        public async Task AddChargeAsync_With_Valid_Data_Should_Add_Charge()
        {
            _viewModel.VisitId = 10;
            _viewModel.NewChargeAmount = 50;
            _viewModel.NewChargeDescription = "Extra Service";
            var invoice = new Invoice { InvoiceId = 100 };
            _invoiceServiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(10, It.IsAny<decimal>(), 0)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.AddAdditionalChargeAsync(100, "Extra Service", 50)).ReturnsAsync(new AdditionalCharge { AdditionalChargeId = 1 });
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(500);
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.GetPaymentsAsync(100)).ReturnsAsync(new List<Payment>());
            _invoiceServiceMock.Setup(x => x.GetAdditionalChargesAsync(100)).ReturnsAsync(new List<AdditionalCharge>());

            await _viewModel.InvokePrivateAsync("AddChargeAsync");

            _invoiceServiceMock.Verify(x => x.AddAdditionalChargeAsync(100, "Extra Service", 50), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تمت إضافة الرسوم الإضافية");
        }
    }
}
