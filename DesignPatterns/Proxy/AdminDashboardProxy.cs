using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace WebBanNuocMVC.DesignPatterns.Proxy
{
    public class AdminDashboardProxy : IAdminDashboardSubject
    {
        private readonly RealAdminDashboardSubject _realSubject;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminDashboardProxy(
            RealAdminDashboardSubject realSubject,
            IHttpContextAccessor httpContextAccessor)
        {
            _realSubject = realSubject;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AdminDashboardAccessResult> GetDashboardAsync()
        {
            var role = GetCurrentRole();

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                return new AdminDashboardAccessResult
                {
                    HasAccess = false,
                    RoleLabel = string.IsNullOrWhiteSpace(role) ? "Guest" : role,
                    Message = "Access Denied"
                };
            }

            var result = await _realSubject.GetDashboardAsync();
            result.RoleLabel = role;
            return result;
        }

        private string GetCurrentRole()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null) return string.Empty;

            var user = httpContext.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                if (user.IsInRole("Admin"))
                    return "Admin";

                var claimRole = user.FindFirst(ClaimTypes.Role)?.Value
                             ?? user.FindFirst("role")?.Value;

                if (!string.IsNullOrWhiteSpace(claimRole))
                    return claimRole;
            }

            var sessionRole = httpContext.Session.GetString("Role")
                           ?? httpContext.Session.GetString("UserRole")
                           ?? httpContext.Session.GetString("AccountRole");

            return sessionRole ?? string.Empty;
        }
    }
}