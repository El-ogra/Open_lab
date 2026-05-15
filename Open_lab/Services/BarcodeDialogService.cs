using System.Windows;
using Open_lab.ViewModels;
using Open_lab.Views.Patients;

namespace Open_lab.Services
{
    public class BarcodeDialogService : IBarcodeDialogService
    {
        private readonly IBarcodeService _barcodeService;
        private readonly IPrintService? _printService;

        public BarcodeDialogService(IBarcodeService barcodeService, IPrintService? printService = null)
        {
            _barcodeService = barcodeService;
            _printService = printService;
        }

        public void ShowBarcodeDialog(BarcodeDialogData data)
        {
            var viewModel = new BarcodeDialogViewModel(_barcodeService, _printService, data);
            var dialog = new BarcodeDialog
            {
                DataContext = viewModel,
                Owner = Application.Current?.MainWindow
            };

            dialog.ShowDialog();
        }
    }
}
