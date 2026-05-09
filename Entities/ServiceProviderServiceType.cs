using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class ServiceProviderServiceType : SharedEntity
    {
        public int ServiceProviderId { get; set; }
        [ForeignKey("ServiceProviderId")]
        public ServiceProviderProfile ServiceProviderProfile { get; set; } = null!;
        public int? ServiceTypeId { get; set; }

        [ForeignKey("ServiceTypeId")]
        public LockupItem? ServiceType { get; set; }
        public string? CustomServiceTypeName { get; set; }
        public bool IsCustom { get; set; }
    }
}
