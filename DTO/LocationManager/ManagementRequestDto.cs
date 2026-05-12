using System.ComponentModel.DataAnnotations;

namespace FilmMaker.DTO.LocationManager
{
    public class ManagementRequestDto
    {
        [Required]
        public int LocationId { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }
    }
}
