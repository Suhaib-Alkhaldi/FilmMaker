using System.ComponentModel.DataAnnotations.Schema;
 
namespace FilmMaker.Entities
{
    public class ServiceMedia : SharedEntity
    {
        public int UploadedByUserId { get; set; }
 
        [ForeignKey("UploadedByUserId")]
        public User UploadedByUser { get; set; } = null!;
 
        public int? ServiceId { get; set; }
 
        //[ForeignKey("ServiceId")]
       // public Service? Service { get; set; }
 
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
        public int MediaTypeId { get; set; }
 
        [ForeignKey("MediaTypeId")]
        public LockupItem MediaType { get; set; } = null!;
    }
}