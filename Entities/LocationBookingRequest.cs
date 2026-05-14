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
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        [NotMapped]
        public bool IsFullDay => (EndDateTime - StartDateTime).TotalHours >= 4;
        public int LocationOwnerId { get; set; }

        [ForeignKey("LocationOwnerId")]
        public LocationOwnerProfile LocationOwner { get; set; } = null!;

        public int? LocationManagerId { get; set; }

        [ForeignKey("LocationManagerId")]
        public LocationManagerProfile? LocationManager { get; set; }

        public int? ProductionCompanyId { get; set; }

        [ForeignKey("ProductionCompanyId")]

        public ProductionCompanyProfile ProductionCompany { get; set; } = null!;
        public string? Message { get; set; }

      

        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BookingStatusHistory> StatusHistories { get; set; }

        public ICollection<LocationVisitRequest> VisitRequests { get; set; }
    }
}
