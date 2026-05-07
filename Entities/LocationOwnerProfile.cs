using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class LocationOwnerProfile : SharedEntity
    {
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public DateTime RegisterDate { get; set; } = DateTime.Now;
    }
}
