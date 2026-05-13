using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class LocationBookingRequest : SharedEntity
    {
        public int LocationId { get; set; }

        [ForeignKey("LocationId")]
        public Location Location { get; set; } = null!;
        public int BookingStatusId { get; set; }

        [ForeignKey("BookingStatusId")]
        public LookupItem BookingStatus { get; set; } = null!;
        public DateTime ShootingDate { get; set; }
        public string RequestDetails { get; set; } = string.Empty;
        public int LocationOwnerId { get; set; }

        [ForeignKey("LocationOwnerId")]
        public LocationOwnerProfile LocationOwner { get; set; } = null!;

        public int? LocationManagerId { get; set; }

        [ForeignKey("LocationManagerId")]
        public LocationManagerProfile? LocationManager { get; set; }

        public int ProductionCompanyId { get; set; }

        [ForeignKey("ProductionCompanyId")]
        public ProductionCompanyProfile ProductionCompany { get; set; } = null!;

        public decimal TotalPrice { get; set; }

        public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<BookingStatusHistory> StatusHistories { get; set; }
    }
}
