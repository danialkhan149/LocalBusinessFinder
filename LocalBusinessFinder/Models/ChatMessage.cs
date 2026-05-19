using LocalBusinessFinder.Data;

namespace LocalBusinessFinder.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public int ServiceRequestId { get; set; }
    public ServiceRequest ServiceRequest { get; set; } = null!;
    public string SenderId { get; set; } = string.Empty;
    public ApplicationUser Sender { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public bool IsOffer { get; set; }
    public decimal? OfferAmount { get; set; }
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
