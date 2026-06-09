using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class ServiceProviderRequest : SharedEntity
    {
        public int RequestToLocationManagerToBookServiceId { get; set; }

        [ForeignKey("RequestToLocationManagerToBookServiceId")]
        public RequestToLocationManagerToBookService RequestToLocationManagerToBookService { get; set; } = null!;

        public int LocationManagerId { get; set; }

        [ForeignKey("LocationManagerId")]
        public LocationManagerProfile LocationManager { get; set; } = null!;

        public int ServiceProviderId { get; set; }

        [ForeignKey("ServiceProviderId")]
        public ServiceProviderProfile ServiceProvider { get; set; } = null!;

        public string? MessageToProvider { get; set; }

        public int StatusId { get; set; }

        [ForeignKey("StatusId")]
        public LookupItem Status { get; set; } = null!;

        public string? ServiceProviderResponse { get; set; }

        public DateTime? RespondedAtUtc { get; set; }

        public int? RespondedByUserId { get; set; }

        public ICollection<ServiceProviderRequestItem> Items { get; set; }
            = new List<ServiceProviderRequestItem>();
    }
}
