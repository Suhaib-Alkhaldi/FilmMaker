using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class LocationManagementRequest : SharedEntity
    {
        public int LocationId { get; set; }

        [ForeignKey(nameof(LocationId))]
        public Location Location { get; set; } = null!;

        public int LocationManagerProfileId { get; set; }

        [ForeignKey(nameof(LocationManagerProfileId))]
        public LocationManagerProfile LocationManagerProfile { get; set; } = null!;

        public string? Message { get; set; }

        public string Status { get; set; } = "Pending";
    }
}
