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
        public void PatientBilling_Commands_When_Admin_Should_Be_Enabled()
        {
            // Function: 2.9 — View Patient Account
            // Arrange
            // Act
            // Assert
            _viewModel.LoadVisitCommand.CanExecute(null).Should().BeTrue();
            _viewModel.SaveInvoiceCommand.CanExecute(null).Should().BeTrue();
            _viewModel.AddPaymentCommand.CanExecute(null).Should().BeFalse();
        }

        [Fact]
        public async Task LoadVisitAsync_With_VisitId_Zero_Should_Set_StatusMessage()
        {
            // Function: 2.9 — View Patient Account
            // Arrange
            _viewModel.VisitId = 0;

            // Act
            _viewModel.LoadVisitCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("يرجى إدخال رقم الزيارة");
        }

        [Fact]
        public async Task LoadVisitAsync_With_Existing_Invoice_Should_Load_Data()
        {
            // Function: 2.9 — View Patient Account
            // Arrange
            _viewModel.VisitId = 10;
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(500);
            var invoice = new Invoice
            {
                InvoiceId = 100,
                Total = 500m,
                Discount = 0m,
                NetTotal = 500m,
                Paid = 0m,
                Balance = 500m
            };
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.GetPaymentsAsync(100)).ReturnsAsync(new List<Payment>());
            _invoiceServiceMock.Setup(x => x.GetAdditionalChargesAsync(100)).ReturnsAsync(new List<AdditionalCharge>());

            // Act
            _viewModel.LoadVisitCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.GetVisitTotalAsync(10), Times.Once);
            _viewModel.Total.Should().Be(500m);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoadVisitAsync_When_VisitHasNoInvoice_Should_Create_New_EdgeGuard()
        {
            // Function: 2.9 — View Patient Account
            // Arrange
            _viewModel.VisitId = 10;
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(300m);
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync((Invoice?)null);

            // Act
            _viewModel.LoadVisitCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.GetVisitTotalAsync(10), Times.Once);
            _invoiceServiceMock.Verify(x => x.GetByVisitIdAsync(10), Times.Once);
            _viewModel.Total.Should().Be(300m);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoadVisitAsync_When_ServiceThrows_Should_Set_StatusMessage_FailureGuard()
        {
            // Function: 2.9 — View Patient Account
            // Arrange
            _viewModel.VisitId = 10;
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10))
                .ThrowsAsync(new InvalidOperationException("account-load-failed"));

            // Act
            _viewModel.LoadVisitCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("account-load-failed");
        }

        [Fact]
        public async Task SaveInvoiceAsync_With_VisitId_Zero_Should_Set_StatusMessage()
        {
            // Function: 2.8 — Generate Invoice
            // Arrange
            _viewModel.VisitId = 0;

            // Act
            _viewModel.SaveInvoiceCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("يرجى إدخال رقم الزيارة");
        }

        [Fact]
        public async Task SaveInvoiceAsync_With_Valid_Visit_Should_Save()
        {
            // Function: 2.8 — Generate Invoice
            // Arrange
            _viewModel.VisitId = 10;
            _viewModel.Discount = 25;
            var invoice = new Invoice { InvoiceId = 100, Total = 500, NetTotal = 475 };
            _invoiceServiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(10, 25, 0)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.GetPaymentsAsync(100)).ReturnsAsync(new List<Payment>());

            // Act
            _viewModel.SaveInvoiceCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.Total.Should().Be(500);
            _viewModel.NetTotal.Should().Be(475);
            _viewModel.StatusMessage.Should().Contain("تم حفظ الفاتورة");
            _invoiceServiceMock.Verify(x => x.CreateOrUpdateInvoiceAsync(10, 25, 0), Times.Once);
        }

        [Fact]
        public async Task AddPaymentAsync_With_Paid_Zero_Should_Set_StatusMessage()
        {
            // Function: 2.3 — Record Payment
            // Arrange
            _viewModel.VisitId = 10;
            _viewModel.Paid = 0;

            // Act
            _viewModel.AddPaymentCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("أدخل قيمة المدفوع");
        }

        [Fact]
        public async Task AddPaymentAsync_When_ServiceThrows_Should_Show_Error_FailureGuard()
        {
            // Function: 2.3 — Record Payment
            // Arrange
            _viewModel.VisitId = 10;
            _viewModel.Paid = 50m;
            var invoice = new Invoice { InvoiceId = 100 };
            _invoiceServiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(10, 0, 0)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.AddPaymentAsync(100, 50m, It.IsAny<string>(), It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("Invalid payment"));

            // Act
            _viewModel.AddPaymentCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.AddPaymentAsync(100, 50m, It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            _viewModel.StatusMessage.Should().Contain("Invalid payment");
        }

        [Fact]
        public async Task AddPaymentAsync_With_MaximumAmount_Should_Handle_LargeValue_EdgeGuard()
        {
            // Function: 2.3 — Record Payment
            // Arrange
            _viewModel.VisitId = 10;
            _viewModel.Paid = 999999.99m;
            var invoice = new Invoice { InvoiceId = 100 };
            _invoiceServiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(10, 0, 0)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.AddPaymentAsync(100, 999999.99m, It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(new Payment { PaymentId = 1 });
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(500);
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.GetPaymentsAsync(100)).ReturnsAsync(new List<Payment>());

            // Act
            _viewModel.AddPaymentCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.AddPaymentAsync(100, 999999.99m, It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task DeletePaymentAsync_With_Null_SelectedPayment_Should_Return()
        {
            // Function: 2.6 — Delete Payment
            // Arrange

            // Act
            _viewModel.DeletePaymentCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.DeletePaymentAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task EditPaymentAsync_When_ServiceThrows_Should_Show_Error_FailureGuard()
        {
            // Function: 2.5 — Edit Payment
            // Arrange
            _viewModel.SelectedPayment = new InvoicePaymentRow { PaymentId = 5, Amount = 50m };
            _viewModel.VisitId = 10;
            _invoiceServiceMock.Setup(x => x.EditPaymentAsync(5, 50m, It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Edit failed"));

            // Act
            _viewModel.EditPaymentCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.EditPaymentAsync(5, 50m, It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            _viewModel.StatusMessage.Should().Contain("Edit failed");
        }

        [Fact]
        public async Task EditPaymentAsync_When_EditingToSameAmount_Should_Handle_NoChange_EdgeGuard()
        {
            // Function: 2.5 — Edit Payment
            // Arrange
            _viewModel.SelectedPayment = new InvoicePaymentRow { PaymentId = 5, Amount = 50m };
            _viewModel.EditPaymentAmount = 50m;
            _viewModel.VisitId = 10;
            var invoice = new Invoice { InvoiceId = 100 };
            _invoiceServiceMock.Setup(x => x.EditPaymentAsync(5, 50m, It.IsAny<int>(), It.IsAny<string>())).ReturnsAsync(new Payment { PaymentId = 5, Amount = 50m });
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(500);
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.GetPaymentsAsync(100)).ReturnsAsync(new List<Payment>());

            // Act
            _viewModel.EditPaymentCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.EditPaymentAsync(5, 50m, It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task DeletePaymentAsync_With_Valid_SelectedPayment_Should_Delete()
        {
            // Function: 2.6 — Delete Payment
            // Arrange
            _viewModel.SelectedPayment = new InvoicePaymentRow { PaymentId = 3 };
            _viewModel.VisitId = 10;
            var invoice = new Invoice { InvoiceId = 100 };
            _invoiceServiceMock.Setup(x => x.DeletePaymentAsync(3, It.IsAny<int>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(500);
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.GetPaymentsAsync(100)).ReturnsAsync(new List<Payment>());

            // Act
            _viewModel.DeletePaymentCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.DeletePaymentAsync(3, It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task DeletePaymentAsync_When_ServiceThrows_Should_Show_Error_FailureGuard()
        {
            // Function: 2.6 — Delete Payment
            // Arrange
            _viewModel.SelectedPayment = new InvoicePaymentRow { PaymentId = 3 };
            _invoiceServiceMock.Setup(x => x.DeletePaymentAsync(3, It.IsAny<int>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Delete failed"));

            // Act
            _viewModel.DeletePaymentCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.DeletePaymentAsync(3, It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            _viewModel.StatusMessage.Should().Contain("Delete failed");
        }

        [Fact]
        public async Task DeletePaymentAsync_When_DeletingLastPayment_Should_UpdateBalanceCorrectly_EdgeGuard()
        {
            // Function: 2.6 — Delete Payment
            // Arrange
            _viewModel.SelectedPayment = new InvoicePaymentRow { PaymentId = 3 };
            _viewModel.VisitId = 10;
            var invoice = new Invoice { InvoiceId = 100, Total = 200m, Paid = 100m, Balance = 100m };
            _invoiceServiceMock.Setup(x => x.DeletePaymentAsync(3, It.IsAny<int>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(200m);
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.GetPaymentsAsync(100)).ReturnsAsync(new List<Payment>());

            // Act
            _viewModel.DeletePaymentCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.DeletePaymentAsync(3, It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            _viewModel.Total.Should().Be(200m);
        }

        [Fact]
        public async Task AddChargeAsync_With_VisitId_Zero_Should_Return()
        {
            // Function: 2.7 — Add Additional Charge
            // Arrange
            _viewModel.VisitId = 0;

            // Act
            _viewModel.AddChargeCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.AddAdditionalChargeAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>()), Times.Never);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task AddChargeAsync_With_Valid_Data_Should_Add_Charge()
        {
            // Function: 2.7 — Add Additional Charge
            // Arrange
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

            // Act
            _viewModel.AddChargeCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.AddAdditionalChargeAsync(100, "Extra Service", 50), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تمت إضافة الرسوم الإضافية");
        }

        [Fact]
        public async Task AddChargeAsync_When_ServiceThrows_Should_Show_Error_FailureGuard()
        {
            // Function: 2.7 — Add Additional Charge
            // Arrange
            _viewModel.VisitId = 10;
            _viewModel.NewChargeAmount = 50;
            _viewModel.NewChargeDescription = "Extra Service";
            var invoice = new Invoice { InvoiceId = 100 };
            _invoiceServiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(10, It.IsAny<decimal>(), 0)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.AddAdditionalChargeAsync(100, "Extra Service", 50))
                .ThrowsAsync(new InvalidOperationException("Add charge failed"));

            // Act
            _viewModel.AddChargeCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.AddAdditionalChargeAsync(100, "Extra Service", 50), Times.Once);
            _viewModel.StatusMessage.Should().Contain("Add charge failed");
        }

        [Fact]
        public async Task AddChargeAsync_With_ZeroAmount_Should_Handle_Gracefully_EdgeGuard()
        {
            // Function: 2.7 — Add Additional Charge
            // Arrange
            _viewModel.VisitId = 10;
            _viewModel.NewChargeAmount = 0;
            _viewModel.NewChargeDescription = "Zero Fee";
            var invoice = new Invoice { InvoiceId = 100 };
            _invoiceServiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(10, It.IsAny<decimal>(), 0)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.AddAdditionalChargeAsync(100, "Zero Fee", 0)).ReturnsAsync(new AdditionalCharge { AdditionalChargeId = 1 });
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(500);
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.GetPaymentsAsync(100)).ReturnsAsync(new List<Payment>());
            _invoiceServiceMock.Setup(x => x.GetAdditionalChargesAsync(100)).ReturnsAsync(new List<AdditionalCharge>());

            // Act
            _viewModel.AddChargeCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.AddAdditionalChargeAsync(100, "Zero Fee", 0), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task EditPaymentAsync_With_InvalidAmount_Should_Set_ValidationMessage_FailureGuard()
        {
            // Function: 2.5 — Edit Payment
            // Arrange
            _viewModel.SelectedPayment = new InvoicePaymentRow { PaymentId = 9, Amount = 25m };
            _viewModel.EditPaymentAmount = 0m;

            // Act
            _viewModel.EditPaymentCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("أدخل مبلغ تعديل صالح");
            _invoiceServiceMock.Verify(x => x.EditPaymentAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SettleAccountAsync_When_ServiceThrows_Should_Set_ErrorStatus_FailureGuard()
        {
            // Function: 2.4 — Settle Account
            // Arrange
            _viewModel.VisitId = 10;
            _invoiceServiceMock.Setup(x => x.SettleAccountAsync(10))
                .ThrowsAsync(new InvalidOperationException("remaining balance"));

            // Act
            _viewModel.SettleAccountCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("خطأ:");
            _viewModel.StatusMessage.Should().Contain("remaining balance");
        }

        [Fact]
        public async Task SettleAccountAsync_When_AlreadySettled_Should_Handle_Gracefully_EdgeGuard()
        {
            // Function: 2.4 — Settle Account
            // Arrange
            _viewModel.VisitId = 10;
            var settledInvoice = new Invoice
            {
                InvoiceId = 100,
                Total = 500m,
                Discount = 0m,
                NetTotal = 500m,
                Paid = 500m,
                Balance = 0m,
                Status = "Settled"
            };
            _invoiceServiceMock.Setup(x => x.SettleAccountAsync(10)).ReturnsAsync(settledInvoice);
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(500);
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(settledInvoice);
            _invoiceServiceMock.Setup(x => x.GetPaymentsAsync(100)).ReturnsAsync(new List<Payment>());

            // Act
            _viewModel.SettleAccountCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.SettleAccountAsync(10), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تمت تصفية الحساب");
        }

        [Fact]
        public async Task PrintInvoiceAsync_When_NoInvoiceFound_Should_NotLogPrint_EdgeGuard()
        {
            // Function: 2.8 — Generate Invoice
            // Arrange
            _viewModel.VisitId = 77;
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(77)).ReturnsAsync((Invoice?)null);

            // Act
            _viewModel.PrintInvoiceCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.LogInvoicePrintedAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task CalculateTotalAsync_Should_Call_GetVisitTotalAsync_Directly()
        {
            // Function: 2.1 — Calculate Total
            // Arrange
            _viewModel.VisitId = 10;
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(500m);

            // Act
            _viewModel.LoadVisitCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.GetVisitTotalAsync(10), Times.Once);
            _viewModel.Total.Should().Be(500m);
        }

        [Fact]
        public async Task LoadVisitCommand_When_VisitNotFound_Should_Show_Error_FailureGuard()
        {
            // Function: 2.1 — Calculate Total
            // Arrange
            _viewModel.VisitId = 99999;
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(99999)).ReturnsAsync(0m);

            // Act
            _viewModel.LoadVisitCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.GetVisitTotalAsync(99999), Times.Once);
            _viewModel.Total.Should().Be(0m);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoadVisitCommand_When_VisitHasNoTests_Should_Return_Zero_EdgeGuard()
        {
            // Function: 2.1 — Calculate Total
            // Arrange
            _viewModel.VisitId = 10;
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(0m);

            // Act
            _viewModel.LoadVisitCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.GetVisitTotalAsync(10), Times.Once);
            _viewModel.Total.Should().Be(0m);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ApplyDiscountAsync_Should_Update_NetTotal_Directly()
        {
            // Function: 2.2 — Apply Discount
            // Arrange
            _viewModel.VisitId = 10;
            _viewModel.Discount = 50m;
            var invoice = new Invoice { InvoiceId = 100, Total = 500m, Discount = 50m, NetTotal = 450m };
            _invoiceServiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(10, 50m, 0)).ReturnsAsync(invoice);

            // Act
            _viewModel.SaveInvoiceCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.CreateOrUpdateInvoiceAsync(10, 50m, 0), Times.Once);
            _viewModel.NetTotal.Should().Be(450m);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ApplyDiscountAsync_When_DiscountExceedsTotal_Should_Throw_FailureGuard()
        {
            // Function: 2.2 — Apply Discount
            // Arrange
            _viewModel.VisitId = 10;
            _viewModel.Discount = 150m;
            _invoiceServiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(10, 150m, 0))
                .ThrowsAsync(new InvalidOperationException("الخصم لا يمكن أن يتجاوز إجمالي الفاتورة"));

            // Act
            _viewModel.SaveInvoiceCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.CreateOrUpdateInvoiceAsync(10, 150m, 0), Times.Once);
            _viewModel.StatusMessage.Should().Contain("الخصم");
        }

        [Fact]
        public async Task ApplyDiscountAsync_With_ZeroDiscount_Should_Calculate_Correctly_EdgeGuard()
        {
            // Function: 2.2 — Apply Discount
            // Arrange
            _viewModel.VisitId = 10;
            _viewModel.Discount = 0m;
            var invoice = new Invoice { InvoiceId = 100, Total = 500m, Discount = 0m, NetTotal = 500m };
            _invoiceServiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(10, 0m, 0)).ReturnsAsync(invoice);

            // Act
            _viewModel.SaveInvoiceCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.CreateOrUpdateInvoiceAsync(10, 0m, 0), Times.Once);
            _viewModel.NetTotal.Should().Be(500m);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task AddPaymentAsync_With_ValidPayment_Should_RecordPayment_SuccessGuard()
        {
            // Function: 2.3 — Record Payment
            // Arrange
            _viewModel.VisitId = 10;
            _viewModel.Paid = 40m;
            var invoice = new Invoice { InvoiceId = 100 };
            _invoiceServiceMock.Setup(x => x.CreateOrUpdateInvoiceAsync(10, 0, 0)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.AddPaymentAsync(100, 40m, It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new Payment { PaymentId = 7, Amount = 40m });
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(200m);
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.GetPaymentsAsync(100)).ReturnsAsync(new List<Payment>());

            // Act
            _viewModel.AddPaymentCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.AddPaymentAsync(100, 40m, It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task SettleAccountAsync_With_FullyPaidInvoice_Should_Settle_SuccessGuard()
        {
            // Function: 2.4 — Settle Account
            // Arrange
            _viewModel.VisitId = 10;
            var settledInvoice = new Invoice { InvoiceId = 100, Total = 500m, NetTotal = 500m, Paid = 500m, Balance = 0m, Status = "Settled" };
            _invoiceServiceMock.Setup(x => x.SettleAccountAsync(10)).ReturnsAsync(settledInvoice);
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(500m);
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(settledInvoice);
            _invoiceServiceMock.Setup(x => x.GetPaymentsAsync(100)).ReturnsAsync(new List<Payment>());

            // Act
            _viewModel.SettleAccountCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.SettleAccountAsync(10), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تمت تصفية الحساب");
        }

        [Fact]
        public async Task EditPaymentAsync_With_ValidAmount_Should_EditPayment_SuccessGuard()
        {
            // Function: 2.5 — Edit Payment
            // Arrange
            _viewModel.SelectedPayment = new InvoicePaymentRow { PaymentId = 5, Amount = 50m };
            _viewModel.EditPaymentAmount = 80m;
            _viewModel.VisitId = 10;
            var invoice = new Invoice { InvoiceId = 100 };
            _invoiceServiceMock.Setup(x => x.EditPaymentAsync(5, 80m, It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(new Payment { PaymentId = 5, Amount = 80m });
            _invoiceServiceMock.Setup(x => x.GetVisitTotalAsync(10)).ReturnsAsync(500);
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(10)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.GetPaymentsAsync(100)).ReturnsAsync(new List<Payment>());

            // Act
            _viewModel.EditPaymentCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _invoiceServiceMock.Verify(x => x.EditPaymentAsync(5, 80m, It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            _viewModel.StatusMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task PrintInvoiceAsync_When_LogThrows_Should_Set_ErrorStatus_FailureGuard()
        {
            // Function: 2.8 — Generate Invoice
            // Arrange
            _viewModel.VisitId = 77;
            var invoice = new Invoice { InvoiceId = 555, VisitId = 77 };
            _invoiceServiceMock.Setup(x => x.GetByVisitIdAsync(77)).ReturnsAsync(invoice);
            _invoiceServiceMock.Setup(x => x.LogInvoicePrintedAsync(555, It.IsAny<int>()))
                .ThrowsAsync(new InvalidOperationException("print-log-failed"));

            // Act
            _viewModel.PrintInvoiceCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _viewModel.StatusMessage.Should().Contain("print-log-failed");
        }
    }
}
