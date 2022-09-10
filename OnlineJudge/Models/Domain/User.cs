namespace OnlineJudge.Models.Domain;

public class User
{
    public User(string email, string role)
    {
        Email = email;
        Role = role;
    }

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; }

    public string PasswordHash { get; set; }

    public string Role { get; set; }
}