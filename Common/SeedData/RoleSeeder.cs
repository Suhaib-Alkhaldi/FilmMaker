using FilmMaker.Entities;

namespace FilmMaker.Common.SeedData
{
    public static class RoleSeeder
    {
        public static void Seed(FilmMakerDbContext context)
        {
            if (context.Roles.Any())
                return;

            var roles = new List<Role>
        {
            new() { Name = "Admin" },
            new() { Name = "Location Owner" },
            new() { Name = "Location Manager" },
            new() { Name = "Production Company" },
            new() { Name = "Service Provider" }
        };

            context.Roles.AddRange(roles);
            context.SaveChanges();
        }
    }
}
