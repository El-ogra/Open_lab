using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Open_lab.Data;
using Open_lab.Models;
using Open_lab.Services;

namespace Open_lab.ViewModels
{
    public class TestCommentsViewModel : BaseViewModel
    {
        private readonly Func<OpenLabDbContext> _dbFactory;
        private Test? _selectedTest;
        private TestComment? _selectedComment;
        private string _commentText = string.Empty;
        private bool _isDefault;
        private string _statusMessage = string.Empty;

        public TestCommentsViewModel(Func<OpenLabDbContext> dbFactory)
        {
            _dbFactory = dbFactory;
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
            using var db = _dbFactory();
            var service = new TestCatalogService(db);
            var tests = await service.GetAllTestsAsync();
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

            using var db = _dbFactory();
            var items = await db.TestComments.AsNoTracking()
                .Where(c => c.TestId == SelectedTest.TestId)
                .ToListAsync();

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
                using var db = _dbFactory();

                if (SelectedComment == null || SelectedComment.CommentId == 0)
                {
                    var comment = new TestComment
                    {
                        TestId = SelectedTest.TestId,
                        CommentText = CommentText,
                        IsDefault = IsDefault
                    };

                    db.TestComments.Add(comment);
                    await db.SaveChangesAsync();
                    Comments.Add(comment);
                }
                else
                {
                    var comment = await db.TestComments.FirstAsync(c => c.CommentId == SelectedComment.CommentId);
                    comment.CommentText = CommentText;
                    comment.IsDefault = IsDefault;
                    await db.SaveChangesAsync();
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
                using var db = _dbFactory();
                var comment = await db.TestComments.FirstAsync(c => c.CommentId == SelectedComment.CommentId);
                db.TestComments.Remove(comment);
                await db.SaveChangesAsync();
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
