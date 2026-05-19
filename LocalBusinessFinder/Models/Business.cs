using LocalBusinessFinder.Data;

namespace LocalBusinessFinder.Models;

public class Business
{
    public int Id { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser Owner { get; set; } = null!;
    public int CategoryId { get; set; }
    public ServiceCategory Category { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public decimal HourlyRate { get; set; }
    public bool IsApproved { get; set; }
    public bool IsOnline { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public ICollection<ServiceRequest> Requests { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}
