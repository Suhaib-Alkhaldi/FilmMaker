using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class ServiceBooking : SharedEntity
    {
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public ServicesProvided Service { get; set; } = null!;

        public int RequesterId { get; set; }

        [ForeignKey("RequesterId")]
        public User Requester { get; set; } = null!;

        public int? LocationBookingId { get; set; }

        [ForeignKey("LocationBookingId")]
        public LocationBookingRequest? LocationBooking { get; set; }

        public int? ServiceProviderRequestItemId { get; set; }

        [ForeignKey("ServiceProviderRequestItemId")]
        public ServiceProviderRequestItem? ServiceProviderRequestItem { get; set; }

        public int? Quantity { get; set; }

        public string? Notes { get; set; }

        public int StatusId { get; set; }

        [ForeignKey("StatusId")]
        public LookupItem Status { get; set; } = null!;

        public DateTime BookingStartDate { get; set; }

        public DateTime BookingEndDate { get; set; }
    }
}
