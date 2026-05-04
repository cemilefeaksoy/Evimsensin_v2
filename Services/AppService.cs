using Evimsensin.Data;
using Evimsensin.Models;
using Evimsensin.ViewModels;

namespace Evimsensin.Services;

public class AppService
{
    private readonly AppDataStore _store;

    private static readonly Dictionary<string, List<string>> _locations = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Istanbul"] = ["Besiktas", "Kadikoy", "Sisli", "Uskudar", "Bakirkoy", "Beylikduzu"],
        ["Ankara"] = ["Cankaya", "Yenimahalle", "Kecioren", "Etimesgut", "Mamak"],
        ["Izmir"] = ["Karsiyaka", "Bornova", "Konak", "Buca", "Balcova"],
        ["Bursa"] = ["Nilufer", "Osmangazi", "Yildirim", "Mudanya"],
        ["Antalya"] = ["Muratpasa", "Konyaalti", "Lara", "Kepez", "Dosemealti"],
        ["Mugla"] = ["Bodrum", "Marmaris", "Fethiye", "Datca"],
        ["Eskisehir"] = ["Tepebasi", "Odunpazari"],
        ["Mersin"] = ["Mezitli", "Yenisehir", "Tarsus"],
        ["Trabzon"] = ["Ortahisar", "Yomra", "Akcaabat"],
        ["Samsun"] = ["Atakum", "Ilkadim", "Canik"],
        ["Kayseri"] = ["Melikgazi", "Kocasinan", "Talas"],
        ["Gaziantep"] = ["Sahinbey", "Sehitkamil"],
        ["Konya"] = ["Selcuklu", "Meram", "Karatay"],
        ["Balikesir"] = ["Ayvalik", "Edremit", "Bandirma"],
        ["Aydin"] = ["Kusadasi", "Didim", "Efeler"]
    };

    public AppService(AppDataStore store)
    {
        _store = store;
        Seed();
    }

    public IReadOnlyDictionary<string, List<string>> GetLocationMap() => _locations;

    private static string BuildCity(string province, string district)
        => string.IsNullOrWhiteSpace(district) ? province : $"{province} / {district}";

    private void Seed()
    {
        if (_store.Users.Count > 0)
        {
            return;
        }

        var admin = new User
        {
            Id = _store.UserSeq++,
            FullName = "Sistem Yonetici",
            Email = "admin@evimsensin.com",
            Password = "Admin123!",
            Role = UserRole.Admin
        };

        var sellerA = new User
        {
            Id = _store.UserSeq++,
            FullName = "Demo Satici",
            Email = "musteri@evimsensin.com",
            Password = "Musteri123!",
            Role = UserRole.Customer
        };

        var sellerB = new User
        {
            Id = _store.UserSeq++,
            FullName = "Satici Elif",
            Email = "elif@evimsensin.com",
            Password = "Satici123!",
            Role = UserRole.Customer
        };

        _store.Users.Add(admin);
        _store.Users.Add(sellerA);
        _store.Users.Add(sellerB);

        var provinces = _locations.Keys.ToList();
        var propertyTypes = new[] { "Daire", "Villa", "Rezidans", "Mustakil Ev", "Dublex" };
        var roomOptions = new[] { "1+1", "2+1", "3+1", "4+1" };
        var heatOptions = new[] { "Kombi Dogalgaz", "Merkezi", "Yerden Isitma", "Klima" };

        for (var i = 0; i < 15; i++)
        {
            var owner = i % 2 == 0 ? sellerA : sellerB;
            var province = provinces[i];
            var districts = _locations[province];
            var district = districts[i % districts.Count];
            var gross = 95 + (i * 11);
            var net = gross - 12;

            _store.Listings.Add(new Listing
            {
                Id = _store.ListingSeq++,
                Title = $"Seckin Portfoy Daire {i + 1}",
                Description = "Sehirin prestijli lokasyonunda, ulasim ve sosyal imkanlara yakin, premium yasam odakli konut.",
                Province = province,
                District = district,
                City = BuildCity(province, district),
                PropertyType = propertyTypes[i % propertyTypes.Length],
                RoomCount = roomOptions[i % roomOptions.Length],
                GrossSquareMeters = gross,
                NetSquareMeters = net,
                BuildingAge = i % 12,
                Floor = (i % 8) + 1,
                TotalFloors = 10 + (i % 7),
                BathroomCount = (i % 3) + 1,
                HeatingType = heatOptions[i % heatOptions.Length],
                Furnished = i % 2 == 0,
                Balcony = true,
                Elevator = true,
                Parking = i % 2 == 0,
                InSite = i % 3 != 0,
                HasPool = i % 5 == 0,
                MonthlyPrice = 18000 + (i * 1600),
                Deposit = 25000 + (i * 1000),
                Dues = 750 + (i * 45),
                ImageUrl = $"/img/seed-{(i % 12) + 1}.jpeg",
                OwnerUserId = owner.Id,
                OwnerName = owner.FullName,
                IsAdminRecommended = i % 4 == 0,
                IsRented = false,
                CreatedAt = DateTime.UtcNow.AddDays(-i)
            });
        }
    }

    public User? Login(string email, string password)
        => _store.Users.FirstOrDefault(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase) && x.Password == password);

    public User Register(string fullName, string email, string password, UserRole role)
    {
        if (_store.Users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Bu e-posta zaten kayitli.");
        }

        var user = new User
        {
            Id = _store.UserSeq++,
            FullName = fullName,
            Email = email,
            Password = password,
            Role = role
        };

        _store.Users.Add(user);
        return user;
    }

    public User? GetUser(int id) => _store.Users.FirstOrDefault(x => x.Id == id);

    public List<Listing> GetListings() => _store.Listings.OrderByDescending(x => x.CreatedAt).ToList();
    public Listing? GetListing(int id) => _store.Listings.FirstOrDefault(x => x.Id == id);

    public List<Listing> GetListingsByOwner(int ownerUserId)
        => _store.Listings.Where(x => x.OwnerUserId == ownerUserId).OrderByDescending(x => x.CreatedAt).ToList();

    public List<Comment> GetCommentsByListing(int listingId)
        => _store.Comments.Where(x => x.ListingId == listingId).OrderByDescending(x => x.CreatedAt).ToList();

    public Listing CreateListing(Listing listing)
    {
        listing.Id = _store.ListingSeq++;
        listing.City = BuildCity(listing.Province, listing.District);
        listing.CreatedAt = DateTime.UtcNow;
        _store.Listings.Add(listing);
        return listing;
    }

    public void UpdateListing(Listing listing)
    {
        var existing = GetListing(listing.Id) ?? throw new InvalidOperationException("Ilan bulunamadi.");
        existing.Title = listing.Title;
        existing.Description = listing.Description;
        existing.Province = listing.Province;
        existing.District = listing.District;
        existing.City = BuildCity(listing.Province, listing.District);
        existing.PropertyType = listing.PropertyType;
        existing.RoomCount = listing.RoomCount;
        existing.GrossSquareMeters = listing.GrossSquareMeters;
        existing.NetSquareMeters = listing.NetSquareMeters;
        existing.BuildingAge = listing.BuildingAge;
        existing.Floor = listing.Floor;
        existing.TotalFloors = listing.TotalFloors;
        existing.BathroomCount = listing.BathroomCount;
        existing.HeatingType = listing.HeatingType;
        existing.Furnished = listing.Furnished;
        existing.Balcony = listing.Balcony;
        existing.Elevator = listing.Elevator;
        existing.Parking = listing.Parking;
        existing.InSite = listing.InSite;
        existing.HasPool = listing.HasPool;
        existing.MonthlyPrice = listing.MonthlyPrice;
        existing.Deposit = listing.Deposit;
        existing.Dues = listing.Dues;
        existing.ImageUrl = listing.ImageUrl;
        existing.IsAdminRecommended = listing.IsAdminRecommended;
    }

    public void DeleteListing(int id)
    {
        var listing = GetListing(id);
        if (listing is null) return;

        _store.Listings.Remove(listing);
        _store.Comments.RemoveAll(c => c.ListingId == id);
        _store.Rentals.RemoveAll(r => r.ListingId == id);
        _store.Offers.RemoveAll(o => o.ListingId == id);
    }

    public Comment AddComment(int listingId, string author, string content)
    {
        var comment = new Comment
        {
            Id = _store.CommentSeq++,
            ListingId = listingId,
            AuthorName = author,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
        _store.Comments.Add(comment);
        return comment;
    }

    public void DeleteComment(int commentId)
    {
        var comment = _store.Comments.FirstOrDefault(c => c.Id == commentId);
        if (comment is not null) _store.Comments.Remove(comment);
    }

    public Rental Rent(int listingId, int renterId, string cardLast4)
    {
        var listing = GetListing(listingId) ?? throw new InvalidOperationException("Ilan bulunamadi.");

        if (listing.OwnerUserId == renterId)
        {
            throw new InvalidOperationException("Satici kendi ilanini kiralayamaz.");
        }

        if (listing.IsRented)
        {
            throw new InvalidOperationException("Bu ilan zaten kiralandi.");
        }

        var rental = new Rental
        {
            Id = _store.RentalSeq++,
            ListingId = listingId,
            RenterUserId = renterId,
            PaymentCardLast4 = cardLast4,
            RentedAt = DateTime.UtcNow
        };

        listing.IsRented = true;
        listing.RentedAt = rental.RentedAt;
        _store.Rentals.Add(rental);
        return rental;
    }

    public Offer CreateOffer(int listingId, int fromUserId, decimal amount, string note)
    {
        var listing = GetListing(listingId) ?? throw new InvalidOperationException("Ilan bulunamadi.");

        if (listing.OwnerUserId == fromUserId)
        {
            throw new InvalidOperationException("Kendi ilaniniza teklif veremezsiniz.");
        }

        if (listing.IsRented)
        {
            throw new InvalidOperationException("Kiralanmis ilana teklif verilemez.");
        }

        var offer = new Offer
        {
            Id = _store.OfferSeq++,
            ListingId = listingId,
            FromUserId = fromUserId,
            ToOwnerUserId = listing.OwnerUserId,
            Amount = amount,
            Note = note,
            Status = OfferStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _store.Offers.Add(offer);
        return offer;
    }

    public List<OfferDisplayViewModel> GetOffersForListing(int listingId)
    {
        var listing = GetListing(listingId);
        if (listing is null) return [];

        return _store.Offers
            .Where(o => o.ListingId == listingId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OfferDisplayViewModel
            {
                OfferId = o.Id,
                ListingTitle = listing.Title,
                FromUserName = GetUser(o.FromUserId)?.FullName ?? "Bilinmeyen",
                Amount = o.Amount,
                Note = o.Note,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            })
            .ToList();
    }

    public List<OfferDisplayViewModel> GetIncomingOffers(int ownerUserId)
    {
        return _store.Offers
            .Where(o => o.ToOwnerUserId == ownerUserId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OfferDisplayViewModel
            {
                OfferId = o.Id,
                ListingTitle = GetListing(o.ListingId)?.Title ?? "Ilan",
                FromUserName = GetUser(o.FromUserId)?.FullName ?? "Bilinmeyen",
                Amount = o.Amount,
                Note = o.Note,
                Status = o.Status,
                CreatedAt = o.CreatedAt
            })
            .ToList();
    }

    public void UpdateOfferStatus(int ownerUserId, int offerId, OfferStatus status)
    {
        var offer = _store.Offers.FirstOrDefault(o => o.Id == offerId);
        if (offer is null) throw new InvalidOperationException("Teklif bulunamadi.");
        if (offer.ToOwnerUserId != ownerUserId) throw new InvalidOperationException("Bu teklifi yonetme yetkiniz yok.");
        if (offer.Status != OfferStatus.Pending) return;

        offer.Status = status;
    }

    public SellerDashboardViewModel GetSellerDashboard(int ownerUserId)
    {
        var myListings = GetListingsByOwner(ownerUserId);
        var incomingOffers = GetIncomingOffers(ownerUserId);

        var total = myListings.Count;
        var rented = myListings.Count(x => x.IsRented);
        var pending = incomingOffers.Count(x => x.Status == OfferStatus.Pending);
        var rate = total == 0 ? 0 : Math.Round((decimal)rented / total * 100m, 1);

        return new SellerDashboardViewModel
        {
            TotalListings = total,
            RentedListings = rented,
            PendingOffers = pending,
            ConversionRate = rate,
            MyListings = myListings,
            IncomingOffers = incomingOffers
        };
    }

    public List<InboxConversationItemViewModel> GetInboxConversations(int currentUserId)
    {
        var messages = _store.Messages
            .Where(m => m.FromUserId == currentUserId || m.ToUserId == currentUserId)
            .ToList();

        var grouped = messages
            .GroupBy(m => m.FromUserId == currentUserId ? m.ToUserId : m.FromUserId)
            .Select(g =>
            {
                var partner = _store.Users.FirstOrDefault(u => u.Id == g.Key);
                if (partner is null)
                {
                    return null;
                }

                var last = g.OrderByDescending(x => x.CreatedAt).First();
                return new InboxConversationItemViewModel
                {
                    PartnerUserId = partner.Id,
                    PartnerName = partner.FullName,
                    PartnerRole = partner.Role,
                    LastMessage = last.Content,
                    LastMessageAt = last.CreatedAt,
                    LastFromMe = last.FromUserId == currentUserId,
                    UnreadCount = g.Count(x => x.ToUserId == currentUserId && !x.IsRead)
                };
            })
            .Where(x => x is not null)
            .Cast<InboxConversationItemViewModel>()
            .OrderByDescending(x => x.LastMessageAt)
            .ToList();

        return grouped;
    }

    public List<Message> GetConversation(int userA, int userB)
    {
        return _store.Messages
            .Where(m => (m.FromUserId == userA && m.ToUserId == userB) || (m.FromUserId == userB && m.ToUserId == userA))
            .OrderBy(m => m.CreatedAt)
            .ToList();
    }

    public int GetUnreadCount(int currentUserId)
        => _store.Messages.Count(m => m.ToUserId == currentUserId && !m.IsRead);

    public Message? GetLatestUnreadMessage(int currentUserId)
        => _store.Messages
            .Where(m => m.ToUserId == currentUserId && !m.IsRead)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefault();

    public void MarkConversationAsRead(int currentUserId, int withUserId)
    {
        foreach (var m in _store.Messages.Where(m => m.FromUserId == withUserId && m.ToUserId == currentUserId && !m.IsRead))
        {
            m.IsRead = true;
        }
    }

    public void SendMessage(int fromUserId, int toUserId, string content)
    {
        _store.Messages.Add(new Message
        {
            Id = _store.MessageSeq++,
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Content = content.Trim(),
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
    }
}
