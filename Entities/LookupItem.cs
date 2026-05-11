using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class LookupItem : SharedEntity
    {
        public int Id { get; set; }
        public int LookupCategoryId { get; set; }
        public string Name { get; set; }

        [ForeignKey("LookupCategoryId")]
        public LookupCategory LookupCategory { get; set; }
    }
}
