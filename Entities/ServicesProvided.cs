using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class ServicesProvided : SharedEntity
    {
        public string ServiceName { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "smallmoney")]
        public decimal DailyPrice { get; set; }

        public int? ServiceTypeId { get; set; }

        [ForeignKey("ServiceTypeId")]
        public LookupItem ServiceType { get; set; }
        public string? CustomServiceType { get; set; }
        public bool IsCustom { get; set; }
        public int ServiceProviderId { get; set; }

        [ForeignKey("ServiceProviderId")]
        public ServiceProviderProfile ServiceProvider { get; set; }

        public int? AvailableQuantity { get; set; }

        public ICollection<ServicesMedia> Media { get; set; } = new List<ServicesMedia>();

    }
}
