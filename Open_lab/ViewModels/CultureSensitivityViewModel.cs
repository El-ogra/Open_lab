using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class CultureSensitivityViewModel : BaseViewModel
    {
        private static readonly string[] SensitivityOptions = { "", "S", "I", "R" };

        private readonly ICultureSensitivityService _service;
        private readonly IPrintService? _printService;
        private Culture? _selectedCulture;
        private Antibiotic? _selectedAntibiotic;
        private CultureAntibiotic? _selectedLink;
        private string _newCultureName = string.Empty;
        private string _newCultureSampleType = string.Empty;
        private string _newCultureOrganism = string.Empty;
        private string _newCultureConditions = string.Empty;
        private int _newCultureColonyCount = 1;
        private string _newAntibioticName = string.Empty;
        private bool _newAntibioticSafeForPregnancy = true;
        private bool _newAntibioticSafeForChildren = true;
        private string _statusMessage = string.Empty;
        private string _labIdFilter = string.Empty;
        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private CultureVisitTestRow? _selectedVisitTest;

        public CultureSensitivityViewModel(ICultureSensitivityService service, IPrintService? printService = null)
        {
            _service = service;
            _printService = printService;
            Cultures = new ObservableCollection<Culture>();
            Antibiotics = new ObservableCollection<Antibiotic>();
            LinkedAntibiotics = new ObservableCollection<CultureAntibiotic>();
            VisitTests = new ObservableCollection<CultureVisitTestRow>();
            ResultRows = new ObservableCollection<CultureSensitivityRow>();
            AllowedSensitivities = new ObservableCollection<string>(SensitivityOptions);

            LoadCommand = new RelayCommand(async _ => await LoadAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsView));
            AddCultureCommand = new RelayCommand(async _ => await AddCultureAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit));
            DeleteCultureCommand = new RelayCommand(async _ => await DeleteCultureAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit) && SelectedCulture != null);
            AddAntibioticCommand = new RelayCommand(async _ => await AddAntibioticAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit));
            DeleteAntibioticCommand = new RelayCommand(async _ => await DeleteAntibioticAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit) && SelectedAntibiotic != null);
            LinkCommand = new RelayCommand(async _ => await LinkAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit) && SelectedCulture != null && SelectedAntibiotic != null);
            UnlinkCommand = new RelayCommand(async _ => await UnlinkAsync(), _ => AppSession.HasPermission(PermissionCodes.TestsEdit) && SelectedLink != null);

            LoadVisitTestsCommand = new RelayCommand(async _ => await LoadVisitTestsAsync(), _ => AppSession.HasPermission(PermissionCodes.ResultsView));
            SaveResultCommand = new RelayCommand(async _ => await SaveResultAsync(), _ => AppSession.HasPermission(PermissionCodes.ResultsEdit) && SelectedVisitTest != null && SelectedCulture != null);
            PrintCultureReportCommand = new RelayCommand(async _ => await PrintCultureReportAsync(), _ => _printService != null && SelectedVisitTest != null && SelectedCulture != null);

            _ = LoadAsync();
        }

        public ObservableCollection<Culture> Cultures { get; }
        public ObservableCollection<Antibiotic> Antibiotics { get; }
        public ObservableCollection<CultureAntibiotic> LinkedAntibiotics { get; }
        public ObservableCollection<CultureVisitTestRow> VisitTests { get; }
        public ObservableCollection<CultureSensitivityRow> ResultRows { get; }
        public ObservableCollection<string> AllowedSensitivities { get; }

        public Culture? SelectedCulture
        {
            get => _selectedCulture;
            set
            {
                if (SetProperty(ref _selectedCulture, value))
                {
                    _ = LoadLinkedAsync();
                    _ = BuildResultRowsAsync();
                    (DeleteCultureCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (LinkCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (SaveResultCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (PrintCultureReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public Antibiotic? SelectedAntibiotic
        {
            get => _selectedAntibiotic;
            set
            {
                if (SetProperty(ref _selectedAntibiotic, value))
                {
                    (DeleteAntibioticCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (LinkCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public CultureAntibiotic? SelectedLink
        {
            get => _selectedLink;
            set
            {
                if (SetProperty(ref _selectedLink, value))
                {
                    (UnlinkCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public CultureVisitTestRow? SelectedVisitTest
        {
            get => _selectedVisitTest;
            set
            {
                if (SetProperty(ref _selectedVisitTest, value))
                {
                    _ = BuildResultRowsAsync();
                    (SaveResultCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (PrintCultureReportCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string LabIdFilter
        {
            get => _labIdFilter;
            set => SetProperty(ref _labIdFilter, value);
        }

        public DateTime DateFrom
        {
            get => _dateFrom;
            set => SetProperty(ref _dateFrom, value);
        }

        public DateTime DateTo
        {
            get => _dateTo;
            set => SetProperty(ref _dateTo, value);
        }

        public string NewCultureName
        {
            get => _newCultureName;
            set => SetProperty(ref _newCultureName, value);
        }

        public string NewAntibioticName
        {
            get => _newAntibioticName;
            set => SetProperty(ref _newAntibioticName, value);
        }

        public string NewCultureSampleType
        {
            get => _newCultureSampleType;
            set => SetProperty(ref _newCultureSampleType, value);
        }

        public string NewCultureOrganism
        {
            get => _newCultureOrganism;
            set => SetProperty(ref _newCultureOrganism, value);
        }

        public string NewCultureConditions
        {
            get => _newCultureConditions;
            set => SetProperty(ref _newCultureConditions, value);
        }

        public int NewCultureColonyCount
        {
            get => _newCultureColonyCount;
            set => SetProperty(ref _newCultureColonyCount, value);
        }

        public bool NewAntibioticSafeForPregnancy
        {
            get => _newAntibioticSafeForPregnancy;
            set => SetProperty(ref _newAntibioticSafeForPregnancy, value);
        }

        public bool NewAntibioticSafeForChildren
        {
            get => _newAntibioticSafeForChildren;
            set => SetProperty(ref _newAntibioticSafeForChildren, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand AddCultureCommand { get; }
        public ICommand DeleteCultureCommand { get; }
        public ICommand AddAntibioticCommand { get; }
        public ICommand DeleteAntibioticCommand { get; }
        public ICommand LinkCommand { get; }
        public ICommand UnlinkCommand { get; }
        public ICommand LoadVisitTestsCommand { get; }
        public ICommand SaveResultCommand { get; }
        public ICommand PrintCultureReportCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                var cultures = await _service.GetCulturesAsync();
                Cultures.Clear();
                foreach (var c in cultures)
                {
                    Cultures.Add(c);
                }

                var antibiotics = await _service.GetAntibioticsAsync();
                Antibiotics.Clear();
                foreach (var a in antibiotics)
                {
                    Antibiotics.Add(a);
                }

                await BuildResultRowsAsync();
                StatusMessage = "تم تحميل البيانات.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadLinkedAsync()
        {
            LinkedAntibiotics.Clear();
            if (SelectedCulture == null)
            {
                return;
            }

            try
            {
                var links = await _service.GetCultureAntibioticsAsync(SelectedCulture.CultureId);
                foreach (var link in links)
                {
                    LinkedAntibiotics.Add(link);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task BuildResultRowsAsync()
        {
            ResultRows.Clear();
            if (SelectedCulture == null)
            {
                return;
            }

            var links = await _service.GetCultureAntibioticsAsync(SelectedCulture.CultureId);
            if (SelectedVisitTest != null)
            {
                var allowed = await _service.GetFilteredAntibioticsAsync(SelectedVisitTest.VisitTestId);
                var allowedIds = allowed.Select(a => a.AntibioticId).ToHashSet();
                links = links.Where(link => allowedIds.Contains(link.AntibioticId)).ToList();
            }

            foreach (var link in links)
            {
                ResultRows.Add(new CultureSensitivityRow
                {
                    AntibioticId = link.AntibioticId,
                    AntibioticName = link.Antibiotic.Name,
                    Sensitivity = string.Empty,
                    Comment = null
                });
            }
        }

        private async Task AddCultureAsync()
        {
            if (string.IsNullOrWhiteSpace(NewCultureName))
            {
                StatusMessage = "أدخل اسم المزرعة.";
                return;
            }
            if (string.IsNullOrWhiteSpace(NewCultureSampleType))
            {
                StatusMessage = "أدخل نوع العينة.";
                return;
            }
            if (string.IsNullOrWhiteSpace(NewCultureOrganism))
            {
                StatusMessage = "أدخل الكائن الدقيق المعزول.";
                return;
            }
            if (string.IsNullOrWhiteSpace(NewCultureConditions))
            {
                StatusMessage = "أدخل ظروف النمو.";
                return;
            }
            if (NewCultureColonyCount <= 0)
            {
                StatusMessage = "عدد المستعمرات يجب أن يكون أكبر من صفر.";
                return;
            }

            try
            {
                var culture = await _service.CreateCultureAsync(new Culture
                {
                    Name = NewCultureName,
                    SampleType = NewCultureSampleType,
                    IsolatedOrganism = NewCultureOrganism,
                    GrowthConditions = NewCultureConditions,
                    ColonyCount = NewCultureColonyCount
                });
                Cultures.Add(culture);
                NewCultureName = string.Empty;
                NewCultureSampleType = string.Empty;
                NewCultureOrganism = string.Empty;
                NewCultureConditions = string.Empty;
                NewCultureColonyCount = 1;
                StatusMessage = "تم إضافة المزرعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task DeleteCultureAsync()
        {
            if (SelectedCulture == null)
            {
                return;
            }

            try
            {
                await _service.DeleteCultureAsync(SelectedCulture.CultureId);
                Cultures.Remove(SelectedCulture);
                SelectedCulture = null;
                LinkedAntibiotics.Clear();
                ResultRows.Clear();
                StatusMessage = "تم حذف المزرعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task AddAntibioticAsync()
        {
            if (string.IsNullOrWhiteSpace(NewAntibioticName))
            {
                StatusMessage = "أدخل اسم المضاد الحيوي.";
                return;
            }

            try
            {
                var antibiotic = await _service.CreateAntibioticAsync(new Antibiotic
                {
                    Name = NewAntibioticName,
                    IsSafeForPregnancy = NewAntibioticSafeForPregnancy,
                    IsSafeForChildren = NewAntibioticSafeForChildren
                });
                Antibiotics.Add(antibiotic);
                NewAntibioticName = string.Empty;
                NewAntibioticSafeForPregnancy = true;
                NewAntibioticSafeForChildren = true;
                StatusMessage = "تم إضافة المضاد الحيوي.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task DeleteAntibioticAsync()
        {
            if (SelectedAntibiotic == null)
            {
                return;
            }

            try
            {
                await _service.DeleteAntibioticAsync(SelectedAntibiotic.AntibioticId);
                Antibiotics.Remove(SelectedAntibiotic);
                SelectedAntibiotic = null;
                StatusMessage = "تم حذف المضاد الحيوي.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LinkAsync()
        {
            if (SelectedCulture == null || SelectedAntibiotic == null)
            {
                return;
            }

            try
            {
                await _service.LinkAntibioticAsync(SelectedCulture.CultureId, SelectedAntibiotic.AntibioticId);
                await LoadLinkedAsync();
                await BuildResultRowsAsync();
                StatusMessage = "تم ربط المضاد بالمزرعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task UnlinkAsync()
        {
            if (SelectedLink == null)
            {
                return;
            }

            try
            {
                await _service.UnlinkAntibioticAsync(SelectedLink.CultureId, SelectedLink.AntibioticId);
                await LoadLinkedAsync();
                await BuildResultRowsAsync();
                SelectedLink = null;
                StatusMessage = "تم فك الربط.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task LoadVisitTestsAsync()
        {
            try
            {
                var from = DateFrom.Date;
                var to = DateTo.Date.AddDays(1).AddSeconds(-1);
                var rows = await _service.SearchCultureVisitTestsAsync(LabIdFilter, from, to);

                VisitTests.Clear();
                foreach (var row in rows)
                {
                    VisitTests.Add(row);
                }

                StatusMessage = $"تم تحميل {VisitTests.Count} طلب مزرعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task SaveResultAsync()
        {
            if (SelectedVisitTest == null || SelectedCulture == null)
            {
                StatusMessage = "اختر الزيارة والمزرعة أولًا.";
                return;
            }

            try
            {
                var values = ResultRows
                    .Where(r => !string.IsNullOrWhiteSpace(r.Sensitivity))
                    .Select(r => new CultureSensitivityValue
                    {
                        AntibioticId = r.AntibioticId,
                        Sensitivity = _service.ClassifySensitivity(r.Sensitivity),
                        Comment = r.Comment
                    })
                    .ToList();

                if (values.Count == 0)
                {
                    StatusMessage = "لا توجد نتائج لحفظها.";
                    return;
                }

                await _service.SaveCultureResultAsync(SelectedVisitTest.VisitTestId, SelectedCulture.CultureId, values);
                StatusMessage = "تم حفظ نتيجة المزرعة وربطها بالزيارة بنجاح.";
                await LoadVisitTestsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task PrintCultureReportAsync()
        {
            if (_printService == null || SelectedVisitTest == null || SelectedCulture == null)
            {
                StatusMessage = "بيانات الطباعة غير مكتملة.";
                return;
            }

            try
            {
                var data = new CultureReportData
                {
                    VisitId = SelectedVisitTest.VisitId,
                    PatientName = SelectedVisitTest.PatientName,
                    LabId = SelectedVisitTest.LabId,
                    CultureName = SelectedCulture.Name,
                    VisitDate = SelectedVisitTest.VisitDate,
                    Results = ResultRows
                        .Where(r => !string.IsNullOrWhiteSpace(r.Sensitivity))
                        .Select(r => new CultureResultRow
                        {
                            AntibioticName = r.AntibioticName,
                            Sensitivity = _service.ClassifySensitivity(r.Sensitivity),
                            Comment = r.Comment
                        })
                        .ToList()
                };

                await _printService.PrintCultureReportAsync(data);
                StatusMessage = "تم إرسال تقرير المزرعة للطباعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
