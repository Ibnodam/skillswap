using Microsoft.EntityFrameworkCore;
using MeetingService.Api.Models;

namespace MeetingService.Api.Data;

public class MeetingDbContext : DbContext
{
    public MeetingDbContext(DbContextOptions<MeetingDbContext> options) : base(options) { }

    public DbSet<Meeting> Meetings => Set<Meeting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToTable("meetings");
            entity.HasKey(m => m.Id);
            entity.Property(m => m.RequesterName).IsRequired().HasMaxLength(100);
            entity.Property(m => m.ReceiverName).IsRequired().HasMaxLength(100);
            entity.Property(m => m.SkillName).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Time).IsRequired().HasMaxLength(10);
            entity.Property(m => m.Format).IsRequired().HasMaxLength(20);
            entity.Property(m => m.Status).IsRequired().HasMaxLength(20);
        });
    }
}