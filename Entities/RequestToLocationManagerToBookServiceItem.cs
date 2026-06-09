using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class RequestToLocationManagerToBookServiceItem : SharedEntity
    {
        public int RequestToLocationManagerToBookServiceId { get; set; }

        [ForeignKey("RequestToLocationManagerToBookServiceId")]
        public RequestToLocationManagerToBookService RequestToLocationManagerToBookService { get; set; } = null!;
        public int? ServiceTypeId { get; set; }

        [ForeignKey("ServiceTypeId")]
        public LookupItem? ServiceType { get; set; }
        public string? CustomServiceType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Details { get; set; }
        public int? Quantity { get; set; }
    }
}
