using System.Text.Json;
using Evimsensin.Models;
using Evimsensin.Services;
using Evimsensin.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Evimsensin.Controllers;

public class ListingsController : Controller
{
    private readonly AppService _appService;
    private readonly IWebHostEnvironment _environment;

    public ListingsController(AppService appService, IWebHostEnvironment environment)
    {
        _appService = appService;
        _environment = environment;
    }

    public IActionResult Index(string? city)
    {
        var listings = _appService.GetListings();
        if (!string.IsNullOrWhiteSpace(city))
        {
            listings = listings.Where(x => x.City.Contains(city, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return View(listings);
    }

    public IActionResult Details(int id)
    {
        var listing = _appService.GetListing(id);
        if (listing is null)
        {
            return NotFound();
        }

        var userId = AuthSession.UserId(this);
        var isAdmin = AuthSession.IsAdmin(this);
        var isLoggedIn = AuthSession.IsLoggedIn(this);
        var isOwner = userId.HasValue && userId.Value == listing.OwnerUserId;

        var vm = new ListingDetailsViewModel
        {
            Listing = listing,
            Comments = _appService.GetCommentsByListing(id),
            Offers = isOwner || isAdmin ? _appService.GetOffersForListing(id) : [],
            IsAdmin = isAdmin,
            IsLoggedIn = isLoggedIn,
            CanEdit = isAdmin || isOwner,
            CanRent = isLoggedIn && !isOwner && !listing.IsRented,
            CanOffer = isLoggedIn && !isOwner && !listing.IsRented
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!AuthSession.IsLoggedIn(this)) return RedirectToAction("Login", "Account");

        SetLocationViewData();
        ViewBag.IsAdmin = AuthSession.IsAdmin(this);

        return View(new ListingEditViewModel
        {
            Province = "Istanbul",
            District = "Besiktas",
            PropertyType = "Daire",
            RoomCount = "2+1",
            GrossSquareMeters = 120,
            NetSquareMeters = 95,
            BuildingAge = 3,
            Floor = 4,
            TotalFloors = 10,
            BathroomCount = 2,
            HeatingType = "Kombi Dogalgaz",
            MonthlyPrice = 30000,
            Deposit = 50000,
            Dues = 1800,
            Balcony = true,
            Elevator = true,
            Parking = true,
            InSite = true,
            ImageUrl = "/img/seed-1.jpeg"
        });
    }

    [HttpPost]
    public IActionResult Create(ListingEditViewModel model)
    {
        var userId = AuthSession.UserId(this);
        if (!userId.HasValue) return RedirectToAction("Login", "Account");

        SetLocationViewData();
        ViewBag.IsAdmin = AuthSession.IsAdmin(this);

        model.ImageUrl = model.ImageUrl?.Trim() ?? string.Empty;

        if (model.ImageFile is null && string.IsNullOrWhiteSpace(model.ImageUrl))
        {
            ModelState.AddModelError(nameof(model.ImageFile), "Resim yukleyin veya gecerli URL girin.");
        }

        string imagePath = model.ImageUrl;

        if (model.ImageFile is not null)
        {
            if (!TrySaveImage(model.ImageFile, out var savedPath, out var error))
            {
                ModelState.AddModelError(nameof(model.ImageFile), error ?? "Resim yuklenemedi.");
            }
            else
            {
                imagePath = savedPath!;
            }
        }

        if (!string.IsNullOrWhiteSpace(model.ImageUrl) && !IsValidImageUrl(model.ImageUrl))
        {
            ModelState.AddModelError(nameof(model.ImageUrl), "URL /img ile baslamali veya http/https olmalidir.");
        }

        if (!ModelState.IsValid) return View(model);

        _appService.CreateListing(new Listing
        {
            Title = model.Title,
            Description = model.Description,
            Province = model.Province,
            District = model.District,
            PropertyType = model.PropertyType,
            RoomCount = model.RoomCount,
            GrossSquareMeters = model.GrossSquareMeters,
            NetSquareMeters = model.NetSquareMeters,
            BuildingAge = model.BuildingAge,
            Floor = model.Floor,
            TotalFloors = model.TotalFloors,
            BathroomCount = model.BathroomCount,
            HeatingType = model.HeatingType,
            Furnished = model.Furnished,
            Balcony = model.Balcony,
            Elevator = model.Elevator,
            Parking = model.Parking,
            InSite = model.InSite,
            HasPool = model.HasPool,
            MonthlyPrice = model.MonthlyPrice,
            Deposit = model.Deposit,
            Dues = model.Dues,
            ImageUrl = imagePath,
            OwnerUserId = userId.Value,
            OwnerName = AuthSession.UserName(this) ?? "Musteri",
            IsAdminRecommended = AuthSession.IsAdmin(this) && model.IsAdminRecommended
        });

        TempData["Success"] = "Ilaniniz basariyla yayina alindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var listing = _appService.GetListing(id);
        if (listing is null) return NotFound();

        var userId = AuthSession.UserId(this);
        var isAdmin = AuthSession.IsAdmin(this);
        if (!(isAdmin || (userId.HasValue && userId.Value == listing.OwnerUserId))) return Forbid();

        SetLocationViewData();
        ViewBag.IsAdmin = isAdmin;

        return View(new ListingEditViewModel
        {
            Id = listing.Id,
            Title = listing.Title,
            Description = listing.Description,
            Province = listing.Province,
            District = listing.District,
            PropertyType = listing.PropertyType,
            RoomCount = listing.RoomCount,
            GrossSquareMeters = listing.GrossSquareMeters,
            NetSquareMeters = listing.NetSquareMeters,
            BuildingAge = listing.BuildingAge,
            Floor = listing.Floor,
            TotalFloors = listing.TotalFloors,
            BathroomCount = listing.BathroomCount,
            HeatingType = listing.HeatingType,
            Furnished = listing.Furnished,
            Balcony = listing.Balcony,
            Elevator = listing.Elevator,
            Parking = listing.Parking,
            InSite = listing.InSite,
            HasPool = listing.HasPool,
            MonthlyPrice = listing.MonthlyPrice,
            Deposit = listing.Deposit,
            Dues = listing.Dues,
            ImageUrl = listing.ImageUrl,
            IsAdminRecommended = listing.IsAdminRecommended
        });
    }

    [HttpPost]
    public IActionResult Edit(ListingEditViewModel model)
    {
        var listing = _appService.GetListing(model.Id);
        if (listing is null) return NotFound();

        var userId = AuthSession.UserId(this);
        var isAdmin = AuthSession.IsAdmin(this);
        if (!(isAdmin || (userId.HasValue && userId.Value == listing.OwnerUserId))) return Forbid();

        SetLocationViewData();
        ViewBag.IsAdmin = isAdmin;

        model.ImageUrl = model.ImageUrl?.Trim() ?? string.Empty;
        var hasFile = model.ImageFile is not null;
        var hasUrl = !string.IsNullOrWhiteSpace(model.ImageUrl);

        string imagePath = listing.ImageUrl;

        if (hasFile)
        {
            if (!TrySaveImage(model.ImageFile, out var savedPath, out var error))
            {
                ModelState.AddModelError(nameof(model.ImageFile), error ?? "Resim yuklenemedi.");
            }
            else
            {
                imagePath = savedPath!;
            }
        }
        else if (hasUrl)
        {
            if (!IsValidImageUrl(model.ImageUrl))
            {
                ModelState.AddModelError(nameof(model.ImageUrl), "URL /img ile baslamali veya http/https olmalidir.");
            }
            else
            {
                imagePath = model.ImageUrl;
            }
        }

        if (!ModelState.IsValid) return View(model);

        _appService.UpdateListing(new Listing
        {
            Id = model.Id,
            Title = model.Title,
            Description = model.Description,
            Province = model.Province,
            District = model.District,
            PropertyType = model.PropertyType,
            RoomCount = model.RoomCount,
            GrossSquareMeters = model.GrossSquareMeters,
            NetSquareMeters = model.NetSquareMeters,
            BuildingAge = model.BuildingAge,
            Floor = model.Floor,
            TotalFloors = model.TotalFloors,
            BathroomCount = model.BathroomCount,
            HeatingType = model.HeatingType,
            Furnished = model.Furnished,
            Balcony = model.Balcony,
            Elevator = model.Elevator,
            Parking = model.Parking,
            InSite = model.InSite,
            HasPool = model.HasPool,
            MonthlyPrice = model.MonthlyPrice,
            Deposit = model.Deposit,
            Dues = model.Dues,
            ImageUrl = imagePath,
            IsAdminRecommended = isAdmin && model.IsAdminRecommended
        });

        TempData["Success"] = "Ilan guncellendi.";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        var listing = _appService.GetListing(id);
        if (listing is null) return NotFound();

        var userId = AuthSession.UserId(this);
        if (!(AuthSession.IsAdmin(this) || (userId.HasValue && userId.Value == listing.OwnerUserId))) return Forbid();

        _appService.DeleteListing(id);
        TempData["Success"] = "Ilan silindi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult AddComment(int listingId, string content)
    {
        if (!AuthSession.IsLoggedIn(this)) return RedirectToAction("Login", "Account");
        if (!string.IsNullOrWhiteSpace(content))
        {
            _appService.AddComment(listingId, AuthSession.UserName(this) ?? "Musteri", content.Trim());
        }

        return RedirectToAction(nameof(Details), new { id = listingId });
    }

    [HttpPost]
    public IActionResult CreateOffer(OfferCreateViewModel model)
    {
        var fromUserId = AuthSession.UserId(this);
        if (!fromUserId.HasValue) return RedirectToAction("Login", "Account");

        try
        {
            var offer = _appService.CreateOffer(model.ListingId, fromUserId.Value, model.Amount, model.Note);
            var listing = _appService.GetListing(model.ListingId);

            if (listing is not null)
            {
                _appService.SendMessage(fromUserId.Value, listing.OwnerUserId,
                    $"Yeni teklif geldi: {listing.Title} icin {model.Amount:N0} TL teklif verildi.");
            }

            TempData["Success"] = "Teklifiniz saticiya iletildi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = model.ListingId });
    }

    private bool TrySaveImage(IFormFile? file, out string? path, out string? error)
    {
        path = null;
        error = null;

        if (file is null || file.Length == 0)
        {
            error = "Lutfen bir dosya secin.";
            return false;
        }

        const long maxBytes = 8 * 1024 * 1024;
        if (file.Length > maxBytes)
        {
            error = "Resim boyutu en fazla 8 MB olabilir.";
            return false;
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowed.Contains(ext))
        {
            error = "Sadece .jpg, .jpeg, .png, .webp dosyalari kabul edilir.";
            return false;
        }

        var uploadsPath = Path.Combine(_environment.WebRootPath, "img", "uploads");
        Directory.CreateDirectory(uploadsPath);

        var fileName = $"listing-{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadsPath, fileName);

        using var stream = System.IO.File.Create(fullPath);
        file.CopyTo(stream);

        path = $"/img/uploads/{fileName}";
        return true;
    }

    private void SetLocationViewData()
    {
        var map = _appService.GetLocationMap();
        ViewBag.Provinces = map.Keys.OrderBy(x => x).ToList();
        ViewBag.LocationMapJson = JsonSerializer.Serialize(map);
    }

    private static bool IsValidImageUrl(string value)
    {
        if (value.StartsWith("/img/", StringComparison.OrdinalIgnoreCase)) return true;
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
