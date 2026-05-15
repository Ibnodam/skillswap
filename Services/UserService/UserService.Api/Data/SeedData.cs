using UserService.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace UserService.Api.Data;

public static class SeedData
{
    public static async Task Initialize(UserDbContext context)
    {
        await context.Database.MigrateAsync();

        if (await context.Skills.AnyAsync())
            return;

        var skills = new List<Skill>
        {
            new() { Id = Guid.NewGuid(), Name = "C#", Category = "Programming" },
            new() { Id = Guid.NewGuid(), Name = "JavaScript", Category = "Programming" },
            new() { Id = Guid.NewGuid(), Name = "TypeScript", Category = "Programming" },
            new() { Id = Guid.NewGuid(), Name = "React", Category = "Frontend" },
            new() { Id = Guid.NewGuid(), Name = "Angular", Category = "Frontend" },
            new() { Id = Guid.NewGuid(), Name = "Vue", Category = "Frontend" },
            new() { Id = Guid.NewGuid(), Name = "Node.js", Category = "Backend" },
            new() { Id = Guid.NewGuid(), Name = "ASP.NET Core", Category = "Backend" },
            new() { Id = Guid.NewGuid(), Name = "PostgreSQL", Category = "Database" },
            new() { Id = Guid.NewGuid(), Name = "MongoDB", Category = "Database" },

            new() { Id = Guid.NewGuid(), Name = "UI/UX Design", Category = "Design" },
            new() { Id = Guid.NewGuid(), Name = "Figma", Category = "Design" },
            new() { Id = Guid.NewGuid(), Name = "Photoshop", Category = "Design" },

            new() { Id = Guid.NewGuid(), Name = "English", Category = "Languages" },
            new() { Id = Guid.NewGuid(), Name = "German", Category = "Languages" },
            new() { Id = Guid.NewGuid(), Name = "Spanish", Category = "Languages" },

            new() { Id = Guid.NewGuid(), Name = "Project Management", Category = "Business" },
            new() { Id = Guid.NewGuid(), Name = "Marketing", Category = "Business" },
            new() { Id = Guid.NewGuid(), Name = "Sales", Category = "Business" },

            new() { Id = Guid.NewGuid(), Name = "Excel", Category = "Office" },
            new() { Id = Guid.NewGuid(), Name = "PowerPoint", Category = "Office" },

            new() { Id = Guid.NewGuid(), Name = "Machine Learning", Category = "AI" },
            new() { Id = Guid.NewGuid(), Name = "Data Analysis", Category = "AI" },

            new() { Id = Guid.NewGuid(), Name = "DevOps", Category = "Infrastructure" },
            new() { Id = Guid.NewGuid(), Name = "Docker", Category = "Infrastructure" },
            new() { Id = Guid.NewGuid(), Name = "Kubernetes", Category = "Infrastructure" }
        };

        await context.Skills.AddRangeAsync(skills);
        await context.SaveChangesAsync();
    }
}