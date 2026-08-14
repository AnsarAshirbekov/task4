using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {        
    }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                .HasColumnName("id");

            entity.Property(u => u.Name)
                .HasColumnName("name");

            entity.Property(u => u.Email)
                .HasColumnName("email");

            entity.Property(u => u.PasswordHash)
                .HasColumnName("password_hash");

            entity.Property(u => u.Status)
                .HasColumnName("status");

            entity.Property(u => u.RegisteredAt)
                .HasColumnName("registered_at");

            entity.Property(u => u.LastLoginAt)
                .HasColumnName("last_login_at");

            entity.Property(u => u.ConfirmationToken)
                .HasColumnName("confirmation_token");
        });
    } 
}