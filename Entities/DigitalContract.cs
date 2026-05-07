using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class DigitalContract : SharedEntity
    {
        public string Terms { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime ContractStart { get; set; }
        public DateTime ContractEnd { get; set; }
        public int ContractStatusId { get; set; }
        [ForeignKey("ContractStatusId")]
        public LockupItem ContractStatus { get; set; }
        public int BookingStatusId { get; set; }
        [ForeignKey("BookingStatusId")]
        public LockupItem BookingStatus { get; set; }
    }
}
