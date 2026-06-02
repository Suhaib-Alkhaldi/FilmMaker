using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class ServiceProviderCities : SharedEntity
    {
        public int ServiceProviderId { get; set; }

        [ForeignKey("ServiceProviderId")]
        public ServiceProviderProfile ServiceProviderProfile { get; set; } = null!;

        public int CityId { get; set; }

        [ForeignKey("CityId")]
        public LookupItem City { get; set; } = null!;

    }
}
