using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class BookingStatusHistory : SharedEntity
    {
        public int LocationBookingRequestId { get; set; }

        [ForeignKey("LocationBookingRequestId")]
        public LocationBookingRequest LocationBookingRequest { get; set; }
        public int FromStatusId { get; set; }

        [ForeignKey("FromStatusId")]
        public LockupItem FromStatus { get; set; }
        public int ToStatusId { get; set; }

        [ForeignKey("ToStatusId")]
        public LockupItem ToStatus { get; set; }
        public int ChangedByUserId { get; set; }

        [ForeignKey("ChangedByUserId")]
        public User ChangedByUser { get; set; }
        public DateTime ChangedAt { get; set; }
        public string? Reason { get; set; }
    }
}
