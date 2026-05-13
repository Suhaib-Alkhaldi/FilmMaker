using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class ServicesProvided : SharedEntity
    {
        public string ServiceName { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "smallmoney")]
        public decimal Price { get; set; }

        public int ServiceTypeId { get; set; }

        [ForeignKey("ServiceTypeId")]
        public LookupItem ServiceType { get; set; }

        public int ServiceProviderId { get; set; }

        [ForeignKey("ServiceProviderId")]
        public ServiceProviderProfile ServiceProvider { get; set; }
    }
}
