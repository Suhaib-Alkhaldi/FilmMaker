using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class DigitalContract : SharedEntity
    {
        public int LocationBookingRequestId { get; set; }

        [ForeignKey("LocationBookingRequestId")]
        public LocationBookingRequest LocationBookingRequest { get; set; } = null!;
        public string ContractNumber { get; set; } = string.Empty;
        public string TermsSnapshot { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime ContractStart { get; set; }
        public DateTime ContractEnd { get; set; }
        public int ContractStatusId { get; set; }

        [ForeignKey("ContractStatusId")]
        public LookupItem ContractStatus { get; set; } = null!;
        public ICollection<DigitalContractApproval> Approvals { get; set; }
    }
}
