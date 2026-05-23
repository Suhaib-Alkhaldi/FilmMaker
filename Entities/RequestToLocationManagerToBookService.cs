using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class RequestToLocationManagerToBookService : SharedEntity
    {

        public int ProductionCompanyId { get; set; }

        [ForeignKey("ProductionCompanyId")]
        public ProductionCompanyProfile ProductionCompany { get; set; }
        public int ServiceTypeId { get; set; }

        [ForeignKey("ServiceTypeId")]
        public LookupItem ServiceType { get; set; }

        public int LocationBookingId { get; set; }

        [ForeignKey("LocationBookingId")]
        public LocationBookingRequest LocationBooking { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Notes { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? LocationOnGoogleMaps { get; set; } = string.Empty;

    }
}
