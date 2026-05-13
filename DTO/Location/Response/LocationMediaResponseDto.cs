namespace FilmMaker.DTO.Location.Response
{
    public class LocationMediaResponseDto
    {
        public int MediaId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string FileUrl { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long SizeInBytes { get; set; }

        public string MediaType { get; set; } = string.Empty;
    }
}
