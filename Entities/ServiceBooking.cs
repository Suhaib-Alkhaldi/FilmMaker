using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class ServiceBooking : SharedEntity
    {
       
        public int ServiceId { get; set; }
        public ServicesProvided Service { get; set; }

        public int RequesterId { get; set; }

        [ForeignKey("RequesterId")]
        public User Requester { get; set; }

        public int? LocationId { get; set; }
        public Location Location { get; set; }

        public string? Notes { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public string? LocationOnGoogleMaps { get; set; }

        public int StatusId { get; set; }

        [ForeignKey("StatusId")]
        public LookupItem Status { get; set; }

        public DateTime bookingStartDate { get; set; }

        public DateTime bookingEndDate { get; set; }
    }
}
