using System;
using System.IO;
using System.Text.Json;

namespace Open_lab.Services
{
    public class UserPreferenceService : IUserPreferenceService
    {
        private sealed class LoginPreference
        {
            public string? Username { get; set; }
        }

        private static string PreferencePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Open_lab",
            "login-preferences.json");

        public string? GetRememberedUsername()
        {
            try
            {
                if (!File.Exists(PreferencePath))
                {
                    return null;
                }

                var json = File.ReadAllText(PreferencePath);
                var pref = JsonSerializer.Deserialize<LoginPreference>(json);
                return string.IsNullOrWhiteSpace(pref?.Username) ? null : pref.Username;
            }
            catch
            {
                return null;
            }
        }

        public void SetRememberedUsername(string? username)
        {
            try
            {
                var directory = Path.GetDirectoryName(PreferencePath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (string.IsNullOrWhiteSpace(username))
                {
                    if (File.Exists(PreferencePath))
                    {
                        File.Delete(PreferencePath);
                    }

                    return;
                }

                var payload = JsonSerializer.Serialize(new LoginPreference { Username = username.Trim() });
                File.WriteAllText(PreferencePath, payload);
            }
            catch
            {
            }
        }
    }
}
