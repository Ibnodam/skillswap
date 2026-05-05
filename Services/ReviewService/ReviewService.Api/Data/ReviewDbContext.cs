using Microsoft.EntityFrameworkCore;
using ReviewService.Api.Models;

namespace ReviewService.Api.Data;

public class ReviewDbContext : DbContext
{
    public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options) { }

    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Review>(entity =>
        {
            entity.ToTable("reviews");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.FromUserName).IsRequired().HasMaxLength(100);
            entity.Property(r => r.ToUserName).IsRequired().HasMaxLength(100);
            entity.Property(r => r.Text).IsRequired().HasMaxLength(1000);
        });
    }
}