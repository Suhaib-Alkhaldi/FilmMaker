using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class LocationScoutingRequest : SharedEntity
    {
        public int ProductionCompanyId { get; set; }

        [ForeignKey("ProductionCompanyId")]
        public ProductionCompanyProfile ProductionCompany { get; set; } = null!;

        public int LocationManagerId { get; set; }

        [ForeignKey("LocationManagerId")]
        public LocationManagerProfile LocationManager { get; set; } = null!;
        public int? CityId { get; set; }

        [ForeignKey("CityId")]
        public LookupItem? City { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Requirements { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public decimal? MinBudget { get; set; }

        public decimal? MaxBudget { get; set; }

        public int StatusId { get; set; }

        [ForeignKey("StatusId")]
        public LookupItem Status { get; set; } = null!;

        public string? LocationManagerResponse { get; set; }

        public DateTime? RespondedAtUtc { get; set; }
    }
}
