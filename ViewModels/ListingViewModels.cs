using System.ComponentModel.DataAnnotations;
using Evimsensin.Models;
using Microsoft.AspNetCore.Http;

namespace Evimsensin.ViewModels;

public class ListingDetailsViewModel
{
    public Listing Listing { get; set; } = new();
    public List<Comment> Comments { get; set; } = [];
    public List<OfferDisplayViewModel> Offers { get; set; } = [];

    public bool CanEdit { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsLoggedIn { get; set; }
    public bool CanRent { get; set; }
    public bool CanOffer { get; set; }
}

public class ListingEditViewModel
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Province { get; set; } = string.Empty;

    [Required]
    public string District { get; set; } = string.Empty;

    [Required]
    public string PropertyType { get; set; } = "Daire";

    [Required]
    public string RoomCount { get; set; } = "2+1";

    [Range(10, 5000)]
    public int GrossSquareMeters { get; set; }

    [Range(10, 5000)]
    public int NetSquareMeters { get; set; }

    [Range(0, 80)]
    public int BuildingAge { get; set; }

    [Range(0, 100)]
    public int Floor { get; set; }

    [Range(1, 150)]
    public int TotalFloors { get; set; }

    [Range(1, 20)]
    public int BathroomCount { get; set; }

    [Required]
    public string HeatingType { get; set; } = "Kombi Dogalgaz";

    public bool Furnished { get; set; }
    public bool Balcony { get; set; }
    public bool Elevator { get; set; }
    public bool Parking { get; set; }
    public bool InSite { get; set; }
    public bool HasPool { get; set; }

    [Range(1000, 1000000)]
    public decimal MonthlyPrice { get; set; }

    [Range(0, 1000000)]
    public decimal Deposit { get; set; }

    [Range(0, 100000)]
    public decimal Dues { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
    public IFormFile? ImageFile { get; set; }

    public bool IsAdminRecommended { get; set; }
}

public class OfferCreateViewModel
{
    public int ListingId { get; set; }

    [Range(1000, 1000000)]
    public decimal Amount { get; set; }

    [StringLength(600)]
    public string Note { get; set; } = string.Empty;
}

public class OfferDisplayViewModel
{
    public int OfferId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;
    public string FromUserName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Note { get; set; } = string.Empty;
    public OfferStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PaymentViewModel
{
    public int ListingId { get; set; }
    public string ListingTitle { get; set; } = string.Empty;

    [Required, StringLength(16, MinimumLength = 12)]
    public string CardNumber { get; set; } = string.Empty;

    [Required, StringLength(5)]
    public string Expiry { get; set; } = string.Empty;

    [Required, StringLength(4, MinimumLength = 3)]
    public string Cvv { get; set; } = string.Empty;
}
