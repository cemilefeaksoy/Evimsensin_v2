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
        return View(_appService.GetListings().Take(6).ToList());
    }

    public IActionResult About() => View();
    public IActionResult Contact() => View();
    public IActionResult Faq() => View();
    public IActionResult Privacy() => View();
}
