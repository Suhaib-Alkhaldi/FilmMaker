using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class EscrowTransaction : SharedEntity
    {
        public int PaymentId { get; set; }

        [ForeignKey("PaymentId")]
        public Payment Payment { get; set; }
        public int LocationBookingRequestId { get; set; }

        [ForeignKey("LocationBookingRequestId")]
        public LocationBookingRequest LocationBookingRequest { get; set; } = null!;
        public decimal Amount { get; set; }
        public int EscrowStatusId { get; set; }

        [ForeignKey("EscrowStatusId")]
        public LockupItem EscrowStatus { get; set; } = null!;
        public DateTime HeldAt{ get; set; } 
        public DateTime? ReleasedAt { get; set; }
        public DateTime? RefundedAt { get; set; }
    }
}
