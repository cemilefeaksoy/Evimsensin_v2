using System.ComponentModel.DataAnnotations;

namespace Evimsensin.Models;

public class Message
{
    public int Id { get; set; }
    public int FromUserId { get; set; }
    public int ToUserId { get; set; }

    [Required, StringLength(1000)]
    public string Content { get; set; } = string.Empty;

    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
