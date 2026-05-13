using System;
using System.Windows.Input;
using Open_lab.ViewModels;

namespace Open_lab.ViewModels.Tools
{
    public class ToolsModuleViewModel : BaseViewModel
    {
        private readonly Action<string> _openPlaceholder;

        public ToolsModuleViewModel(Action<string> openPlaceholder)
        {
            _openPlaceholder = openPlaceholder ?? throw new ArgumentNullException(nameof(openPlaceholder));

            OpenTestLibraryCommand = new RelayCommand(_ => _openPlaceholder("مكتبة التحاليل"));
            OpenStopwatchCommand = new RelayCommand(_ => _openPlaceholder("ساعة التوقيت Stopwatch"));
            OpenRequirementsListCommand = new RelayCommand(_ => _openPlaceholder("قائمة المطلوبات والمشتريات"));
            OpenImageLibraryCommand = new RelayCommand(_ => _openPlaceholder("مكتبة الصور"));
            OpenUnitConverterCommand = new RelayCommand(_ => _openPlaceholder("محول وحدات نتائج التحاليل"));
            OpenAppointmentsCommand = new RelayCommand(_ => _openPlaceholder("نونة المواعيد"));
            OpenAbbreviationsDictionaryCommand = new RelayCommand(_ => _openPlaceholder("قاموس لاختصارات"));
            OpenCalculatorCommand = new RelayCommand(_ => _openPlaceholder("الآلة الحاسبة"));
            OpenPhoneDirectoryCommand = new RelayCommand(_ => _openPlaceholder("دليل الهاتف"));
        }

        public ICommand OpenTestLibraryCommand { get; }
        public ICommand OpenStopwatchCommand { get; }
        public ICommand OpenRequirementsListCommand { get; }
        public ICommand OpenImageLibraryCommand { get; }
        public ICommand OpenUnitConverterCommand { get; }
        public ICommand OpenAppointmentsCommand { get; }
        public ICommand OpenAbbreviationsDictionaryCommand { get; }
        public ICommand OpenCalculatorCommand { get; }
        public ICommand OpenPhoneDirectoryCommand { get; }
    }
}
