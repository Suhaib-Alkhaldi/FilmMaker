using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class LocationMedia : SharedEntity
    {
        public int LocationId { get; set; }

        [ForeignKey("LocationId")]
        public Location Location { get; set; } = null!;

        public int MediaId { get; set; }

        [ForeignKey("MediaId")]
        public Media Media { get; set; } = null!;
    }
}
