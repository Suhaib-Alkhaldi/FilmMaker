using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class LocationArchiveHistory : SharedEntity
    {
        public int LocationId { get; set; }

        [ForeignKey("LocationId")]
        public Location Location { get; set; } = null!;

        public int ArchivedByUserId { get; set; }

        [ForeignKey("ArchivedByUserId")]
        public User ArchivedByUser { get; set; } = null!;

        public DateTime ArchivedAt { get; set; } = DateTime.UtcNow;

        public string? Reason { get; set; }

        public bool IsRestored { get; set; }

        public DateTime? RestoredAt { get; set; }

        public int? RestoredByUserId { get; set; }

        [ForeignKey("RestoredByUserId")]
        public User? RestoredByUser { get; set; }

    }
}
