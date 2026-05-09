using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class LocationTermsOfUse : SharedEntity
    {
        public int LocationId { get; set; }

        [ForeignKey("LocationId")]
        public Location Location { get; set; } = null!;
        public string TermText { get; set; } = string.Empty;
        public int Order { get; set; }
    }
}
