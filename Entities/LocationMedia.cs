using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class LocationMedia : SharedEntity
    {
        public int UploadedByUserId { get; set; }

        [ForeignKey("UploadedByUserId")]
        public User UploadedByUser { get; set; }
        public int? LocationId { get; set; }

        [ForeignKey("LocationId")]
        public Location? Location { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
        public int MediaTypeId { get; set; }

        [ForeignKey("MediaTypeId")]
        public LockupItem MediaType { get; set; } = null!;
    }
}
