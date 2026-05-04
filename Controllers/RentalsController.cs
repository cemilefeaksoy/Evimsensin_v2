using Evimsensin.Services;
using Evimsensin.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Evimsensin.Controllers;

public class RentalsController : Controller
{
    private readonly AppService _appService;

    public RentalsController(AppService appService)
    {
        _appService = appService;
    }

    [HttpGet]
    public IActionResult Payment(int listingId)
    {
        var userId = AuthSession.UserId(this);
        if (!userId.HasValue) return RedirectToAction("Login", "Account");

        var listing = _appService.GetListing(listingId);
        if (listing is null) return NotFound();

        if (listing.OwnerUserId == userId.Value)
        {
            TempData["Error"] = "Satici kendi ilanini kiralayamaz.";
            return RedirectToAction("Details", "Listings", new { id = listingId });
        }

        if (listing.IsRented)
        {
            TempData["Error"] = "Bu ilan zaten kiralandi.";
            return RedirectToAction("Details", "Listings", new { id = listingId });
        }

        return View(new PaymentViewModel
        {
            ListingId = listingId,
            ListingTitle = listing.Title
        });
    }

    [HttpPost]
    public IActionResult Payment(PaymentViewModel model)
    {
        var userId = AuthSession.UserId(this);
        if (!userId.HasValue) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);

        var listing = _appService.GetListing(model.ListingId);
        if (listing is null) return NotFound();

        try
        {
            var last4 = model.CardNumber.Length >= 4 ? model.CardNumber[^4..] : "0000";
            _appService.Rent(model.ListingId, userId.Value, last4);

            _appService.SendMessage(userId.Value, listing.OwnerUserId,
                $"Ilan kiralandi: {listing.Title} ilaniniz kiralandi. Tebrikler!");

            TempData["Success"] = $"Odeme tamamlandi. {listing.Title} kiralama islemi basarili.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Details", "Listings", new { id = model.ListingId });
    }
}
