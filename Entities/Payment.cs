using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class Payment : SharedEntity
    {
        public int LocationBookingRequestId { get; set; }

        [ForeignKey("LocationBookingRequestId")]
        public LocationBookingRequest LocationBookingRequest { get; set; } = null!;
        public int DigitalContractId { get; set; }

        [ForeignKey("DigitalContractId")]
        public DigitalContract DigitalContract { get; set; } = null!;
        public decimal Amount { get; set; }
        public int PaymentStatusId { get; set; }

        [ForeignKey("PaymentStatusId")]
        public LockupItem PaymentStatus { get; set; } = null!;
        public int PaymentTypeId { get; set; }

        [ForeignKey("PaymentTypeId")]
        public LockupItem PaymentType { get; set; } = null!;
        public string? PaymentReference { get; set; }
        public DateTime? PaidAt { get; set; }
    }
}
