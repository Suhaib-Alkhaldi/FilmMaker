using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class LocationTermsOfUse : SharedEntity
    {
        public int LocationId { get; set; }

        [ForeignKey("LocationId")]
        public virtual Location Location { get; set; }
        public string Term { get; set; } = string.Empty;
    }
}
