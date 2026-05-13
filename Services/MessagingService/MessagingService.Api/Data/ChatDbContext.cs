using Microsoft.EntityFrameworkCore;
using MessagingService.Api.Models;

namespace MessagingService.Api.Data;

public class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }

    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages");
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => new { m.SenderId, m.ReceiverId, m.SentAt });
            entity.Property(m => m.Text).IsRequired().HasMaxLength(2000);
            entity.Property(m => m.SenderName).IsRequired().HasMaxLength(100);
        });
    }
}