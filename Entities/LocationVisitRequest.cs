using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class LocationVisitRequest : SharedEntity
    {
        public int LocationId { get; set; }

        [ForeignKey("LocationId")]
        public Location Location { get; set; } = null!;
        public int LocationOwnerId { get; set; }

        [ForeignKey("LocationOwnerId")]
        public LocationOwnerProfile LocationOwner { get; set; } = null!;
        public int LocationManagerId { get; set; }

        [ForeignKey("LocationManagerId")]
        public LocationManagerProfile LocationManager { get; set; } = null!;
        public DateTime RequestedVisitDateUtc { get; set; }
        public string? RequestMessage { get; set; }
        public int VisitStatusId { get; set; }

        [ForeignKey("VisitStatusId")]
        public LookupItem VisitStatus { get; set; } = null!;
        public string? OwnerResponseMessage { get; set; }
        public DateTime? RespondedAtUtc { get; set; }
        public int? RespondedByUserId { get; set; }

        [ForeignKey("RespondedByUserId")]
        public User? RespondedByUser { get; set; }
    }
}
