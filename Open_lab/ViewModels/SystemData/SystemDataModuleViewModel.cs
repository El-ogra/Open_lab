using System;
using System.Windows.Input;
using Open_lab.ViewModels;

namespace Open_lab.ViewModels.SystemData
{
    public class SystemDataModuleViewModel : BaseViewModel
    {
        private readonly Action<string> _openPlaceholder;

        public SystemDataModuleViewModel(Action<string> openPlaceholder)
        {
            _openPlaceholder = openPlaceholder ?? throw new ArgumentNullException(nameof(openPlaceholder));

            OpenTestDataCommand = Create("بيانات التحاليل");
            OpenBarcodeTypesCommand = Create("Barcode Types");
            OpenCultureAntibioticsCommand = Create("Culture Antibiotics");
            OpenTestGroupsCommand = Create("مجموعات التحاليل");
            OpenTestUnitsCommand = Create("Test Units");
            OpenTestCommentsCommand = Create("Test Comments");
            OpenLabBranchesCommand = Create("Lab. branches");
            OpenPatientTitlesCommand = Create("القاب وتعريفات المرضى");
            OpenCustomGroupsCommand = Create("Custom Groups");
            OpenWorkGroupsLogCommand = Create("مجموعات العمل (Log)");
            OpenPrintPriceListCommand = Create("طباعة قائمة اسعار التحاليل");
            OpenLabEquipmentCommand = Create("أجهزة ومعدات المعمل");
            OpenExternalEntitiesCommand = Create("الجهات الخارجية والمعدل");
            OpenExternalPriceListsCommand = Create("قائمة أسعار التحاليل للجهات");
        }

        public ICommand OpenTestDataCommand { get; }
        public ICommand OpenBarcodeTypesCommand { get; }
        public ICommand OpenCultureAntibioticsCommand { get; }
        public ICommand OpenTestGroupsCommand { get; }
        public ICommand OpenTestUnitsCommand { get; }
        public ICommand OpenTestCommentsCommand { get; }
        public ICommand OpenLabBranchesCommand { get; }
        public ICommand OpenPatientTitlesCommand { get; }
        public ICommand OpenCustomGroupsCommand { get; }
        public ICommand OpenWorkGroupsLogCommand { get; }
        public ICommand OpenPrintPriceListCommand { get; }
        public ICommand OpenLabEquipmentCommand { get; }
        public ICommand OpenExternalEntitiesCommand { get; }
        public ICommand OpenExternalPriceListsCommand { get; }

        private ICommand Create(string title)
        {
            return new RelayCommand(_ => _openPlaceholder(title));
        }
    }
}
