namespace FilmMaker.DTOs.Media
{
    
    public class UploadLocationMediaDto
    {
        public List<IFormFile> Files { get; set; } = new();
    }
 
    
    public class UploadServiceMediaDto
    {
        public List<IFormFile> Files { get; set; } = new();
    }
 
   
    public class MediaUploadResultDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
    }
}