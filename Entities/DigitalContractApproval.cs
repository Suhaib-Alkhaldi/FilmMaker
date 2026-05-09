using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class DigitalContractApproval : SharedEntity
    {
        public int DigitalContractId { get; set; }

        [ForeignKey("DigitalContractId")]
        public DigitalContract DigitalContract { get; set; } = null!;
        public int ApprovedByUserId { get; set; }

        [ForeignKey("ApprovedByUserId")]
        public User ApprovedByUser { get; set; } = null!;
        public DateTime ApprovedAt { get; set; }
    }
}
