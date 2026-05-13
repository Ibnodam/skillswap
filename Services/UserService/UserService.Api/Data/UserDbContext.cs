using Microsoft.EntityFrameworkCore;
using UserService.Api.Models;
using UsersService.Api.Models;

namespace UserService.Api.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();

    public DbSet<UserBan> UserBans => Set<UserBan>();

    public DbSet<BannedUser> BannedUsers => Set<BannedUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Name).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.ToTable("skills");
            entity.HasKey(s => s.Id);
            entity.HasIndex(s => s.Name).IsUnique();
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<UserSkill>(entity =>
        {
            entity.ToTable("user_skills");
            entity.HasKey(us => new { us.UserId, us.SkillId, us.Type });

            entity.HasOne(us => us.User)
                  .WithMany(u => u.UserSkills)
                  .HasForeignKey(us => us.UserId);

            entity.HasOne(us => us.Skill)
                  .WithMany(s => s.UserSkills)
                  .HasForeignKey(us => us.SkillId);

            entity.Property(us => us.Type).IsRequired().HasMaxLength(20);
        });

        // === Конфигурация новой сущности BannedUser ===
        modelBuilder.Entity<BannedUser>(entity =>
        {
            entity.ToTable("banned_users");
            entity.HasKey(b => b.Id);

            entity.HasIndex(b => b.UserId).IsUnique();     // один активный бан на пользователя
            entity.HasIndex(b => b.Email);

            entity.HasOne(b => b.User)
                  .WithOne(u => u.BannedUser)
                  .HasForeignKey<BannedUser>(b => b.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Старая конфигурация UserBan (оставляем, чтобы не сломать)
        modelBuilder.Entity<UserBan>(entity =>
        {
            entity.HasOne(b => b.User)
                  .WithMany()
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}














//using Microsoft.EntityFrameworkCore;
//using UserService.Api.Models;
//using UsersService.Api.Models;

//namespace UserService.Api.Data;

//public class UserDbContext : DbContext
//{
//    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

//    public DbSet<User> Users => Set<User>();
//    public DbSet<Skill> Skills => Set<Skill>();
//    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
//    public DbSet<UserBan> UserBans => Set<UserBan>();
//    public DbSet<BannedUser> BannedUsers => Set<BannedUser>();

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        modelBuilder.Entity<User>(entity =>
//        {
//            entity.ToTable("users");
//            entity.HasKey(u => u.Id);
//            entity.HasIndex(u => u.Email).IsUnique();
//            entity.HasIndex(u => u.Name).IsUnique();
//            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
//            entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
//        });

//        modelBuilder.Entity<Skill>(entity =>
//        {
//            entity.ToTable("skills");
//            entity.HasKey(s => s.Id);
//            entity.HasIndex(s => s.Name).IsUnique();
//            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
//        });

//        modelBuilder.Entity<UserSkill>(entity =>
//        {
//            entity.ToTable("user_skills");
//            entity.HasKey(us => new { us.UserId, us.SkillId, us.Type });

//            entity.HasOne(us => us.User)
//                .WithMany(u => u.UserSkills)
//                .HasForeignKey(us => us.UserId);

//            entity.HasOne(us => us.Skill)
//                .WithMany(s => s.UserSkills)
//                .HasForeignKey(us => us.SkillId);

//            entity.Property(us => us.Type).IsRequired().HasMaxLength(20);
//        });

//        modelBuilder.Entity<UserBan>(entity =>
//        {
//            entity.HasOne(b => b.User)
//                  .WithMany()
//                  .HasForeignKey(b => b.UserId)
//                  .OnDelete(DeleteBehavior.Cascade);
//        });
//    }
//}