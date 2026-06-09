using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class RequestToLocationManagerToBookService : SharedEntity
    {

        public int ProductionCompanyId { get; set; }

        [ForeignKey("ProductionCompanyId")]
        public ProductionCompanyProfile ProductionCompany { get; set; } = null!;

        public int LocationManagerId { get; set; }

        [ForeignKey("LocationManagerId")]
        public LocationManagerProfile LocationManager { get; set; } = null!;

        public int LocationBookingId { get; set; }

        [ForeignKey("LocationBookingId")]
        public LocationBookingRequest LocationBooking { get; set; } = null!;
        public string? GeneralNotes { get; set; }
        public int StatusId { get; set; }

        [ForeignKey("StatusId")]
        public LookupItem Status { get; set; } = null!;

        public string? LocationManagerResponse { get; set; }

        public DateTime? RespondedAtUtc { get; set; }

        public int? RespondedByUserId { get; set; }
        public ICollection<RequestToLocationManagerToBookServiceItem> Items { get; set; }
            = new List<RequestToLocationManagerToBookServiceItem>();

    }
}
