using LocalBusinessFinder.Models;
using Microsoft.AspNetCore.Identity;

namespace LocalBusinessFinder.Data;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public double? LastLatitude { get; set; }
    public double? LastLongitude { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public Business? OwnedBusiness { get; set; }
    public ICollection<ServiceRequest> ServiceRequests { get; set; } = [];
}
