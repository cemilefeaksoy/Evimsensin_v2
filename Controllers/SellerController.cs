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

    [HttpPost]
    public IActionResult UpdateOfferStatus(int offerId, OfferStatus status)
    {
        var userId = AuthSession.UserId(this);
        if (!userId.HasValue) return RedirectToAction("Login", "Account");

        try
        {
            _appService.UpdateOfferStatus(userId.Value, offerId, status);
            TempData["Success"] = status == OfferStatus.Accepted ? "Teklif kabul edildi." : "Teklif reddedildi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Dashboard));
    }
}
