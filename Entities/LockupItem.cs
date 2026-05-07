using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class LockupItem : SharedEntity
    {
        public string Name { get; set; }
        public int LockupCategoryId { get; set; }
        [ForeignKey("LockupCategoryId")]
        public LockupCategory LockupCategory { get; set; }
    }
}
