using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class ProductionCompanyProfile : SharedEntity 
    {
        public string CompanyName { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public List<LockupItem>? ProductionTypes { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string? Bio {  get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public DateTime RegisterDate { get; set; } = DateTime.UtcNow;

    }
}
