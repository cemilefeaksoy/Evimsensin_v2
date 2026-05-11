using Evimsensin.Services;
using Evimsensin.Models;
using Evimsensin.ViewModels;
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
        ViewBag.TotalUsers = _appService.GetUsers().Count;
        ViewBag.PendingSellers = _appService.GetUsers().Count(x => x.Role == UserRole.Customer && !x.IsSellerApproved);
        ViewBag.Users = _appService.GetUsers();
        return View(_appService.GetListings().Take(12).ToList());
    }

    [HttpPost]
    public IActionResult DeleteComment(int commentId, int listingId)
    {
        if (!AuthSession.IsAdmin(this)) return Forbid();
        _appService.DeleteComment(commentId);
        return RedirectToAction("Details", "Listings", new { id = listingId });
    }

    [HttpGet]
    public IActionResult EditUser(int id)
    {
        if (!AuthSession.IsAdmin(this)) return Forbid();
        var user = _appService.GetUser(id);
        if (user is null) return NotFound();

        return View(new UserAdminEditViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsSellerApproved = user.IsSellerApproved,
            Bio = user.Bio,
            ProfileImageUrl = user.ProfileImageUrl
        });
    }

    [HttpPost]
    public IActionResult EditUser(UserAdminEditViewModel model)
    {
        if (!AuthSession.IsAdmin(this)) return Forbid();
        if (!ModelState.IsValid) return View(model);

        try
        {
            _appService.UpdateUserByAdmin(model);
            TempData["Success"] = "Kullanici bilgileri guncellendi.";
            return RedirectToAction(nameof(Dashboard));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    public IActionResult ToggleSellerApproval(int id)
    {
        if (!AuthSession.IsAdmin(this)) return Forbid();
        var user = _appService.GetUser(id);
        if (user is null) return RedirectToAction(nameof(Dashboard));

        _appService.SetSellerApproval(id, !user.IsSellerApproved);
        TempData["Success"] = user.IsSellerApproved ? "Satici onayi kaldirildi." : "Satici onayi verildi.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    public IActionResult ToggleAdminRole(int id)
    {
        if (!AuthSession.IsAdmin(this)) return Forbid();
        var user = _appService.GetUser(id);
        if (user is null) return RedirectToAction(nameof(Dashboard));

        try
        {
            var makeAdmin = user.Role != UserRole.Admin;
            _appService.SetAdminRole(id, makeAdmin);
            TempData["Success"] = makeAdmin ? "Kullanici admin yapildi." : "Kullanicinin admin rolu kaldirildi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    public IActionResult DeleteUser(int id)
    {
        if (!AuthSession.IsAdmin(this)) return Forbid();
        var currentUserId = AuthSession.UserId(this);

        try
        {
            _appService.DeleteUser(id);
            TempData["Success"] = "Kullanici silindi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        if (currentUserId.HasValue && currentUserId.Value == id)
        {
            AuthSession.SignOut(this);
            return RedirectToAction("Index", "Home");
        }

        return RedirectToAction(nameof(Dashboard));
    }
}
