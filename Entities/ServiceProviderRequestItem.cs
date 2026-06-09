using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class ServiceProviderRequestItem : SharedEntity
    {
        public int ServiceProviderRequestId { get; set; }

        [ForeignKey("ServiceProviderRequestId")]
        public ServiceProviderRequest ServiceProviderRequest { get; set; } = null!;

        public int RequestToLocationManagerToBookServiceItemId { get; set; }

        [ForeignKey("RequestToLocationManagerToBookServiceItemId")]
        public RequestToLocationManagerToBookServiceItem RequestToLocationManagerToBookServiceItem { get; set; } = null!;

        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public ServicesProvided Service { get; set; } = null!;
    }
}
