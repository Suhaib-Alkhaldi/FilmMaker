using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class LocationManagerProfile : SharedEntity
    {
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? Description { get; set; } = string.Empty;
        public decimal? CommissionRate { get; set; }
        public List<string>? PreviousProject {  get; set; }
        public List<string>? Cities {  get; set; }
    }
}
