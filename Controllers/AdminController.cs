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

    public IActionResult Dashboard(
        string? tab = "overview",
        string? userSearch = null,
        string? listingSearch = null,
        string? dateFrom = null,
        string? dateTo = null,
        string? listingStatus = null,
        string? globalSearch = null,
        string? sort = null)
    {
        if (!AuthSession.IsAdmin(this)) return RedirectToAction("Login", "Account");

        var allUsers = _appService.GetUsers();
        var allListings = _appService.GetListings();

        // ─── KPIs ─────────────────────────────────
        ViewBag.TotalListings = allListings.Count;
        ViewBag.TotalUsers = allUsers.Count;
        ViewBag.PendingSellers = allUsers.Count(x => x.Role == UserRole.Customer && !x.IsSellerApproved);
        ViewBag.ActiveListings = allListings.Count(x => !x.IsRented);
        ViewBag.RentedListings = allListings.Count(x => x.IsRented);

        // ─── User search ──────────────────────────
        var filteredUsers = allUsers.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(userSearch))
        {
            var q = userSearch.Trim().ToLowerInvariant();
            filteredUsers = filteredUsers.Where(u =>
                u.FullName.ToLowerInvariant().Contains(q) ||
                u.Email.ToLowerInvariant().Contains(q));
        }
        ViewBag.Users = filteredUsers.ToList();
        ViewBag.UserSearch = userSearch;

        // ─── Listing search + date filter ─────────
        var filteredListings = allListings.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(listingSearch))
        {
            var q = listingSearch.Trim().ToLowerInvariant();
            filteredListings = filteredListings.Where(l =>
                l.Title.ToLowerInvariant().Contains(q) ||
                l.Province.ToLowerInvariant().Contains(q) ||
                l.OwnerName.ToLowerInvariant().Contains(q));
        }

        if (DateTime.TryParse(dateFrom, out var dfrom))
            filteredListings = filteredListings.Where(l => l.CreatedAt.Date >= dfrom.Date);

        if (DateTime.TryParse(dateTo, out var dto))
            filteredListings = filteredListings.Where(l => l.CreatedAt.Date <= dto.Date);

        if (listingStatus == "active")
            filteredListings = filteredListings.Where(l => !l.IsRented);
        else if (listingStatus == "rented")
            filteredListings = filteredListings.Where(l => l.IsRented);

        filteredListings = sort switch
        {
            "oldest" => filteredListings.OrderBy(l => l.CreatedAt),
            _ => filteredListings.OrderByDescending(l => l.CreatedAt)
        };

        ViewBag.ListingSearch = listingSearch;
        ViewBag.DateFrom = dateFrom;
        ViewBag.DateTo = dateTo;
        ViewBag.ListingStatus = listingStatus;
        ViewBag.Sort = sort;

        // ─── Global search ────────────────────────
        List<User> searchUsers = new();
        List<Listing> searchListings = new();
        if (!string.IsNullOrWhiteSpace(globalSearch))
        {
            var q = globalSearch.Trim().ToLowerInvariant();
            searchUsers = allUsers.Where(u =>
                u.FullName.ToLowerInvariant().Contains(q) ||
                u.Email.ToLowerInvariant().Contains(q)).ToList();
            searchListings = allListings.Where(l =>
                l.Title.ToLowerInvariant().Contains(q) ||
                l.Province.ToLowerInvariant().Contains(q) ||
                l.OwnerName.ToLowerInvariant().Contains(q)).ToList();
        }
        ViewBag.GlobalSearch = globalSearch;
        ViewBag.SearchUsers = searchUsers;
        ViewBag.SearchListings = searchListings;

        // ─── Active tab ───────────────────────────
        ViewBag.ActiveTab = tab ?? "overview";

        return View(filteredListings.ToList());
    }

    [HttpPost]
    public IActionResult DeleteComment(int commentId, int listingId)
    {
        if (!AuthSession.IsAdmin(this)) return RedirectToAction("Login", "Account");
        _appService.DeleteComment(commentId);
        return RedirectToAction("Details", "Listings", new { id = listingId });
    }

    [HttpGet]
    public IActionResult EditUser(int id)
    {
        if (!AuthSession.IsAdmin(this)) return RedirectToAction("Login", "Account");
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
        if (!AuthSession.IsAdmin(this)) return RedirectToAction("Login", "Account");
        if (!ModelState.IsValid) return View(model);

        try
        {
            _appService.UpdateUserByAdmin(model);
            TempData["Success"] = "Kullanici bilgileri guncellendi.";
            return RedirectToAction(nameof(Dashboard), new { tab = "users" });
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
        if (!AuthSession.IsAdmin(this)) return RedirectToAction("Login", "Account");
        var user = _appService.GetUser(id);
        if (user is null) return RedirectToAction(nameof(Dashboard));

        _appService.SetSellerApproval(id, !user.IsSellerApproved);
        TempData["Success"] = user.IsSellerApproved ? "Satici onayi kaldirildi." : "Satici onayi verildi.";
        return RedirectToAction(nameof(Dashboard), new { tab = "users" });
    }

    [HttpPost]
    public IActionResult ToggleAdminRole(int id)
    {
        if (!AuthSession.IsAdmin(this)) return RedirectToAction("Login", "Account");
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

        return RedirectToAction(nameof(Dashboard), new { tab = "users" });
    }

    [HttpPost]
    public IActionResult DeleteUser(int id)
    {
        if (!AuthSession.IsAdmin(this)) return RedirectToAction("Login", "Account");
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

        return RedirectToAction(nameof(Dashboard), new { tab = "users" });
    }
}
