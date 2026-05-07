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
            var seedDate = new DateTime(2026, 5, 7, 0, 0, 0, DateTimeKind.Utc);

            modelBuilder.Entity<LockupCategory>().HasData(
                new Entities.LockupCategory { Id = 1, Name = "LocationStatus" ,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                }
            );

            modelBuilder.Entity<LockupItem>().HasData(
                new Entities.LockupItem { Id = 1, Name = "Published", LockupCategoryId = 1 ,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                },
                new Entities.LockupItem { Id = 2, Name = "Blocked", LockupCategoryId = 1 ,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                }
            );

            modelBuilder.Entity<Role>().HasData(
                new Entities.Role { Id = 1, Name = "Admin" ,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                },
                new Entities.Role { Id = 2, Name = "LocationOwner",
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                },
                new Entities.Role { Id = 3, Name = "LocationManager" ,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                },
                new Entities.Role { Id = 4, Name = "ProductionCompany" ,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                },
                new Entities.Role { Id = 5, Name = "ServiceProvider" ,
                    CreatedAt = seedDate,
                    UpdatedAt = seedDate,
                    CreatedBy = "System",
                    UpdatedBy = "System"
                }
            );

            modelBuilder.Entity<User>().HasData(
       new User
       {
           Id = 1,
           Name = "Admin User",
           Email = "admin@filmmaker.com",
           Password = "123", 
           PhoneNumber = "0500000000",
           IBAN = "SA0000000000000000000000",
           RoleId = 1,
           IsActive = true,
           IsDeleted = false,
           CreatedAt = seedDate,
           UpdatedAt = seedDate,
           CreatedBy = "System",
           UpdatedBy = "System"
       },
       new User
       {
           Id = 2,
           Name = "Omar Owner",
           Email = "omar@owner.com",
           Password = "HashedPasswordHere",
           PhoneNumber = "0511111111",
           IBAN = "SA1111111111111111111111",
           RoleId = 2,
           IsActive = true,
           IsDeleted = false,
           CreatedAt = seedDate,
           UpdatedAt = seedDate,
           CreatedBy = "System",
           UpdatedBy = "System"
       }
   );

     modelBuilder.Entity<LocationOwnerProfile>().HasData(
         new LocationOwnerProfile
         {
             Id = 1,
             UserId = 2, 
             RegisterDate = new DateTime(2026, 1, 1),
             IsActive = true,
             IsDeleted = false,
             CreatedAt = seedDate,
             UpdatedAt = seedDate,
             CreatedBy = "System",
             UpdatedBy = "System"
         }
     );
        }
    }
}
