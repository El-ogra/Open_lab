using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly IStatisticsService _statisticsService;
        private readonly IUserProductivityService _userProductivityService;
        private readonly IPrintService _printService;
        private DateTime _from = DateTime.Today.AddDays(-30);
        private DateTime _to = DateTime.Today;
        private string _selectedGender = "الكل";
        private StatisticsReferralLookup? _selectedReferral;
        private int _visitCount;
        private int _patientCount;
        private int _testCount;
        private decimal _totalRevenue;
        private decimal _totalPaid;
        private string _statusMessage = string.Empty;

        public StatisticsViewModel(IStatisticsService statisticsService, IUserProductivityService userProductivityService, IPrintService printService)
        {
            _statisticsService = statisticsService;
            _userProductivityService = userProductivityService;
            _printService = printService;

            Genders = new ObservableCollection<string> { "الكل", "ذكر", "أنثى" };
            Referrals = new ObservableCollection<StatisticsReferralLookup>();
            ByGender = new ObservableCollection<StatisticsGenderRow>();
            ByReferral = new ObservableCollection<StatisticsReferralRow>();
            MonthlyAnalysis = new ObservableCollection<MonthlyAnalysisRow>();
            TopTests = new ObservableCollection<TopTestRow>();
            YearlySamples = new ObservableCollection<YearlySampleRow>();
            UserProductivity = new ObservableCollection<UserPerformanceRow>();

            LoadCommand = new RelayCommand(async _ => await LoadAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.StatisticsView));
            PrintCommand = new RelayCommand(async _ => await PrintAsync(), _ => SessionContext.Current.HasPermission(PermissionCodes.StatisticsView));

            _ = InitializeAsync();
        }

        public DateTime From
        {
            get => _from;
            set => SetProperty(ref _from, value);
        }

        public DateTime To
        {
            get => _to;
            set => SetProperty(ref _to, value);
        }

        public ObservableCollection<string> Genders { get; }
        public ObservableCollection<StatisticsReferralLookup> Referrals { get; }
        public ObservableCollection<StatisticsGenderRow> ByGender { get; }
        public ObservableCollection<StatisticsReferralRow> ByReferral { get; }
        public ObservableCollection<MonthlyAnalysisRow> MonthlyAnalysis { get; }
        public ObservableCollection<TopTestRow> TopTests { get; }
        public ObservableCollection<YearlySampleRow> YearlySamples { get; }
        public ObservableCollection<UserPerformanceRow> UserProductivity { get; }

        public string SelectedGender
        {
            get => _selectedGender;
            set => SetProperty(ref _selectedGender, value);
        }

        public StatisticsReferralLookup? SelectedReferral
        {
            get => _selectedReferral;
            set => SetProperty(ref _selectedReferral, value);
        }

        public int VisitCount
        {
            get => _visitCount;
            private set => SetProperty(ref _visitCount, value);
        }

        public int PatientCount
        {
            get => _patientCount;
            private set => SetProperty(ref _patientCount, value);
        }

        public int TestCount
        {
            get => _testCount;
            private set => SetProperty(ref _testCount, value);
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            private set => SetProperty(ref _totalRevenue, value);
        }

        public decimal TotalPaid
        {
            get => _totalPaid;
            private set => SetProperty(ref _totalPaid, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand PrintCommand { get; }

        private async Task InitializeAsync()
        {
            try
            {
                var referrals = await _statisticsService.GetReferralsAsync();
                Referrals.Clear();
                Referrals.Add(new StatisticsReferralLookup { ReferralId = null, Name = "الكل" });
                foreach (var referral in referrals)
                {
                    Referrals.Add(referral);
                }

                SelectedReferral = Referrals.FirstOrDefault();
                await LoadAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task LoadAsync()
        {
            try
            {
                var from = From.Date;
                var to = To.Date.AddDays(1).AddSeconds(-1);
                var snapshot = await _statisticsService.GetSnapshotAsync(from, to, SelectedGender, SelectedReferral?.ReferralId);

                VisitCount = snapshot.Summary.VisitCount;
                PatientCount = snapshot.Summary.PatientCount;
                TestCount = snapshot.Summary.TestCount;
                TotalRevenue = snapshot.Summary.TotalRevenue;
                TotalPaid = snapshot.Summary.TotalPaid;

                ByGender.Clear();
                foreach (var row in snapshot.ByGender)
                {
                    ByGender.Add(row);
                }

                ByReferral.Clear();
                foreach (var row in snapshot.ByReferral)
                {
                    ByReferral.Add(row);
                }

                var monthly = await _statisticsService.GetMonthlyAnalysisAsync(From.Year);
                MonthlyAnalysis.Clear();
                foreach (var row in monthly)
                {
                    MonthlyAnalysis.Add(row);
                }

                var topTests = await _statisticsService.GetTop10TestsAsync(from, to);
                TopTests.Clear();
                foreach (var row in topTests)
                {
                    TopTests.Add(row);
                }

                var yearly = await _statisticsService.GetSampleCountPerYearAsync(5);
                YearlySamples.Clear();
                foreach (var row in yearly)
                {
                    YearlySamples.Add(row);
                }

                var productivity = await _userProductivityService.GetUserPerformanceAsync(from, to);
                UserProductivity.Clear();
                foreach (var row in productivity)
                {
                    UserProductivity.Add(row);
                }

                StatusMessage = "تم تحميل الإحصائيات.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }

        private async Task PrintAsync()
        {
            try
            {
                var referralLabel = SelectedReferral?.Name ?? "الكل";
                var lines = new ObservableCollection<string>
                {
                    $"الفترة: {From:yyyy-MM-dd} إلى {To:yyyy-MM-dd}",
                    $"الجنس: {SelectedGender}",
                    $"الجهة: {referralLabel}",
                    $"عدد الزيارات: {VisitCount}",
                    $"عدد المرضى: {PatientCount}",
                    $"عدد التحاليل: {TestCount}",
                    $"إجمالي الفواتير: {TotalRevenue:N2}",
                    $"إجمالي المدفوع: {TotalPaid:N2}",
                    string.Empty,
                    "تفصيل حسب الجنس:"
                };

                foreach (var row in ByGender)
                {
                    lines.Add($"- {row.Gender}: زيارات {row.VisitsCount}, تحاليل {row.TestsCount}, إيراد {row.Revenue:N2}");
                }

                lines.Add(string.Empty);
                lines.Add("تفصيل حسب الجهة:");
                foreach (var row in ByReferral)
                {
                    lines.Add($"- {row.ReferralName}: زيارات {row.VisitsCount}, تحاليل {row.TestsCount}, إيراد {row.Revenue:N2}, مدفوع {row.Paid:N2}");
                }

                await _printService.PrintTextReportAsync("تقرير الإحصائيات", lines, "StatisticsReport");
                StatusMessage = "تم إرسال التقرير للطباعة.";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ: " + ex.Message;
            }
        }
    }
}
