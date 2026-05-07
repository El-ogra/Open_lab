using FluentAssertions;
using Open_lab.ViewModels;

namespace Open_lab.Tests.Services
{
    public class RelayCommandTests
    {
        [Fact]
        public void Constructor_With_Null_Execute_Should_Throw_ArgumentNullException()
        {
            // Function: X.X — To Be Determined
            // Arrange
            Action act = () => new RelayCommand(null!);

            // Act & Assert
            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void CanExecute_When_No_CanExecute_Func_Should_Return_True()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var command = new RelayCommand(_ => { });

            // Act
            var canExecute = command.CanExecute(null);

            // Assert
            canExecute.Should().BeTrue();
        }

        [Fact]
        public void CanExecute_When_CanExecute_Returns_True_Should_Return_True()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var command = new RelayCommand(_ => { }, _ => true);

            // Act
            var canExecute = command.CanExecute(null);

            // Assert
            canExecute.Should().BeTrue();
        }

        [Fact]
        public void CanExecute_When_CanExecute_Returns_False_Should_Return_False()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var command = new RelayCommand(_ => { }, _ => false);

            // Act
            var canExecute = command.CanExecute(null);

            // Assert
            canExecute.Should().BeFalse();
        }

        [Fact]
        public void Execute_Should_Invoke_Execute_Action()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var executed = false;
            var command = new RelayCommand(_ => executed = true);

            // Act
            command.Execute(null);

            // Assert
            executed.Should().BeTrue();
        }

        [Fact]
        public void Execute_Should_Pass_Parameter_To_Execute_Action()
        {
            // Function: X.X — To Be Determined
            // Arrange
            object? receivedParameter = null;
            var command = new RelayCommand(param => receivedParameter = param);

            // Act
            command.Execute("testValue");

            // Assert
            receivedParameter.Should().Be("testValue");
        }

        [Fact]
        public void RaiseCanExecuteChanged_Should_Trigger_CanExecuteChanged_Event()
        {
            // Function: X.X — To Be Determined
            // Arrange
            var command = new RelayCommand(_ => { }, _ => true);
            var eventRaised = false;
            command.CanExecuteChanged += (_, _) => eventRaised = true;

            // Act
            command.RaiseCanExecuteChanged();

            // Assert
            eventRaised.Should().BeTrue();
        }
    }
}
