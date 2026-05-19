using LocalBusinessFinder.Data;
using LocalBusinessFinder.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalBusinessFinder.Services;

public class BusinessService(ApplicationDbContext db)
{
    public Task<List<ServiceCategory>> GetCategoriesAsync() =>
        db.Categories.OrderBy(c => c.Name).ToListAsync();

    public Task<Business?> GetByIdAsync(int id) =>
        db.Businesses
            .Include(b => b.Category)
            .Include(b => b.Owner)
            .FirstOrDefaultAsync(b => b.Id == id);

    public Task<Business?> GetByOwnerIdAsync(string ownerId) =>
        db.Businesses
            .Include(b => b.Category)
            .FirstOrDefaultAsync(b => b.OwnerId == ownerId);

    public async Task<List<BusinessSearchResult>> SearchNearbyAsync(
        int? categoryId,
        double latitude,
        double longitude,
        double radiusKm = 500)
    {
        var query = db.Businesses
            .Include(b => b.Category)
            .Where(b => b.IsApproved && b.IsOnline);

        if (categoryId.HasValue)
            query = query.Where(b => b.CategoryId == categoryId);

        var businesses = await query.ToListAsync();

        return businesses
            .Select(b => new BusinessSearchResult
            {
                Business = b,
                DistanceKm = GeoHelper.DistanceKm(latitude, longitude, b.Latitude, b.Longitude)
            })
            .Where(x => x.DistanceKm <= radiusKm)
            .OrderBy(x => x.DistanceKm)
            .ToList();
    }

    public async Task<Business> RegisterAsync(Business business)
    {
        db.Businesses.Add(business);
        await db.SaveChangesAsync();
        return business;
    }

    public async Task UpdateAsync(Business business)
    {
        db.Businesses.Update(business);
        await db.SaveChangesAsync();
    }

    public Task<List<Business>> GetPendingApprovalAsync() =>
        db.Businesses.Include(b => b.Category).Include(b => b.Owner)
            .Where(b => !b.IsApproved).OrderByDescending(b => b.RegisteredAt).ToListAsync();

    public Task<List<Business>> GetAllAsync() =>
        db.Businesses.Include(b => b.Category).Include(b => b.Owner)
            .OrderByDescending(b => b.RegisteredAt).ToListAsync();
}

public class BusinessSearchResult
{
    public Business Business { get; set; } = null!;
    public double DistanceKm { get; set; }
}
