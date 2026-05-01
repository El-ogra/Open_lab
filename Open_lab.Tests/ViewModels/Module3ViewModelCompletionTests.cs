using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Open_lab.Models;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;
using Xunit;

namespace Open_lab.Tests.ViewModels
{
    public class Module3ViewModelCompletionTests : IDisposable
    {
        private readonly Mock<ITestCatalogService> _catalogMock;
        private readonly Mock<IBarcodeService> _barcodeMock;
        private readonly Mock<IPrintService> _printMock;

        public Module3ViewModelCompletionTests()
        {
            AppSessionTestHelper.ResetToAdmin();
            _catalogMock = new Mock<ITestCatalogService>();
            _barcodeMock = new Mock<IBarcodeService>();
            _printMock = new Mock<IPrintService>();
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        // ────────────────────────────────────────────────────────────────────────
        // 3.1 / 3.2 / 3.9 — Test Catalog (TestCatalogViewModel)
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public void TestCatalog_NewCommand_ShouldResetForm_SuccessGuard()
        {
            // Function: 3.1 — Add New Test
            var vm = new TestCatalogViewModel(_catalogMock.Object, _barcodeMock.Object);
            vm.SelectedTest = new Test { TestId = 1, Code = "T1" };
            vm.Code = "T1";

            vm.NewCommand.Execute(null);

            vm.SelectedTest.Should().BeNull();
            vm.Code.Should().BeEmpty();
        }

        [Fact]
        public async Task TestCatalog_Save_WhenIsSendOut_ShouldIncludePricing_LogicGuard()
        {
            // Function: 3.9 — Mark as Outsourced
            var vm = new TestCatalogViewModel(_catalogMock.Object, _barcodeMock.Object);
            vm.Code = "OUT1";
            vm.NameReport = "Outsource Test";
            vm.NameReceipt = "Outsource Test";
            vm.IsSendOut = true;
            vm.CostPrice = 100m;
            vm.PatientPrice = 200m;

            _catalogMock.Setup(x => x.CreateTestAsync(It.IsAny<Test>()))
                .ReturnsAsync((Test t) => { t.TestId = 99; return t; });

            await vm.InvokePrivateAsync("SaveAsync");

            _catalogMock.Verify(x => x.CreateTestAsync(It.Is<Test>(t => 
                t.IsSendOut && t.CostPrice == 100m && t.PatientPrice == 200m)), Times.Once);
        }

        // ────────────────────────────────────────────────────────────────────────
        // 3.3 — Reference Ranges (ReferenceRangesViewModel)
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task ReferenceRanges_Save_WithInvalidBoundaries_ShouldShowError_FailureGuard()
        {
            // Function: 3.3 — Set Reference Values
            var vm = new ReferenceRangesViewModel(_catalogMock.Object);
            vm.SelectedTest = new Test { TestId = 1 };
            vm.AgeFrom = 50;
            vm.AgeTo = 10; // Invalid range

            _catalogMock.Setup(x => x.CreateReferenceRangeAsync(It.IsAny<TestReferenceRange>()))
                .ThrowsAsync(new ArgumentException("age range invalid"));

            await vm.InvokePrivateAsync("SaveAsync");

            vm.StatusMessage.Should().Contain("خطأ: age range invalid");
        }

        // ────────────────────────────────────────────────────────────────────────
        // 3.4 / 3.6 — Test Comments (TestCommentsViewModel)
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task TestComments_Save_WithValidComment_ShouldCallService_SuccessGuard()
        {
            // Function: 3.6 — Add Test Comments
            var vm = new TestCommentsViewModel(_catalogMock.Object);
            vm.SelectedTest = new Test { TestId = 5 };
            vm.CommentText = "New Comment";
            vm.LowComment = "Low Msg";
            vm.HighComment = "High Msg";

            _catalogMock.Setup(x => x.CreateTestCommentAsync(It.IsAny<TestComment>()))
                .ReturnsAsync(new TestComment { CommentId = 1, CommentText = "New Comment" });

            await vm.InvokePrivateAsync("SaveAsync");

            _catalogMock.Verify(x => x.CreateTestCommentAsync(It.Is<TestComment>(c => 
                c.TestId == 5 && c.CommentText == "New Comment" && c.LowComment == "Low Msg" && c.HighComment == "High Msg")), Times.Once);
            vm.StatusMessage.Should().Contain("تم حفظ");
        }

        [Fact]
        public async Task TestComments_Delete_WhenSelected_ShouldCallService_SuccessGuard()
        {
            // Function: 3.6 — Add Test Comments
            var vm = new TestCommentsViewModel(_catalogMock.Object);
            var comment = new TestComment { CommentId = 99 };
            vm.SelectedComment = comment;

            await vm.InvokePrivateAsync("DeleteAsync");

            _catalogMock.Verify(x => x.DeleteTestCommentAsync(99), Times.Once);
            vm.Comments.Should().NotContain(comment);
        }

        // ────────────────────────────────────────────────────────────────────────
        // 3.5 — Custom Groups (CustomGroupsViewModel)
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task CustomGroups_DeleteItem_WhenSelected_ShouldCallService_SuccessGuard()
        {
            // Function: 3.5 — Create Custom Group
            var vm = new CustomGroupsViewModel(_catalogMock.Object);
            var item = new CustomGroupItem { CustomGroupItemId = 12 };
            vm.SelectedItem = item; // Property is SelectedItem, not SelectedGroupItem

            await vm.InvokePrivateAsync("DeleteItemAsync");

            _catalogMock.Verify(x => x.DeleteCustomGroupItemAsync(12), Times.Once);
        }

        // ────────────────────────────────────────────────────────────────────────
        // 3.7 / 3.8 — Price Lists (PriceListsViewModel)
        // ────────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task PriceLists_DeleteItem_WhenSelected_ShouldCallService_SuccessGuard()
        {
            // Function: 3.8 — Update Prices
            var vm = new PriceListsViewModel(_catalogMock.Object, _printMock.Object);
            var item = new PriceListItem { PriceListItemId = 44 };
            vm.SelectedItem = item;

            await vm.InvokePrivateAsync("DeleteItemAsync");

            _catalogMock.Verify(x => x.DeletePriceListItemAsync(44), Times.Once);
        }

        [Fact]
        public async Task PriceLists_Print_ShouldCallPrintService_SuccessGuard()
        {
            // Function: 3.7 — Create Price List
            var vm = new PriceListsViewModel(_catalogMock.Object, _printMock.Object);
            var pl = new PriceList { PriceListId = 1, Name = "List" };
            vm.SelectedPriceList = pl;
            vm.Items.Add(new PriceListItem { PriceListItemId = 1, TestId = 1 });

            await vm.InvokePrivateAsync("PrintListAsync");

            _printMock.Verify(x => x.PrintTextReportAsync(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<string>()), Times.Once);
        }
    }
}
