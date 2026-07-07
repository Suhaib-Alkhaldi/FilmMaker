using System.ComponentModel.DataAnnotations;

namespace FilmMaker.DTO.RequestToLocationManagerToBookService
{
    public class ReadRequestToLocationManagerToBookServiceDTO
    {
        public int Id { get; set; }

        public int ProductionCompanyId { get; set; }
        public string ProductionCompanyName { get; set; } = string.Empty;

        public int LocationManagerId { get; set; }
        public string LocationManagerName { get; set; } = string.Empty;

        public int LocationBookingId { get; set; }

        public int? BookingLocationManagerId { get; set; }
        public string? BookingLocationManagerName { get; set; }

        public string? GeneralNotes { get; set; }

        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public List<ReadRequestToLocationManagerToBookServiceItemDTO> Items { get; set; } = new();
    }

    public class ReadRequestToLocationManagerToBookServiceItemDTO
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Service Type ID is required.")]   
        [MinLength(1, ErrorMessage = "At least one service type must be selected.")]
        public int? ServiceTypeId { get; set; }

        public string? ServiceTypeName { get; set; }

       // public string? CustomServiceType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Details { get; set; }

        public int? Quantity { get; set; }
    }
}
