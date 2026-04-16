using System.Windows.Media;

namespace Open_lab.Services
{
    public interface IBarcodeService
    {
        ImageSource GenerateCode128(string content, int width = 360, int height = 96);
    }
}
