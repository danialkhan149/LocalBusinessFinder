using LocalBusinessFinder.Data;
using LocalBusinessFinder.Models;
using Microsoft.EntityFrameworkCore;

namespace LocalBusinessFinder.Services;

public class RequestService(ApplicationDbContext db)
{
    public Task<ServiceRequest?> GetByIdAsync(int id) =>
        db.ServiceRequests
            .Include(r => r.Business).ThenInclude(b => b!.Category)
            .Include(r => r.Business).ThenInclude(b => b!.Owner)
            .Include(r => r.User)
            .Include(r => r.Messages).ThenInclude(m => m.Sender)
            .Include(r => r.Review)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<bool> CanUserAccessAsync(int requestId, string userId, bool isAdmin)
    {
        if (isAdmin) return true;

        var req = await db.ServiceRequests
            .Include(r => r.Business)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (req == null) return false;
        return req.UserId == userId || req.Business?.OwnerId == userId;
    }

    public Task<List<ServiceRequest>> GetForUserAsync(string userId) =>
        db.ServiceRequests
            .Include(r => r.Business)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public Task<List<ServiceRequest>> GetForBusinessAsync(int businessId) =>
        db.ServiceRequests
            .Include(r => r.User)
            .Where(r => r.BusinessId == businessId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public Task<List<ServiceRequest>> GetAllAsync() =>
        db.ServiceRequests
            .Include(r => r.User)
            .Include(r => r.Business)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<ServiceRequest> CreateAsync(ServiceRequest request)
    {
        db.ServiceRequests.Add(request);
        await db.SaveChangesAsync();
        return request;
    }

    public async Task UpdateStatusAsync(int id, RequestStatus status)
    {
        var req = await db.ServiceRequests.FindAsync(id);
        if (req == null) return;
        req.Status = status;
        if (status == RequestStatus.Accepted) req.AcceptedAt = DateTime.UtcNow;
        if (status == RequestStatus.Completed) req.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task AcceptDealAsync(int id, decimal price, VisitType visitType)
    {
        var req = await db.ServiceRequests.FindAsync(id);
        if (req == null) return;
        req.AgreedPrice = price;
        req.VisitType = visitType;
        req.Status = RequestStatus.DealAgreed;
        await db.SaveChangesAsync();
    }

    public async Task StartTripAsync(int id)
    {
        var req = await db.ServiceRequests.FindAsync(id);
        if (req == null) return;
        req.Status = RequestStatus.InProgress;
        await db.SaveChangesAsync();
    }

    public async Task CompleteAsync(int id)
    {
        var req = await db.ServiceRequests.FindAsync(id);
        if (req == null) return;
        req.Status = RequestStatus.Completed;
        req.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task CancelAsync(int id)
    {
        var req = await db.ServiceRequests.FindAsync(id);
        if (req == null) return;
        req.Status = RequestStatus.Cancelled;
        await db.SaveChangesAsync();
    }

    public async Task<ChatMessage> SendMessageAsync(ChatMessage message)
    {
        db.ChatMessages.Add(message);
        var req = await db.ServiceRequests.FindAsync(message.ServiceRequestId);
        if (req != null && req.Status == RequestStatus.Pending)
            req.Status = RequestStatus.Negotiating;
        await db.SaveChangesAsync();
        return message;
    }

    public async Task SaveLocationPingAsync(int requestId, string userId, double lat, double lng)
    {
        db.LocationPings.Add(new LocationPing
        {
            ServiceRequestId = requestId,
            UserId = userId,
            Latitude = lat,
            Longitude = lng
        });
        var user = await db.Users.FindAsync(userId);
        if (user != null)
        {
            user.LastLatitude = lat;
            user.LastLongitude = lng;
        }
        await db.SaveChangesAsync();
    }

    public Task<List<LocationPing>> GetRecentPingsAsync(int requestId, int count = 50) =>
        db.LocationPings
            .Where(p => p.ServiceRequestId == requestId)
            .OrderByDescending(p => p.Timestamp)
            .Take(count)
            .ToListAsync();

    public async Task AddReviewAsync(Review review)
    {
        db.Reviews.Add(review);
        var business = await db.Businesses.FindAsync(review.BusinessId);
        if (business != null)
        {
            var reviews = await db.Reviews.Where(r => r.BusinessId == business.Id).ToListAsync();
            reviews.Add(review);
            business.ReviewCount = reviews.Count;
            business.Rating = reviews.Average(r => r.Rating);
        }
        await db.SaveChangesAsync();
    }
}
