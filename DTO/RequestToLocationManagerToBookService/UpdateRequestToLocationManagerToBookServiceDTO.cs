using System.ComponentModel.DataAnnotations;

namespace FilmMaker.DTO.RequestToLocationManagerToBookService
{
    public class UpdateRequestToLocationManagerToBookServiceDTO
    {
        public int RequestId { get; set; }

        public int? LocationBookingId { get; set; }
        public int? LocationManagerId { get; set; }

        public string? GeneralNotes { get; set; }

        public List<UpdateRequestToLocationManagerToBookServiceItemDTO>? Items { get; set; }
    }

    public class UpdateRequestToLocationManagerToBookServiceItemDTO
    {
        [Required(ErrorMessage = "Service Type ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Service Type ID.")]
        
        public int? ServiceTypeId { get; set; }

       // public string? CustomServiceType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? Details { get; set; }

        public int? Quantity { get; set; }
    }
}
