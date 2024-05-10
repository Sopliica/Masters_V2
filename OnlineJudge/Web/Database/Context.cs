using Microsoft.EntityFrameworkCore;
using OnlineJudge.Models.Domain;

namespace OnlineJudge.Database;
public class Context : DbContext
{
    public Context(DbContextOptions options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Assignment> Assignments { get; set; }

    public DbSet<Submission> Submissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Assignment>()
            .HasMany(a => a.Submissions)
            .WithOne(s => s.Assignment)
            .HasForeignKey(e => e.AssignmentId);

        modelBuilder.Entity<User>()
            .HasMany(a => a.Submissions)
            .WithOne(s => s.User)
            .HasForeignKey(e => e.UserId);

        modelBuilder.Entity<Submission>()
            .HasMany(s => s.Results)
            .WithOne(x => x.Submission)
            .HasForeignKey(e => e.SubmissionId);

        base.OnModelCreating(modelBuilder);
    }
}