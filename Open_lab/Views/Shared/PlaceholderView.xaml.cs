using System.Windows.Controls;
using System.Windows.Input;

namespace Open_lab.Views.Shared
{
    public partial class PlaceholderView : UserControl
    {
        public PlaceholderView()
            : this("وظيفة مؤقتة", null)
        {
        }

        public PlaceholderView(string functionTitle, ICommand? backCommand)
        {
            InitializeComponent();
            FunctionTitle = functionTitle;
            BackCommand = backCommand;
            DataContext = this;
        }

        public string FunctionTitle { get; }

        public ICommand? BackCommand { get; }
    }
}
