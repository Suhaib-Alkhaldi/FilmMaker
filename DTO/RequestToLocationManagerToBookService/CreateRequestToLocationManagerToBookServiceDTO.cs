using FilmMaker.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.DTO.RequestToLocationManagerToBookService
{
    public class CreateRequestToLocationManagerToBookServiceDTO
    {

        public int ServiceTypeId { get; set; }


        public int LocationBookingId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Notes { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? LocationOnGoogleMaps { get; set; } = string.Empty;
    }
}
