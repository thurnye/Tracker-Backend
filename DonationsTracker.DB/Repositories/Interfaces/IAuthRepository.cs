using DonationsTracker.DB;
using System.Threading.Tasks;

namespace DonationsTracker.DB.Repositories
{
   public interface IAuthRepository
{
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string password);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
}

}
