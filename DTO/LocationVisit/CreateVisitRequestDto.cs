using System.ComponentModel.DataAnnotations;
namespace FilmMaker.DTO.LocationVisit
{
    public class CreateVisitRequestDto
    {
        [Required]
        public int LocationId { get; set; }
        [Required]
        public DateTime RequestedVisitDate { get; set; }
        public string? RequestMessage { get; set; }
    }
}