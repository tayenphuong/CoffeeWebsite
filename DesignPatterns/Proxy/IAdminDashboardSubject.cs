using System.Threading.Tasks;

namespace WebBanNuocMVC.DesignPatterns.Proxy
{
    public interface IAdminDashboardSubject
    {
        Task<AdminDashboardAccessResult> GetDashboardAsync();
    }
}