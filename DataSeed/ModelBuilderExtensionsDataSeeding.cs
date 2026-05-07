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
                new Entities.LockupItem { Id = 1, Name = "Published" , LockupCategoryId = 1},
                new Entities.LockupItem { Id = 2, Name = "Blocked" , LockupCategoryId = 1}
            );


        }
    }
}
