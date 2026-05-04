using System.ComponentModel.DataAnnotations;

namespace Evimsensin.Models;

public class User
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Customer;
}
