using Evimsensin.Models;

namespace Evimsensin.Data;

public class AppDataStore
{
    public List<User> Users { get; } = [];
    public List<Listing> Listings { get; } = [];
    public List<Comment> Comments { get; } = [];
    public List<Rental> Rentals { get; } = [];
    public List<Message> Messages { get; } = [];
    public List<Offer> Offers { get; } = [];

    public int UserSeq = 1;
    public int ListingSeq = 1;
    public int CommentSeq = 1;
    public int RentalSeq = 1;
    public int MessageSeq = 1;
    public int OfferSeq = 1;
}
