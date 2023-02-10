namespace OnlineJudge.Models.Domain;

public class User
{
    public User(string login, string role)
    {
        Login = login;
        Role = role;
    }

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Login { get; set; }

    public string PasswordHash { get; set; }

    public string Role { get; set; }

    public List<Submission> Submissions { get; set; } = new List<Submission>();
}