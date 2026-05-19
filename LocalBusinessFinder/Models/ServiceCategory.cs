namespace LocalBusinessFinder.Models;

public class ServiceCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "bi-shop";

    public ICollection<Business> Businesses { get; set; } = [];
}
