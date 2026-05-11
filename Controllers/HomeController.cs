using Evimsensin.Services;
using Microsoft.AspNetCore.Mvc;

namespace Evimsensin.Controllers;

public class HomeController : Controller
{
    private readonly AppService _appService;

    public HomeController(AppService appService)
    {
        _appService = appService;
    }

    public IActionResult Index()
    {
        ViewBag.UserName = AuthSession.UserName(this);
        var allListings = _appService.GetListings();
        var featured = allListings.Take(3).ToList();
        var adminRecommended = allListings
            .Where(x => x.IsAdminRecommended)
            .Take(6)
            .ToList();
        if (adminRecommended.Count == 0)
        {
            adminRecommended = allListings.Skip(3).Take(6).ToList();
        }
        ViewBag.AdminRecommended = adminRecommended;
        return View(featured);
    }

    public IActionResult About() => View();
    public IActionResult Contact() => View();
    public IActionResult Faq() => View();
    public IActionResult Privacy() => View();
}
