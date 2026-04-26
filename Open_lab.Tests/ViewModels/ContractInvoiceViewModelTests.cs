using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class ContractInvoiceViewModelTests
    {
        private readonly Mock<IContractInvoiceService> _contractServiceMock;
        private readonly Mock<ITestCatalogService> _catalogServiceMock;
        private readonly ContractInvoiceViewModel _viewModel;

        public ContractInvoiceViewModelTests()
        {
            _contractServiceMock = new Mock<IContractInvoiceService>();
            _catalogServiceMock = new Mock<ITestCatalogService>();
            _catalogServiceMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>());

            _viewModel = new ContractInvoiceViewModel(_contractServiceMock.Object, _catalogServiceMock.Object);
        }

        [Fact]
        public async Task LoadPendingCommand_Should_Load_Pending_Invoices_And_Total_SuccessGuard()
        {
            _catalogServiceMock.Setup(s => s.GetReferralsAsync()).ReturnsAsync(new List<Referral>
            {
                new() { ReferralId = 1, Name = "Corp A", ReferralType = "Company" }
            });

            _contractServiceMock.Setup(s => s.GetPendingInvoicesAsync(1, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<BulkClaimRow>
                {
                    new() { InvoiceId = 10, LabId = "L1", PatientName = "P1", NetTotal = 200m },
                    new() { InvoiceId = 11, LabId = "L2", PatientName = "P2", NetTotal = 300m }
                });

            _viewModel.SelectedReferralId = 1;
            _viewModel.LoadPendingCommand.Execute(null);
            await Task.Delay(50);

            _viewModel.PendingInvoices.Should().HaveCount(2);
            _viewModel.TotalPending.Should().Be(500m);
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
        }

        [Fact]
        public async Task CreateInvoiceCommand_Should_Create_And_Refresh_Data_SuccessGuard()
        {
            _viewModel.SelectedReferralId = 3;
            _viewModel.InvoiceNumber = "INV-2026-01";
            _contractServiceMock.Setup(s => s.GetPendingInvoicesAsync(3, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<BulkClaimRow> { new() { InvoiceId = 15, NetTotal = 150m } });
            _contractServiceMock.Setup(s => s.CreateContractInvoiceAsync(3, "INV-2026-01", It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(9001);
            _contractServiceMock.Setup(s => s.GetContractInvoicesAsync(3))
                .ReturnsAsync(new List<ContractInvoice> { new() { ContractInvoiceId = 9001, ReferralId = 3, InvoiceNumber = "INV-2026-01" } });

            _viewModel.LoadPendingCommand.Execute(null);
            await Task.Delay(50);
            _viewModel.CreateInvoiceCommand.Execute(null);
            await Task.Delay(80);

            _contractServiceMock.Verify(s => s.CreateContractInvoiceAsync(3, "INV-2026-01", It.IsAny<DateTime>(), It.IsAny<DateTime>()), Times.Once);
            _viewModel.StatusMessage.Should().Contain("تم تحميل");
        }

        [Fact]
        public async Task SettleSelectedCommand_When_Service_Fails_Should_Set_Error_Message_FailureGuard()
        {
            var contractInvoice = new ContractInvoice { ContractInvoiceId = 20, ReferralId = 5, InvoiceNumber = "X1", IsPaid = false };
            _viewModel.SelectedContractInvoice = contractInvoice;
            _contractServiceMock.Setup(s => s.SettleContractInvoiceAsync(20))
                .ThrowsAsync(new InvalidOperationException("Already paid"));

            _viewModel.SettleSelectedCommand.Execute(null);
            await Task.Delay(50);

            _viewModel.StatusMessage.Should().Contain("Already paid");
        }
    }
}
