using FilmMaker.DataSeed;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Entities
{
    public class FilmMakerDbContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<ProductionCompanyProfile>productionCompanyProfiles { get; set; }
        public DbSet<LocationOwnerProfile> locationOwnerProfiles { get; set; }
        public DbSet<LocationManagerProfile> locationManagerProfiles { get; set; }
        public DbSet<ServiceProviderProfile> serviceProviderProfiles { get; set; }
        public DbSet<LocationBookingRequest> locationBookingRequests { get; set; }
        public DbSet<Location> locations { get; set; }
        public DbSet<PreviousLocation> previousLocations { get; set; }
        public DbSet<LockupCategory> lockupCategories { get; set; }
        public DbSet<LockupItem> lockupItems { get; set; }
        public DbSet<DigitalContract> digitalContract { get; set; }
        public DbSet<Payment> payments { get; set; }
        public DbSet<LocationTermsOfUse> locationTermsOfUse { get; set; }

        public FilmMakerDbContext(DbContextOptions<FilmMakerDbContext> options)
        : base(options)
        {
        }





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Seed();

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DigitalContract>()
                        .HasOne(d => d.ContractStatus)
                        .WithMany()
                        .HasForeignKey(d => d.ContractStatusId)
                        .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DigitalContract>()
                .HasOne(d => d.BookingStatus)
                .WithMany()
                .HasForeignKey(d => d.BookingStatusId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LocationBookingRequest>()
            .HasOne(x => x.LocationOwner)
            .WithMany()
            .HasForeignKey(x => x.LocationOwnerId)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LocationBookingRequest>()
                .HasOne(x => x.LocationManager)
                .WithMany()
                .HasForeignKey(x => x.LocationManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LocationBookingRequest>()
                .HasOne(x => x.ProductionCompany)
                .WithMany()
                .HasForeignKey(x => x.ProductionCompanyId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LocationBookingRequest>()
                .HasOne(x => x.BookingStatus)
                .WithMany()
                .HasForeignKey(x => x.BookingRequestStatusId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DigitalContract>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Location>()
                .Property(x => x.DailyPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Location>()
                .Property(x => x.Latitude)
                .HasPrecision(18, 6);

            modelBuilder.Entity<Location>()
                .Property(x => x.Longitude)
                .HasPrecision(18, 6);

            modelBuilder.Entity<LocationBookingRequest>()
                .Property(x => x.TotalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LocationManagerProfile>()
                .Property(x => x.CommissionRate)
                .HasPrecision(5, 2);


            modelBuilder.Entity<Payment>()
                .HasOne(x => x.PaymentStatus)
                .WithMany()
                .HasForeignKey(x => x.PaymentStatusId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
                .HasOne(x => x.PaymentType)
                .WithMany()
                .HasForeignKey(x => x.PaymentTypeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
                .HasOne(x => x.LocationBookingRequest)
                .WithMany()
                .HasForeignKey(x => x.BookingId)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<PreviousLocation>()
                    .HasOne(pl => pl.LocationStatus)
                    .WithMany()
                    .HasForeignKey(pl => pl.LocationStatusId)
                    .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
