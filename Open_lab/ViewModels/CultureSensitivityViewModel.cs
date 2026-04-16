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
        private Culture? _selectedCulture;
        private Antibiotic? _selectedAntibiotic;
        private CultureAntibiotic? _selectedLink;
        private string _newCultureName = string.Empty;
        private string _newAntibioticName = string.Empty;
        private string _statusMessage = string.Empty;
        private string _labIdFilter = string.Empty;
        private DateTime _dateFrom = DateTime.Today;
        private DateTime _dateTo = DateTime.Today;
        private CultureVisitTestRow? _selectedVisitTest;

        public CultureSensitivityViewModel(ICultureSensitivityService service)
        {
            _service = service;
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
                    (SaveResultCommand as RelayCommand)?.RaiseCanExecuteChanged();
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

            try
            {
                var culture = await _service.CreateCultureAsync(new Culture { Name = NewCultureName });
                Cultures.Add(culture);
                NewCultureName = string.Empty;
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
                var antibiotic = await _service.CreateAntibioticAsync(new Antibiotic { Name = NewAntibioticName });
                Antibiotics.Add(antibiotic);
                NewAntibioticName = string.Empty;
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
                        Sensitivity = r.Sensitivity,
                        Comment = r.Comment
                    })
                    .ToList();

                await _service.SaveCultureResultAsync(SelectedVisitTest.VisitTestId, SelectedCulture.CultureId, values);
                StatusMessage = "تم حفظ نتيجة المزرعة وربطها بالزيارة بنجاح.";
                await LoadVisitTestsAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
