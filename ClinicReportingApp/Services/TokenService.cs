using ClinicReportingApp.Services.Interfaces;

namespace ClinicReportingApp.Services
{
    public class TokenService : ITokenService
    {
        private const string TokenKey = "jwt_token";
        private const string NameKey  = "user_name";
        private const string RoleKey  = "user_role";

        public string? GetToken(HttpContext context)
            => context.Session.GetString(TokenKey);

        public void StoreToken(HttpContext context, string token, string fullName, string role)
        {
            context.Session.SetString(TokenKey, token);
            context.Session.SetString(NameKey,  fullName);
            context.Session.SetString(RoleKey,  role);
        }

        public void ClearToken(HttpContext context)
        {
            context.Session.Remove(TokenKey);
            context.Session.Remove(NameKey);
            context.Session.Remove(RoleKey);
        }

        public bool IsAuthenticated(HttpContext context)
            => !string.IsNullOrEmpty(context.Session.GetString(TokenKey));

        public static string? GetUserName(HttpContext context)
            => context.Session.GetString(NameKey);

        public static string? GetUserRole(HttpContext context)
            => context.Session.GetString(RoleKey);
    }
}
