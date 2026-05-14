using System.ComponentModel.DataAnnotations;

namespace FilmMaker.DTO.LocationManager
{
    public class CreateBookingRequestDto
    {
        [Required]
        public int LocationId { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }
        [Required]
        public DateTime EndDateTime { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }
    }
}
