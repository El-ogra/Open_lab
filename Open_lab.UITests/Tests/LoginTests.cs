using System;
using System.Threading.Tasks;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Input;
using FluentAssertions;
using Open_lab.UITests.Infrastructure;
using Xunit;

namespace Open_lab.UITests.Tests;

public class LoginTests : IAsyncLifetime
{
    private UiTestBase? _uiTestBase;

    public async Task InitializeAsync()
    {
        _uiTestBase = new UiTestBase();
        await _uiTestBase.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        if (_uiTestBase != null)
        {
            _uiTestBase.Dispose();
        }
        await Task.CompletedTask;
    }

    [Fact]
    public async Task LoginWithValidCredentials_ShouldOpenMainWindow()
    {
        // Arrange
        _uiTestBase.Should().NotBeNull("_uiTestBase is null - InitializeAsync may have failed");
        var loginWindow = _uiTestBase!.LoginWindow;
        loginWindow.Should().NotBeNull("LoginWindow is null - app may not have started");
        var usernameTextBox = loginWindow.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTextBox")).AsTextBox();
        usernameTextBox.Should().NotBeNull("UsernameTextBox element is null");
        var passwordBox = loginWindow.FindFirstDescendant(cf => cf.ByAutomationId("PasswordBox")).AsTextBox();
        passwordBox.Should().NotBeNull("PasswordBox element is null");
        var loginButton = loginWindow.FindFirstDescendant(cf => cf.ByAutomationId("LoginButton")).AsButton();
        loginButton.Should().NotBeNull("LoginButton element is null");

        // Act
        usernameTextBox.Text = "testuser";
        passwordBox.Focus();
        FlaUI.Core.Input.Keyboard.Type("TestPassword123!");
        loginButton.Click();

        // Wait for login window to close or main window to appear
        await WaitUntilAsync(() => 
        {
            try
            {
                var mainWindow = _uiTestBase.Application.GetMainWindow(_uiTestBase.Automation);
                var loginWindowStillVisible = loginWindow.IsOffscreen == false;
                return mainWindow != null && mainWindow.Name.Contains("Open Lab System");
            }
            catch
            {
                return false;
            }
        }, "Main window did not appear within timeout", TimeSpan.FromSeconds(10));

        // Assert
        var mainWindow = _uiTestBase.Application.GetMainWindow(_uiTestBase.Automation);
        mainWindow.Should().NotBeNull();
        mainWindow!.Name.Should().Contain("Open Lab System");
    }

    [Fact]
    public async Task LoginWithValidUsernameInvalidPassword_ShouldShowErrorDialog()
    {
        // Arrange
        _uiTestBase.Should().NotBeNull("_uiTestBase is null - InitializeAsync may have failed");
        var loginWindow = _uiTestBase!.LoginWindow;
        loginWindow.Should().NotBeNull("LoginWindow is null - app may not have started");
        var usernameTextBox = loginWindow.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTextBox")).AsTextBox();
        usernameTextBox.Should().NotBeNull("UsernameTextBox element is null");
        var passwordBox = loginWindow.FindFirstDescendant(cf => cf.ByAutomationId("PasswordBox")).AsTextBox();
        passwordBox.Should().NotBeNull("PasswordBox element is null");
        var loginButton = loginWindow.FindFirstDescendant(cf => cf.ByAutomationId("LoginButton")).AsButton();
        loginButton.Should().NotBeNull("LoginButton element is null");

        // Act
        usernameTextBox.Text = "testuser";
        passwordBox.Focus();
        FlaUI.Core.Input.Keyboard.Type("WrongPassword");
        loginButton.Click();

        // Wait for error dialog to appear
        await WaitUntilAsync(() => 
        {
            try
            {
                var dialog = _uiTestBase.Automation.GetDesktop().FindFirstDescendant(cf => 
                    cf.ByControlType(ControlType.Window).And(cf.ByName("خطأ")));
                return dialog != null;
            }
            catch
            {
                return false;
            }
        }, "Error dialog did not appear within timeout", TimeSpan.FromSeconds(10));

        // Assert
        var dialog = _uiTestBase.Automation.GetDesktop().FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.Window).And(cf.ByName("خطأ")));
        dialog.Should().NotBeNull();

        var messageText = dialog!.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text)).AsTextBox();
        messageText.Text.Should().Contain("اسم المستخدم أو كلمة المرور غير صحيحة");

        // Verify dialog is on top of login window
        loginWindow.IsOffscreen.Should().BeFalse();

        // Close dialog
        var okButton = dialog.FindFirstDescendant(cf => cf.ByName("OK")).AsButton();
        if (okButton == null)
        {
            okButton = dialog.FindFirstDescendant(cf => cf.ByName("موافق")).AsButton();
        }
        okButton.Should().NotBeNull();
        okButton!.Click();
    }

    [Fact]
    public async Task LoginWithInvalidUsernameValidPassword_ShouldShowErrorDialog()
    {
        // Arrange
        _uiTestBase.Should().NotBeNull("_uiTestBase is null - InitializeAsync may have failed");
        var loginWindow = _uiTestBase!.LoginWindow;
        loginWindow.Should().NotBeNull("LoginWindow is null - app may not have started");
        var usernameTextBox = loginWindow.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTextBox")).AsTextBox();
        usernameTextBox.Should().NotBeNull("UsernameTextBox element is null");
        var passwordBox = loginWindow.FindFirstDescendant(cf => cf.ByAutomationId("PasswordBox")).AsTextBox();
        passwordBox.Should().NotBeNull("PasswordBox element is null");
        var loginButton = loginWindow.FindFirstDescendant(cf => cf.ByAutomationId("LoginButton")).AsButton();
        loginButton.Should().NotBeNull("LoginButton element is null");

        // Act
        usernameTextBox.Text = "wronguser";
        passwordBox.Focus();
        FlaUI.Core.Input.Keyboard.Type("TestPassword123!");
        loginButton.Click();

        // Wait for error dialog to appear
        await WaitUntilAsync(() => 
        {
            try
            {
                var dialog = _uiTestBase.Automation.GetDesktop().FindFirstDescendant(cf => 
                    cf.ByControlType(ControlType.Window).And(cf.ByName("خطأ")));
                return dialog != null;
            }
            catch
            {
                return false;
            }
        }, "Error dialog did not appear within timeout", TimeSpan.FromSeconds(10));

        // Assert
        var dialog = _uiTestBase.Automation.GetDesktop().FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.Window).And(cf.ByName("خطأ")));
        dialog.Should().NotBeNull();

        var messageText = dialog!.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text)).AsTextBox();
        messageText.Text.Should().Contain("اسم المستخدم أو كلمة المرور غير صحيحة");

        // Verify dialog is on top of login window
        loginWindow.IsOffscreen.Should().BeFalse();

        // Close dialog
        var okButton = dialog.FindFirstDescendant(cf => cf.ByName("OK")).AsButton();
        if (okButton == null)
        {
            okButton = dialog.FindFirstDescendant(cf => cf.ByName("موافق")).AsButton();
        }
        okButton.Should().NotBeNull();
        okButton!.Click();
    }

    [Fact]
    public async Task LoginWithInvalidCredentials_ShouldShowErrorDialog()
    {
        // Arrange
        _uiTestBase.Should().NotBeNull("_uiTestBase is null - InitializeAsync may have failed");
        var loginWindow = _uiTestBase!.LoginWindow;
        loginWindow.Should().NotBeNull("LoginWindow is null - app may not have started");
        var usernameTextBox = loginWindow.FindFirstDescendant(cf => cf.ByAutomationId("UsernameTextBox")).AsTextBox();
        usernameTextBox.Should().NotBeNull("UsernameTextBox element is null");
        var passwordBox = loginWindow.FindFirstDescendant(cf => cf.ByAutomationId("PasswordBox")).AsTextBox();
        passwordBox.Should().NotBeNull("PasswordBox element is null");
        var loginButton = loginWindow.FindFirstDescendant(cf => cf.ByAutomationId("LoginButton")).AsButton();
        loginButton.Should().NotBeNull("LoginButton element is null");

        // Act
        usernameTextBox.Text = "wronguser";
        passwordBox.Focus();
        FlaUI.Core.Input.Keyboard.Type("WrongPassword");
        loginButton.Click();

        // Wait for error dialog to appear
        await WaitUntilAsync(() => 
        {
            try
            {
                var dialog = _uiTestBase.Automation.GetDesktop().FindFirstDescendant(cf => 
                    cf.ByControlType(ControlType.Window).And(cf.ByName("خطأ")));
                return dialog != null;
            }
            catch
            {
                return false;
            }
        }, "Error dialog did not appear within timeout", TimeSpan.FromSeconds(10));

        // Assert
        var dialog = _uiTestBase.Automation.GetDesktop().FindFirstDescendant(cf => 
            cf.ByControlType(ControlType.Window).And(cf.ByName("خطأ")));
        dialog.Should().NotBeNull();

        var messageText = dialog!.FindFirstDescendant(cf => cf.ByControlType(ControlType.Text)).AsTextBox();
        messageText.Text.Should().Contain("اسم المستخدم أو كلمة المرور غير صحيحة");

        // Verify dialog is on top of login window
        loginWindow.IsOffscreen.Should().BeFalse();

        // Close dialog
        var okButton = dialog.FindFirstDescendant(cf => cf.ByName("OK")).AsButton();
        if (okButton == null)
        {
            okButton = dialog.FindFirstDescendant(cf => cf.ByName("موافق")).AsButton();
        }
        okButton.Should().NotBeNull();
        okButton!.Click();
    }

    private static async Task WaitUntilAsync(Func<bool> predicate, string failureMessage, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow.Add(timeout);
        while (DateTime.UtcNow < deadline)
        {
            if (predicate())
            {
                return;
            }

            await Task.Delay(100);
        }

        throw new TimeoutException(failureMessage);
    }
}
