using System.Windows.Media;
using FluentAssertions;
using Moq;
using Open_lab.Services;
using Open_lab.Tests.Infrastructure;
using Open_lab.ViewModels;

namespace Open_lab.Tests.ViewModels
{
    /// <summary>
    /// اختبارات BarcodeDialogViewModel — تغطي الأوامر الخمسة الرئيسية:
    /// PrintAllCommand, PrintCaseCommand, PrintFileCommand, PrintLabCommand, PrintTubesCommand.
    /// النمط مأخوذ من DeliveryViewModelTests:
    ///   - Mock للخدمات الحقيقية (IBarcodeService, IPrintService)
    ///   - استدعاء Command.Execute(null) مباشرة (وليس InvokePrivateAsync)
    ///   - Mock.Verify على استدعاءات الخدمة الفعلية وليس مجرد StatusMessage
    /// </summary>
    public class BarcodeDialogViewModelTests : IDisposable
    {
        private readonly Mock<IBarcodeService> _barcodeServiceMock = new();
        private readonly Mock<IPrintService> _printServiceMock = new();

        public BarcodeDialogViewModelTests()
        {
            AppSessionTestHelper.ResetToAdmin();

            // أي محتوى → نُرجع DrawingImage بسيط (لا أهمية للمحتوى البصري في الاختبار)
            _barcodeServiceMock
                .Setup(x => x.GenerateCode128(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns<string, int, int>((content, w, h) => new DrawingImage());

            // الطباعة تُكمل بنجاح (Task.CompletedTask) لكل المسارات
            _printServiceMock
                .Setup(x => x.PrintBarcodeImageAsync(
                    It.IsAny<string>(),
                    It.IsAny<ImageSource?>(),
                    It.IsAny<string>(),
                    It.IsAny<string?>()))
                .Returns(Task.CompletedTask);
        }

        public void Dispose()
        {
            AppSessionTestHelper.Reset();
        }

        private BarcodeDialogViewModel CreateViewModel(BarcodeDialogData? data = null)
        {
            data ??= new BarcodeDialogData
            {
                PatientName = "Test Patient",
                Gender = "Male",
                Age = 30,
                AgeUnit = "Years",
                CaseCode = "CASE-1",
                FileCode = "FILE-1",
                LabCode = "LAB-1",
                BarcodeDate = new DateTime(2026, 5, 16, 10, 30, 0),
                SampleLabels = new List<string> { "CBC", "ESR", "Urine" }
            };

            return new BarcodeDialogViewModel(
                _barcodeServiceMock.Object,
                _printServiceMock.Object,
                data);
        }

        [Fact]
        public async Task PrintCaseCommand_Should_Call_PrintBarcodeImageAsync_With_CaseCode()
        {
            // Arrange
            var vm = CreateViewModel();

            // Act
            vm.PrintCaseCommand.Execute(null);
            await Task.Delay(100);

            // Assert — تحقق من خدمة الطباعة الحقيقية وليس مجرد رسالة حالة
            _printServiceMock.Verify(x => x.PrintBarcodeImageAsync(
                "كود الحالة",
                It.IsAny<ImageSource?>(),
                "CASE-1",
                It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task PrintFileCommand_Should_Call_PrintBarcodeImageAsync_With_FileCode()
        {
            // Arrange
            var vm = CreateViewModel();

            // Act
            vm.PrintFileCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _printServiceMock.Verify(x => x.PrintBarcodeImageAsync(
                "كود الملف",
                It.IsAny<ImageSource?>(),
                "FILE-1",
                It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task PrintLabCommand_Should_Call_PrintBarcodeImageAsync_With_LabCode()
        {
            // Arrange
            var vm = CreateViewModel();

            // Act
            vm.PrintLabCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _printServiceMock.Verify(x => x.PrintBarcodeImageAsync(
                "كود المعمل",
                It.IsAny<ImageSource?>(),
                "LAB-1",
                It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task PrintTubesCommand_Should_Call_PrintBarcodeImageAsync_For_Each_Tube_Label()
        {
            // Arrange — ثلاث ملصقات → ثلاث استدعاءات للطباعة (كل ملصق له باركود فريد)
            var data = new BarcodeDialogData
            {
                PatientName = "Tube Patient",
                Gender = "Female",
                Age = 25,
                AgeUnit = "Years",
                CaseCode = "C2",
                FileCode = "F2",
                LabCode = "LAB-2",
                SampleLabels = new List<string> { "CBC", "ESR", "Urine" }
            };
            var vm = CreateViewModel(data);

            // Act
            vm.PrintTubesCommand.Execute(null);
            await Task.Delay(150);

            // Assert — كل ملصق يجب أن يُرسل للطباعة مع كود فريد يتضمن اسم العينة + LabCode
            _printServiceMock.Verify(x => x.PrintBarcodeImageAsync(
                "ملصق: CBC",
                It.IsAny<ImageSource?>(),
                "LAB-2-CBC",
                It.IsAny<string?>()), Times.Once);

            _printServiceMock.Verify(x => x.PrintBarcodeImageAsync(
                "ملصق: ESR",
                It.IsAny<ImageSource?>(),
                "LAB-2-ESR",
                It.IsAny<string?>()), Times.Once);

            _printServiceMock.Verify(x => x.PrintBarcodeImageAsync(
                "ملصق: Urine",
                It.IsAny<ImageSource?>(),
                "LAB-2-Urine",
                It.IsAny<string?>()), Times.Once);

            _printServiceMock.Verify(x => x.PrintBarcodeImageAsync(
                It.IsAny<string>(),
                It.IsAny<ImageSource?>(),
                It.IsAny<string>(),
                It.IsAny<string?>()), Times.Exactly(3));
        }

        [Fact]
        public async Task PrintTubesCommand_When_No_Sample_Labels_Should_Not_Call_Print_Service()
        {
            // Arrange — لا توجد ملصقات → لا طباعة
            var data = new BarcodeDialogData
            {
                PatientName = "Empty",
                CaseCode = "C3",
                FileCode = "F3",
                LabCode = "L3",
                SampleLabels = new List<string>()
            };
            var vm = CreateViewModel(data);

            // Act
            vm.PrintTubesCommand.Execute(null);
            await Task.Delay(100);

            // Assert
            _printServiceMock.Verify(x => x.PrintBarcodeImageAsync(
                It.IsAny<string>(),
                It.IsAny<ImageSource?>(),
                It.IsAny<string>(),
                It.IsAny<string?>()), Times.Never);
        }

        [Fact]
        public async Task PrintAllCommand_Should_Generate_Barcodes_For_Case_File_Lab_And_Tubes()
        {
            // Arrange — الأمر PrintAll يبني FlowDocument يحوي الباركودات الثلاثة + ملصقات الأنابيب،
            // ثم يفتح PrintDialog. في بيئة headless، فشل PrintDialog يُتجاهل بصمت.
            // المنطق الذي يمكن التحقق منه: استدعاء IBarcodeService.GenerateCode128 لكل من
            // CaseCode/FileCode/LabCode + كل ملصق أنبوب — وهذا يتم في constructor الـ ViewModel.
            var data = new BarcodeDialogData
            {
                PatientName = "All Patient",
                Gender = "Male",
                Age = 40,
                AgeUnit = "Years",
                CaseCode = "ALL-C",
                FileCode = "ALL-F",
                LabCode = "ALL-L",
                SampleLabels = new List<string> { "CBC", "ESR" }
            };

            // Act — إنشاء الـ ViewModel يولّد الباركودات، ثم استدعاء PrintAllCommand
            var vm = CreateViewModel(data);
            vm.PrintAllCommand.Execute(null);
            await Task.Delay(100);

            // Assert — IBarcodeService استُدعي لـ Case/File/Lab + كل ملصق
            _barcodeServiceMock.Verify(x => x.GenerateCode128("ALL-C", It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            _barcodeServiceMock.Verify(x => x.GenerateCode128("ALL-F", It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            _barcodeServiceMock.Verify(x => x.GenerateCode128("ALL-L", It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            _barcodeServiceMock.Verify(x => x.GenerateCode128("ALL-L-CBC", It.IsAny<int>(), It.IsAny<int>()), Times.Once);
            _barcodeServiceMock.Verify(x => x.GenerateCode128("ALL-L-ESR", It.IsAny<int>(), It.IsAny<int>()), Times.Once);

            // التحقق من ObservableCollection — تأكيد أن كل ملصق له باركود فريد
            vm.TubeLabels.Should().HaveCount(2);
            vm.TubeLabels.Should().Contain(t => t.Text == "CBC" && t.Code == "ALL-L-CBC");
            vm.TubeLabels.Should().Contain(t => t.Text == "ESR" && t.Code == "ALL-L-ESR");
        }
    }
}
