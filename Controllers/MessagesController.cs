using Evimsensin.Services;
using Evimsensin.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Evimsensin.Controllers;

public class MessagesController : Controller
{
    private readonly AppService _appService;

    public MessagesController(AppService appService)
    {
        _appService = appService;
    }

    public IActionResult Inbox()
    {
        var userId = AuthSession.UserId(this);
        if (!userId.HasValue) return RedirectToAction("Login", "Account");

        var conversations = _appService.GetInboxConversations(userId.Value);

        return View(new InboxViewModel
        {
            Conversations = conversations,
            CurrentUserId = userId.Value,
            IsAdmin = AuthSession.IsAdmin(this)
        });
    }

    public IActionResult Chat(int withUserId)
    {
        var currentUserId = AuthSession.UserId(this);
        if (!currentUserId.HasValue) return RedirectToAction("Login", "Account");
        if (currentUserId.Value == withUserId) return RedirectToAction(nameof(Inbox));

        var other = _appService.GetUser(withUserId);
        if (other is null) return NotFound();

        _appService.MarkConversationAsRead(currentUserId.Value, withUserId);

        var vm = new ChatThreadViewModel
        {
            OtherUser = other,
            CurrentUserId = currentUserId.Value,
            Messages = _appService.GetConversation(currentUserId.Value, withUserId)
        };

        ViewBag.WithUserId = withUserId;
        return View(vm);
    }

    [HttpPost]
    public IActionResult Send(int withUserId, string content)
    {
        var currentUserId = AuthSession.UserId(this);
        if (!currentUserId.HasValue) return RedirectToAction("Login", "Account");
        if (currentUserId.Value == withUserId) return RedirectToAction(nameof(Inbox));

        if (!string.IsNullOrWhiteSpace(content))
        {
            _appService.SendMessage(currentUserId.Value, withUserId, content);
        }

        return RedirectToAction(nameof(Chat), new { withUserId });
    }

    [HttpGet]
    public IActionResult UnreadCount()
    {
        var userId = AuthSession.UserId(this);
        if (!userId.HasValue)
        {
            return Json(new { unread = 0, latestUnreadMessageId = 0, fromName = "", preview = "" });
        }

        var latest = _appService.GetLatestUnreadMessage(userId.Value);
        var fromName = latest is null ? string.Empty : (_appService.GetUser(latest.FromUserId)?.FullName ?? "Bilinmeyen");
        var preview = latest is null ? string.Empty : latest.Content;

        return Json(new
        {
            unread = _appService.GetUnreadCount(userId.Value),
            latestUnreadMessageId = latest?.Id ?? 0,
            fromName,
            preview
        });
    }

    [HttpGet]
    public IActionResult Conversation(int withUserId)
    {
        var currentUserId = AuthSession.UserId(this);
        if (!currentUserId.HasValue) return Unauthorized();

        _appService.MarkConversationAsRead(currentUserId.Value, withUserId);

        var messages = _appService.GetConversation(currentUserId.Value, withUserId)
            .Select(m => new
            {
                id = m.Id,
                fromUserId = m.FromUserId,
                content = m.Content,
                createdAt = m.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
            });

        return Json(messages);
    }
}
