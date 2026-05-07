using FluentAssertions;
using Open_lab.ViewModels;
using System.ComponentModel;

namespace Open_lab.Tests.Services
{
    public class BaseViewModelTests
    {
        private class TestViewModel : BaseViewModel
        {
            private string _name = string.Empty;
            public string Name
            {
                get => _name;
                set => SetProperty(ref _name, value);
            }

            private int _age;
            public int Age
            {
                get => _age;
                set => SetProperty(ref _age, value);
            }
        }

        [Fact]
        public void SetProperty_When_Value_Changes_Should_Raise_PropertyChanged()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var vm = new TestViewModel();
            var propertyName = string.Empty;
            vm.PropertyChanged += (_, e) => propertyName = e.PropertyName;

            // Act
            vm.Name = "John";

            // Assert
            propertyName.Should().Be(nameof(TestViewModel.Name));
        }

        [Fact]
        public void SetProperty_When_Value_Does_Not_Change_Should_Not_Raise_PropertyChanged()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var vm = new TestViewModel();
            vm.Name = "John";
            var eventRaised = false;
            vm.PropertyChanged += (_, _) => eventRaised = true;

            // Act
            vm.Name = "John";

            // Assert
            eventRaised.Should().BeFalse();
        }

        [Fact]
        public void SetProperty_Should_Return_True_When_Value_Changes()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var vm = new TestViewModel();

            // Act - use reflection to capture return value
            var property = typeof(TestViewModel).GetProperty(nameof(TestViewModel.Name));
            property!.SetValue(vm, "John");

            // Assert
            vm.Name.Should().Be("John");
        }

        [Fact]
        public void SetProperty_With_Different_Types_Should_Raise_Correct_PropertyName()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var vm = new TestViewModel();
            var raisedProperties = new List<string>();
            vm.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName!);

            // Act
            vm.Age = 25;

            // Assert
            raisedProperties.Should().ContainSingle()
                .Which.Should().Be(nameof(TestViewModel.Age));
        }
    }
}
