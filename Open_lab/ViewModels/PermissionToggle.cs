namespace Open_lab.ViewModels
{
    public class PermissionToggle : BaseViewModel
    {
        private bool _isGranted;

        public PermissionToggle(string code, bool granted)
        {
            Code = code;
            _isGranted = granted;
        }

        public string Code { get; }

        public bool IsGranted
        {
            get => _isGranted;
            set => SetProperty(ref _isGranted, value);
        }
    }
}
