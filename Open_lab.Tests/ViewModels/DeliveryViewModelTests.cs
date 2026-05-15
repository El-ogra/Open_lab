using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;

namespace Open_lab.Tests.ViewModels
{
    public class DeliveryViewModelTests : IDisposable
    {
        private readonly Mock<IDeliveryService> _deliveryServiceMock = new();
        private readonly Mock<IInvoiceService> _invoiceServiceMock = new();

        public DeliveryViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        [Fact]
        public async Task PayCommand_With_SelectedVisit_Should_AddPayment_And_Reload()
        {
            // Function: 4 — Delivery direct payment
            // Arrange
            var vm = new DeliveryViewModel(_deliveryServiceMock.Object, _invoiceServiceMock.Object)
            {
                PaymentAmount = 125m
            };
            vm.SelectedVisit = new DeliveryVisitRow { VisitId = 20, PatientName = "Pay Patient", Balance = 125m };

            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(20))
                .ReturnsAsync(new Invoice { InvoiceId = 30, VisitId = 20, Balance = 125m });
            _invoiceServiceMock.Setup(x => x.AddPaymentAsync(30, 125m, "Cash", 1))
                .ReturnsAsync(new Payment { PaymentId = 40, InvoiceId = 30, Amount = 125m, PaymentMethod = "Cash", UserId = 1 });
            _deliveryServiceMock.Setup(x => x.SearchAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<string?>()))
                .ReturnsAsync(new List<DeliveryVisitRow>());

            // Act
            vm.PayCommand.Execute(null);
            await Task.Delay(150);

            // Assert
            _invoiceServiceMock.Verify(x => x.AddPaymentAsync(30, 125m, "Cash", 1), Times.Once);
            vm.PaymentAmount.Should().Be(0);
            vm.StatusMessage.Should().Contain("تم تسجيل الدفع");
        }

        [Fact]
        public async Task PayCommand_WhenInvoiceMissing_Should_Show_StatusMessage()
        {
            // Function: 4 — Delivery direct payment edge guard
            // Arrange
            var vm = new DeliveryViewModel(_deliveryServiceMock.Object, _invoiceServiceMock.Object)
            {
                PaymentAmount = 50m,
                SelectedVisit = new DeliveryVisitRow { VisitId = 99, PatientName = "No Invoice" }
            };
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(99)).ReturnsAsync((Invoice?)null);

            // Act
            vm.PayCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            vm.StatusMessage.Should().Contain("لا توجد فاتورة");
            _invoiceServiceMock.Verify(x => x.AddPaymentAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }
    }
}
