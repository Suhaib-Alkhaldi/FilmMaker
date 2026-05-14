using FilmMaker.Services.Service;

namespace FilmMaker.DTO.Location.Response
{
    public class ArchivedLocationResponseDto : LocationResponseDto
    {
        public string? ArchiveReason { get; set; }
        public DateTime? ArchivedAt { get; set; }
    }
}
