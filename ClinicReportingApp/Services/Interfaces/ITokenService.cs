namespace ClinicReportingApp.Services.Interfaces
{
    public interface ITokenService
    {
        string? GetToken(HttpContext context);
        void StoreToken(HttpContext context, string token, string fullName, string role);
        void ClearToken(HttpContext context);
        bool IsAuthenticated(HttpContext context);
    }
}
