using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class PreviousLocation : SharedEntity
    {
        public int LocationId { get; set; }
        [ForeignKey("LocationId")]
        public Location Location { get; set; }

        public int LocationStatusId { get; set; }
        [ForeignKey("LocationStatusId")]
        public LockupItem LocationStatus { get; set; }

    }
}
