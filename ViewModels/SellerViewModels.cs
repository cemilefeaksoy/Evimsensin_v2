using Evimsensin.Models;

namespace Evimsensin.ViewModels;

public class SellerDashboardViewModel
{
    public int TotalListings { get; set; }
    public int RentedListings { get; set; }
    public int PendingOffers { get; set; }
    public decimal ConversionRate { get; set; }

    public List<Listing> MyListings { get; set; } = [];
    public List<OfferDisplayViewModel> IncomingOffers { get; set; } = [];
}
