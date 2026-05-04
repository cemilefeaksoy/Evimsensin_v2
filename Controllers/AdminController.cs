using Evimsensin.Services;
using Microsoft.AspNetCore.Mvc;

namespace Evimsensin.Controllers;

public class AdminController : Controller
{
    private readonly AppService _appService;

    public AdminController(AppService appService)
    {
        _appService = appService;
    }

    public IActionResult Dashboard()
    {
        if (!AuthSession.IsAdmin(this)) return Forbid();

        ViewBag.TotalListings = _appService.GetListings().Count;
        return View(_appService.GetListings().Take(10).ToList());
    }

    [HttpPost]
    public IActionResult DeleteComment(int commentId, int listingId)
    {
        if (!AuthSession.IsAdmin(this)) return Forbid();
        _appService.DeleteComment(commentId);
        return RedirectToAction("Details", "Listings", new { id = listingId });
    }
}
