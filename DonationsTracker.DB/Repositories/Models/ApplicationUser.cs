using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace DonationsTracker.DB;

public class ApplicationUser : IdentityUser
{

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
}
