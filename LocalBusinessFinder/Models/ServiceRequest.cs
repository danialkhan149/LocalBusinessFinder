using LocalBusinessFinder.Data;

namespace LocalBusinessFinder.Models;

public enum RequestStatus
{
    Pending,
    Accepted,
    Negotiating,
    DealAgreed,
    InProgress,
    Completed,
    Cancelled
}

public enum VisitType
{
    UserGoesToBusiness,
    BusinessComesToUser
}

public class ServiceRequest
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public VisitType VisitType { get; set; }
    public decimal? AgreedPrice { get; set; }
    public string? UserOffer { get; set; }
    public string? BusinessOffer { get; set; }

    public double UserLatitude { get; set; }
    public double UserLongitude { get; set; }
    public string UserAddress { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<ChatMessage> Messages { get; set; } = [];
    public ICollection<LocationPing> LocationPings { get; set; } = [];
    public Review? Review { get; set; }
}
