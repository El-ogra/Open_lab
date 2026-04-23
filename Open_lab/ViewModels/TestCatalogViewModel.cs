using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class TestCatalogViewModel : BaseViewModel
    {
        private readonly ITestCatalogService _testCatalogService;
        private readonly IBarcodeService _barcodeService;
        private Test? _selectedTest;
        private string _code = string.Empty;
        private string _nameReport = string.Empty;
        private string _nameReceipt = string.Empty;
        private decimal _price;
        private int _turnaroundHours;
        private int _reportOrder;
        private bool _isRoutine;
        private bool _isSendOut;
        private decimal? _costPrice;
        private decimal? _patientPrice;
        private TestGroup? _selectedGroup;
        private SampleType? _selectedSampleType;
        private Unit? _selectedUnit;
        private string _statusMessage = string.Empty;
        private ImageSource? _barcodeImage;

        public TestCatalogViewModel(ITestCatalogService testCatalogService, IBarcodeService barcodeService)
        {
            _testCatalogService = testCatalogService;
            _barcodeService = barcodeService;
            Tests = new ObservableCollection<Test>();
            Groups = new ObservableCollection<TestGroup>();
            SampleTypes = new ObservableCollection<SampleType>();
            Units = new ObservableCollection<Unit>();

            SaveCommand = new RelayCommand(async _ => await SaveAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit));
            NewCommand = new RelayCommand(_ => ClearForm(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit));
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit) && SelectedTest != null);
            ReloadCommand = new RelayCommand(async _ => await LoadAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsView));
            GenerateBarcodeCommand = new RelayCommand(_ => GenerateBarcode(), _ => !string.IsNullOrWhiteSpace(Code));

            _ = LoadAsync();
        }

        public ObservableCollection<Test> Tests { get; }
        public ObservableCollection<TestGroup> Groups { get; }
        public ObservableCollection<SampleType> SampleTypes { get; }
        public ObservableCollection<Unit> Units { get; }

        public Test? SelectedTest
        {
            get => _selectedTest;
            set
            {
                if (SetProperty(ref _selectedTest, value))
                {
                    LoadFromSelected();
                    (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string Code
        {
            get => _code;
            set
            {
                if (SetProperty(ref _code, value))
                {
                    (GenerateBarcodeCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    GenerateBarcode();
                }
            }
        }

        public string NameReport
        {
            get => _nameReport;
            set => SetProperty(ref _nameReport, value);
        }

        public string NameReceipt
        {
            get => _nameReceipt;
            set => SetProperty(ref _nameReceipt, value);
        }

        public decimal Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        public int TurnaroundHours
        {
            get => _turnaroundHours;
            set => SetProperty(ref _turnaroundHours, value);
        }

        public int ReportOrder
        {
            get => _reportOrder;
            set => SetProperty(ref _reportOrder, value);
        }

        public bool IsRoutine
        {
            get => _isRoutine;
            set => SetProperty(ref _isRoutine, value);
        }

        public bool IsSendOut
        {
            get => _isSendOut;
            set => SetProperty(ref _isSendOut, value);
        }

        public decimal? CostPrice
        {
            get => _costPrice;
            set => SetProperty(ref _costPrice, value);
        }

        public decimal? PatientPrice
        {
            get => _patientPrice;
            set => SetProperty(ref _patientPrice, value);
        }

        public TestGroup? SelectedGroup
        {
            get => _selectedGroup;
            set => SetProperty(ref _selectedGroup, value);
        }

        public SampleType? SelectedSampleType
        {
            get => _selectedSampleType;
            set => SetProperty(ref _selectedSampleType, value);
        }

        public Unit? SelectedUnit
        {
            get => _selectedUnit;
            set => SetProperty(ref _selectedUnit, value);
        }

        public ImageSource? BarcodeImage
        {
            get => _barcodeImage;
            private set => SetProperty(ref _barcodeImage, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand GenerateBarcodeCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                var tests = await _testCatalogService.GetAllTestsAsync();
                Tests.Clear();
                foreach (var test in tests)
                {
                    Tests.Add(test);
                }

                var groups = await _testCatalogService.GetTestGroupsAsync();
                Groups.Clear();
                foreach (var group in groups)
                {
                    Groups.Add(group);
                }

                var sampleTypes = await _testCatalogService.GetSampleTypesAsync();
                SampleTypes.Clear();
                foreach (var sample in sampleTypes)
                {
                    SampleTypes.Add(sample);
                }

                var units = await _testCatalogService.GetUnitsAsync();
                Units.Clear();
                foreach (var unit in units)
                {
                    Units.Add(unit);
                }

                StatusMessage = "تم تحميل الكتالوج.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private void LoadFromSelected()
        {
            if (SelectedTest == null)
            {
                return;
            }

            Code = SelectedTest.Code;
            NameReport = SelectedTest.NameReport;
            NameReceipt = SelectedTest.NameReceipt;
            Price = SelectedTest.Price;
            TurnaroundHours = SelectedTest.TurnaroundHours;
            ReportOrder = SelectedTest.ReportOrder;
            IsRoutine = SelectedTest.IsRoutine;
            IsSendOut = SelectedTest.IsSendOut;
            CostPrice = SelectedTest.CostPrice;
            PatientPrice = SelectedTest.PatientPrice;

            SelectedGroup = Groups.FirstOrDefault(g => g.GroupId == SelectedTest.GroupId);
            SelectedSampleType = SampleTypes.FirstOrDefault(s => s.SampleTypeId == SelectedTest.SampleTypeId);
            SelectedUnit = Units.FirstOrDefault(u => u.UnitId == SelectedTest.UnitId);
            GenerateBarcode();
        }

        private async Task SaveAsync()
        {
            try
            {
                if (SelectedTest == null || SelectedTest.TestId == 0)
                {
                    var created = await _testCatalogService.CreateTestAsync(new Test
                    {
                        Code = Code,
                        NameReport = NameReport,
                        NameReceipt = NameReceipt,
                        Price = Price,
                        TurnaroundHours = TurnaroundHours,
                        ReportOrder = ReportOrder,
                        IsRoutine = IsRoutine,
                        IsSendOut = IsSendOut,
                        CostPrice = CostPrice,
                        PatientPrice = PatientPrice,
                        GroupId = SelectedGroup?.GroupId,
                        SampleTypeId = SelectedSampleType?.SampleTypeId,
                        UnitId = SelectedUnit?.UnitId
                    });

                    Tests.Add(created);
                    SelectedTest = created;
                    StatusMessage = "تم إنشاء التحليل.";
                }
                else
                {
                    SelectedTest.Code = Code;
                    SelectedTest.NameReport = NameReport;
                    SelectedTest.NameReceipt = NameReceipt;
                    SelectedTest.Price = Price;
                    SelectedTest.TurnaroundHours = TurnaroundHours;
                    SelectedTest.ReportOrder = ReportOrder;
                    SelectedTest.IsRoutine = IsRoutine;
                    SelectedTest.IsSendOut = IsSendOut;
                    SelectedTest.CostPrice = CostPrice;
                    SelectedTest.PatientPrice = PatientPrice;
                    SelectedTest.GroupId = SelectedGroup?.GroupId;
                    SelectedTest.SampleTypeId = SelectedSampleType?.SampleTypeId;
                    SelectedTest.UnitId = SelectedUnit?.UnitId;

                    await _testCatalogService.UpdateTestAsync(SelectedTest);
                    StatusMessage = "تم تحديث التحليل.";
                }

                GenerateBarcode();
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedTest == null)
            {
                return;
            }

            try
            {
                await _testCatalogService.DeleteTestAsync(SelectedTest.TestId);
                Tests.Remove(SelectedTest);
                ClearForm();
                StatusMessage = "تم حذف التحليل.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private void GenerateBarcode()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Code))
                {
                    BarcodeImage = null;
                    return;
                }

                BarcodeImage = _barcodeService.GenerateCode128(Code.Trim());
            }
            catch
            {
                BarcodeImage = null;
            }
        }

        private void ClearForm()
        {
            SelectedTest = null;
            Code = string.Empty;
            NameReport = string.Empty;
            NameReceipt = string.Empty;
            Price = 0;
            TurnaroundHours = 0;
            ReportOrder = 0;
            IsRoutine = false;
            IsSendOut = false;
            CostPrice = null;
            PatientPrice = null;
            SelectedGroup = null;
            SelectedSampleType = null;
            SelectedUnit = null;
            BarcodeImage = null;
        }
    }
}
