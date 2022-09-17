using Microsoft.EntityFrameworkCore;
using OnlineJudge.Models.Domain;

namespace OnlineJudge.Database;
public class Context : DbContext
{
    public Context(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}