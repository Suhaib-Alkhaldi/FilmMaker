using System.ComponentModel.DataAnnotations;

namespace FilmMaker.DTO.LocationBooking
{
    public class UpdateBookingRequestDto
    {
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string? Message { get; set; }
    }
}
