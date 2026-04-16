using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class TestCommentsViewModel : BaseViewModel
    {
        private readonly ITestCatalogService _testCatalogService;
        private Test? _selectedTest;
        private TestComment? _selectedComment;
        private string _commentText = string.Empty;
        private bool _isDefault;
        private string _statusMessage = string.Empty;

        public TestCommentsViewModel(ITestCatalogService testCatalogService)
        {
            _testCatalogService = testCatalogService;
            Tests = new ObservableCollection<Test>();
            Comments = new ObservableCollection<TestComment>();

            LoadCommand = new RelayCommand(async _ => await LoadCommentsAsync());
            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedComment != null);

            _ = LoadTestsAsync();
        }

        public ObservableCollection<Test> Tests { get; }
        public ObservableCollection<TestComment> Comments { get; }

        public Test? SelectedTest
        {
            get => _selectedTest;
            set
            {
                if (SetProperty(ref _selectedTest, value))
                {
                    _ = LoadCommentsAsync();
                }
            }
        }

        public TestComment? SelectedComment
        {
            get => _selectedComment;
            set
            {
                if (SetProperty(ref _selectedComment, value))
                {
                    if (value != null)
                    {
                        CommentText = value.CommentText;
                        IsDefault = value.IsDefault;
                    }
                    (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string CommentText
        {
            get => _commentText;
            set => SetProperty(ref _commentText, value);
        }

        public bool IsDefault
        {
            get => _isDefault;
            set => SetProperty(ref _isDefault, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }

        private async Task LoadTestsAsync()
        {
            var tests = await _testCatalogService.GetAllTestsAsync();
            Tests.Clear();
            foreach (var test in tests)
            {
                Tests.Add(test);
            }
        }

        private async Task LoadCommentsAsync()
        {
            Comments.Clear();
            if (SelectedTest == null)
            {
                return;
            }

            var items = await _testCatalogService.GetTestCommentsAsync(SelectedTest.TestId);
            foreach (var item in items)
            {
                Comments.Add(item);
            }
        }

        private async Task SaveAsync()
        {
            if (SelectedTest == null)
            {
                StatusMessage = "اختر تحليلًا.";
                return;
            }

            try
            {
                if (SelectedComment == null || SelectedComment.CommentId == 0)
                {
                    var comment = await _testCatalogService.CreateTestCommentAsync(new TestComment
                    {
                        TestId = SelectedTest.TestId,
                        CommentText = CommentText,
                        IsDefault = IsDefault
                    });

                    Comments.Add(comment);
                }
                else
                {
                    await _testCatalogService.UpdateTestCommentAsync(new TestComment
                    {
                        CommentId = SelectedComment.CommentId,
                        TestId = SelectedComment.TestId,
                        CommentText = CommentText,
                        IsDefault = IsDefault
                    });
                }

                StatusMessage = "تم حفظ التعليق.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }

        private async Task DeleteAsync()
        {
            if (SelectedComment == null)
            {
                return;
            }

            try
            {
                await _testCatalogService.DeleteTestCommentAsync(SelectedComment.CommentId);
                Comments.Remove(SelectedComment);
                SelectedComment = null;
                StatusMessage = "تم حذف التعليق.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"خطأ: {ex.Message}";
            }
        }
    }
}
