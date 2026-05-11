using Evimsensin.Models;
using Evimsensin.Services;
using Microsoft.AspNetCore.Mvc;

namespace Evimsensin.Controllers;

public class SellerController : Controller
{
    private readonly AppService _appService;

    public SellerController(AppService appService)
    {
        _appService = appService;
    }

    public IActionResult Dashboard()
    {
        var userId = AuthSession.UserId(this);
        if (!userId.HasValue) return RedirectToAction("Login", "Account");

        var vm = _appService.GetSellerDashboard(userId.Value);
        return View(vm);
    }

    [HttpGet]
    public IActionResult Offers()
    {
        var userId = AuthSession.UserId(this);
        if (!userId.HasValue) return RedirectToAction("Login", "Account");

        var offers = _appService.GetIncomingOffers(userId.Value);
        return View(offers);
    }

    [HttpPost]
    public IActionResult UpdateOfferStatus(int offerId, OfferStatus status)
    {
        var userId = AuthSession.UserId(this);
        if (!userId.HasValue) return RedirectToAction("Login", "Account");

        try
        {
            var offer = _appService.UpdateOfferStatus(userId.Value, offerId, status);
            var listing = _appService.GetListing(offer.ListingId);

            if (listing is not null)
            {
                var statusText = status == OfferStatus.Accepted ? "KABUL" : "RED";
                _appService.SendMessage(userId.Value, offer.FromUserId,
                    $"{listing.Title} ilaniniz icin teklifiniz {statusText} edildi.");
            }

            if (status == OfferStatus.Accepted && offer.Type == OfferType.RentalRequest)
            {
                TempData["Success"] = "Kiralama talebi kabul edildi. Ilan kiralandi olarak isaretlendi.";
            }
            else
            {
                TempData["Success"] = status == OfferStatus.Accepted ? "Teklif kabul edildi." : "Teklif reddedildi.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Dashboard));
    }
}
