using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Entities
{
    public class FilmMakerDbContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public DbSet<LookupCategory> LookupCategories { get; set; }
        public DbSet<LookupItem> LookupItems { get; set; }

        // Profiles
        public DbSet<LocationOwnerProfile> LocationOwnerProfiles { get; set; }
        public DbSet<LocationManagerProfile> LocationManagerProfiles { get; set; }
        public DbSet<LocationManagerCity> LocationManagerCities { get; set; }
        public DbSet<PreviousProject> PreviousProjects { get; set; }
        public DbSet<ServiceBooking> ServiceBookings { get; set; }

        public DbSet<ServicesProvided> ServicesProvided { get; set; }

        public DbSet<ServicesProvidedMedia> ServicesProvidedMedia { get; set; }

        public DbSet<ServiceProviderCities> ServiceProviderCities { get; set; }

        public DbSet<ProductionCompanyProfile> ProductionCompanyProfiles { get; set; }

        public DbSet<ProductionCompanyProductionType> ProductionCompanyProductionTypes { get; set; }

        public DbSet<ServiceProviderProfile> ServiceProviderProfiles { get; set; }
        public DbSet<ServiceProviderServiceType> ServiceProviderServiceTypes { get; set; }

        public DbSet<Media> Media { get; set; }

        // Locations
        public DbSet<Location> Locations { get; set; }
        public DbSet<LocationMedia> LocationMedia { get; set; }
        public DbSet<LocationTermsOfUse> LocationTermsOfUse { get; set; }
        public DbSet<LocationArchiveHistory> LocationArchiveHistories { get; set; }
        
        // Booking
        public DbSet<LocationBookingRequest> LocationBookingRequests { get; set; }
        public DbSet<BookingStatusHistory> BookingStatusHistories { get; set; }
        public DbSet<LocationVisitRequest> LocationVisitRequests { get; set; }

        // Contract
        public DbSet<DigitalContract> DigitalContracts { get; set; }
        public DbSet<DigitalContractApproval> DigitalContractApprovals { get; set; }

        // Payment / Escrow
        public DbSet<Payment> Payments { get; set; }
        public DbSet<EscrowTransaction> EscrowTransactions { get; set; }

        public FilmMakerDbContext(DbContextOptions<FilmMakerDbContext> options)
        : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            

            modelBuilder.Entity<LocationArchiveHistory>()
                .HasOne(x => x.Location)
                .WithMany(x => x.ArchiveHistories)
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LocationArchiveHistory>()
                .HasOne(x => x.ArchivedByUser)
                .WithMany()
                .HasForeignKey(x => x.ArchivedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LocationArchiveHistory>()
                .HasOne(x => x.RestoredByUser)
                .WithMany()
                .HasForeignKey(x => x.RestoredByUserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<LocationBookingRequest>()
                .HasOne(x => x.Location)
                .WithMany()
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LocationBookingRequest>()
                .HasOne(x => x.LocationOwner)
                .WithMany()
                .HasForeignKey(x => x.LocationOwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ServiceBooking>()
            .HasOne(x => x.Service)
            .WithMany()
            .HasForeignKey(x => x.ServiceId)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Location>()
             .HasOne(x => x.City)
             .WithMany()
             .HasForeignKey(x => x.CityId)
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
                .HasForeignKey(x => x.BookingStatusId)
                .OnDelete(DeleteBehavior.NoAction);



            modelBuilder.Entity<BookingStatusHistory>()
                .HasOne(x => x.LocationBookingRequest)
                .WithMany(x => x.StatusHistories)
                .HasForeignKey(x => x.LocationBookingRequestId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookingStatusHistory>()
                .HasOne(x => x.FromStatus)
                .WithMany()
                .HasForeignKey(x => x.FromStatusId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookingStatusHistory>()
                .HasOne(x => x.ToStatus)
                .WithMany()
                .HasForeignKey(x => x.ToStatusId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BookingStatusHistory>()
                .HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.NoAction);



            modelBuilder.Entity<Payment>()
                .HasOne(x => x.LocationBookingRequest)
                .WithMany()
                .HasForeignKey(x => x.LocationBookingRequestId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Payment>()
                .HasOne(x => x.DigitalContract)
                .WithMany()
                .HasForeignKey(x => x.DigitalContractId)
                .OnDelete(DeleteBehavior.NoAction);

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


            

            modelBuilder.Entity<LocationVisitRequest>()
                .HasOne(v => v.Location)
                .WithMany()
                .HasForeignKey(v => v.LocationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LocationVisitRequest>()
                .HasOne(v => v.LocationOwner)
                .WithMany()
                .HasForeignKey(v => v.LocationOwnerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LocationVisitRequest>()
                .HasOne(v => v.LocationManager)
                .WithMany()
                .HasForeignKey(v => v.LocationManagerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LocationVisitRequest>()
                .HasOne(v => v.VisitStatus)
                .WithMany()
                .HasForeignKey(v => v.VisitStatusId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LocationVisitRequest>()
                .HasOne(v => v.RespondedByUser)
                .WithMany()
                .HasForeignKey(v => v.RespondedByUserId);


            modelBuilder.Entity<LocationMedia>()
                .HasOne(x => x.Media)
                .WithMany()
                .HasForeignKey(x => x.MediaId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<LocationMedia>()
                .HasOne(x => x.Location)
                .WithMany(x => x.Media)
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Media>()
                .HasOne(x => x.UploadedByUser)
                .WithMany()
                .HasForeignKey(x => x.UploadedByUserId)
                .OnDelete(DeleteBehavior.NoAction);



            modelBuilder.Entity<Location>()
                .HasOne(x => x.LocationType)
                .WithMany()
                .HasForeignKey(x => x.LocationTypeId)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<Location>()
                .HasOne(x => x.LocationStatus)
                .WithMany()
                .HasForeignKey(x => x.LocationStatusId)
                .OnDelete(DeleteBehavior.NoAction);
            
        }
    }
}
