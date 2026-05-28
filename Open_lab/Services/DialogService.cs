using System.Windows;

namespace Open_lab.Services;

public class DialogService : IDialogService
{
    public void ShowError(string message, string title = "خطأ")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
