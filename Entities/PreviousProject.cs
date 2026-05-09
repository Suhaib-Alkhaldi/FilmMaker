using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class PreviousProject : SharedEntity
    {
        public int LocationManagerProfileId { get; set; }

        [ForeignKey("LocationManagerProfileId")]
        public LocationManagerProfile LocationManagerProfile { get; set; } = null!;
        public string ProjectName { get; set; } = string.Empty;
    }
}
