using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class ServicesMedia :  SharedEntity
    {
        public int ServicesProvidedId { get; set; }

        [ForeignKey("ServicesProvidedId")]
        public ServicesProvided ServicesProvided { get; set; } = null!;

        public int MediaId { get; set; }

        [ForeignKey("MediaId")]
        public Media Media { get; set; } = null!;
    }
}
