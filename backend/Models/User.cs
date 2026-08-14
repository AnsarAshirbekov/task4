namespace backend.Models;

public class User
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set;} = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Status { get; set; } = "unverified";

    public DateTime RegisteredAt { get; set; }

    public DateTime? LastLoginAt {get; set; }

    public Guid? ConfirmationToken { get; set; }
}