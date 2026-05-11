using FilmMaker.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilmMaker.DataSeed
{
    public static class ModelBuilderExtensionsDataSeeding
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LockupCategory>().HasData(
                new Entities.LockupCategory { Id = 1, Name = "LocationStatus" }
            );

            modelBuilder.Entity<LockupItem>().HasData(
                new Entities.LockupItem { Id = 1, Name = "Published", LockupCategoryId = 1 },
                new Entities.LockupItem { Id = 2, Name = "Blocked", LockupCategoryId = 1 },
                new Entities.LockupItem { Id = 3, Name = "Deleted", LockupCategoryId = 1 },
                new Entities.LockupItem { Id = 4, Name = "Under Moderation", LockupCategoryId = 1 },
                new Entities.LockupItem { Id = 5, Name = "Archived", LockupCategoryId = 1 }
            );

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" }
                );

            modelBuilder.Entity<LocationOwnerProfile>().HasData(

                new LocationOwnerProfile {Id=1, UserId = 1, RegisterDate = new DateTime(2023, 1, 15) }
                );

            modelBuilder.Entity<User>().HasData(
               new User
               {
                   Id = 1, // Assuming you have an Id property in your User entity
                   Name = "Alice Johnson",
                   Email = "alice.johnson@example.com",
                   Password = "HashedPassword123!", // In a real app, always store hashes!
                   PhoneNumber = "+1-555-0101",
                   RoleId = 1, // Admin
                   IBAN = "US12345678901234567890"
               }
           );
        }
    }
}
