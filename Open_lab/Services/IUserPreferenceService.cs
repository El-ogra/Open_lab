namespace Open_lab.Services
{
    public interface IUserPreferenceService
    {
        string? GetRememberedUsername();
        void SetRememberedUsername(string? username);
    }
}
