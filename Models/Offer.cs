using System.ComponentModel.DataAnnotations;

namespace Evimsensin.Models;

public enum OfferStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}

public class Offer
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public int FromUserId { get; set; }
    public int ToOwnerUserId { get; set; }

    [Range(1000, 1000000)]
    public decimal Amount { get; set; }

    [StringLength(600)]
    public string Note { get; set; } = string.Empty;

    public OfferStatus Status { get; set; } = OfferStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
