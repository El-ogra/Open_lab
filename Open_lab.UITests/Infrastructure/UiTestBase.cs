using System;
using System.IO;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.UIA3;
using Open_lab.UITests.Infrastructure;

namespace Open_lab.UITests.Infrastructure;

public class UiTestBase : IDisposable
{
    private readonly TestDatabaseHelper _dbHelper;
    private FlaUI.Core.Application? _application;
    private UIA3Automation? _automation;

    public UiTestBase()
    {
        _dbHelper = new TestDatabaseHelper();
    }

    public async Task InitializeAsync()
    {
        // Initialize test database
        await _dbHelper.InitializeAsync();

        // Get the path to the executable
        var solutionDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", ".."));
        var exePath = Path.Combine(solutionDir, "Open_lab", "bin", "Debug", "net8.0-windows", "Open_lab.exe");

        if (!File.Exists(exePath))
        {
            throw new FileNotFoundException($"Application executable not found at: {exePath}");
        }

        // Initialize FlaUI automation
        _automation = new UIA3Automation();

        // Launch the application in a separate process
        _application = FlaUI.Core.Application.Launch(exePath);

        // Wait for the login window to appear (timeout: 10 seconds)
        var loginWindow = await WaitForMainWindowAsync(TimeSpan.FromSeconds(10));
        if (loginWindow == null)
        {
            throw new TimeoutException("Login window did not appear within 10 seconds");
        }
    }

    private async Task<Window?> WaitForMainWindowAsync(TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow.Add(timeout);
        
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                if (_application == null || _automation == null)
                {
                    await Task.Delay(100);
                    continue;
                }

                var mainWindow = _application.GetMainWindow(_automation);
                if (mainWindow != null)
                {
                    return mainWindow;
                }
            }
            catch
            {
                // Window might not be ready yet
            }

            await Task.Delay(100);
        }

        return null;
    }

    public FlaUI.Core.Application Application => _application ?? throw new InvalidOperationException("Application not initialized");
    public UIA3Automation Automation => _automation ?? throw new InvalidOperationException("Automation not initialized");
    public Window LoginWindow => Application.GetMainWindow(Automation) ?? throw new InvalidOperationException("Login window not found");

    public void Dispose()
    {
        try
        {
            try
            {
                if (_application != null)
                {
                    _application.Close();
                    _application.WaitWhileBusy(TimeSpan.FromSeconds(3));
                    if (!_application.HasExited)
                    {
                        _application.Kill();
                    }
                }
            }
            catch
            {
            }
        }
        finally
        {
            if (_application != null && !_application.HasExited)
            {
                try { _application.Kill(); } catch { }
            }
            _automation?.Dispose();
            _dbHelper.Dispose();
        }
    }
}
