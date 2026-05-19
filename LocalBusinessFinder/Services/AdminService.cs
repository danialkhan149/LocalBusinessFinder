using LocalBusinessFinder.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LocalBusinessFinder.Services;

public class AdminService(
    ApplicationDbContext db,
    UserManager<ApplicationUser> userManager)
{
    public async Task<DashboardStats> GetStatsAsync()
    {
        return new DashboardStats
        {
            TotalUsers = await userManager.Users.CountAsync(),
            TotalBusinesses = await db.Businesses.CountAsync(),
            PendingBusinesses = await db.Businesses.CountAsync(b => !b.IsApproved),
            ActiveRequests = await db.ServiceRequests.CountAsync(r =>
                r.Status == Models.RequestStatus.InProgress ||
                r.Status == Models.RequestStatus.DealAgreed),
            CompletedRequests = await db.ServiceRequests.CountAsync(r =>
                r.Status == Models.RequestStatus.Completed),
            TotalCategories = await db.Categories.CountAsync()
        };
    }

    public Task<List<ApplicationUser>> GetUsersAsync() =>
        userManager.Users.OrderByDescending(u => u.CreatedAt).ToListAsync();

    public async Task ApproveBusinessAsync(int businessId)
    {
        var b = await db.Businesses.FindAsync(businessId);
        if (b == null) return;
        b.IsApproved = true;
        await db.SaveChangesAsync();
    }

    public async Task RejectBusinessAsync(int businessId)
    {
        var b = await db.Businesses.FindAsync(businessId);
        if (b == null) return;
        db.Businesses.Remove(b);
        await db.SaveChangesAsync();
    }

    public async Task SetUserActiveAsync(string userId, bool active)
    {
        var u = await userManager.FindByIdAsync(userId);
        if (u == null) return;
        u.IsActive = active;
        await userManager.UpdateAsync(u);
    }
}

public class DashboardStats
{
    public int TotalUsers { get; set; }
    public int TotalBusinesses { get; set; }
    public int PendingBusinesses { get; set; }
    public int ActiveRequests { get; set; }
    public int CompletedRequests { get; set; }
    public int TotalCategories { get; set; }
}
