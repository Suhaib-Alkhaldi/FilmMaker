using FilmMaker.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.DTO.RequestToLocationManagerToBookService
{
    public class CreateRequestToLocationManagerToBookServiceDTO
    {
        public int LocationBookingId { get; set; }
        public int? LocationManagerId { get; set; }
        public string? GeneralNotes { get; set; }

        public List<CreateRequestToLocationManagerToBookServiceItemDTO> Items { get; set; }
    }

    public class CreateRequestToLocationManagerToBookServiceItemDTO
    {
        public int? ServiceTypeId { get; set; }

        public string? CustomServiceType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Details { get; set; }

        public int? Quantity { get; set; }
    }
}
