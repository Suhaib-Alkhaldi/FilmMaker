using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class Payment : SharedEntity
    {
        public int BookingId { get; set; }
        [ForeignKey("BookingId")]
        public LocationBookingRequest LocationBookingRequest { get; set; }
        public double Amount { get; set; }
        public int PaymentStatusId { get; set; }
        [ForeignKey("PaymentStatusId")]
        public LockupItem PaymentStatus { get; set; }
        public int PaymentTypeId { get; set; }
        [ForeignKey("PaymentTypeId")]
        public LockupItem PaymentType { get; set; }
    }
}
