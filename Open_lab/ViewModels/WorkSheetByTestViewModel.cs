using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class WorkSheetByTestViewModel : BaseViewModel
    {
        private readonly IWorksheetService _worksheetService;
        private DateTime _from = DateTime.Today;
        private DateTime _to = DateTime.Today;
        private string _statusMessage = string.Empty;

        public WorkSheetByTestViewModel(IWorksheetService worksheetService)
        {
            _worksheetService = worksheetService;
            Rows = new ObservableCollection<WorkSheetTestRow>();
            LoadCommand = new RelayCommand(async _ => await LoadAsync());
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

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ObservableCollection<WorkSheetTestRow> Rows { get; }

        public ICommand LoadCommand { get; }

        private async Task LoadAsync()
        {
            try
            {
                var from = From.Date;
                var to = To.Date.AddDays(1).AddSeconds(-1);
                var rows = await _worksheetService.GetWorksheetByTestAsync(from, to);

                Rows.Clear();
                foreach (var row in rows)
                {
                    Rows.Add(row);
                }

                StatusMessage = $"تم تحميل {Rows.Count} تحليل.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
