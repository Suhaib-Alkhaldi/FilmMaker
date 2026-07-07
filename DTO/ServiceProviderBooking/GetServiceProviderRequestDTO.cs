using System.ComponentModel.DataAnnotations;

namespace FilmMaker.DTO.ServiceProviderBooking
{
    public class GetServiceProviderRequestDTO
    {
        public int Id { get; set; }

        public int ServiceRequestToLocationManagerId { get; set; }

        public int LocationManagerId { get; set; }

        public string LocationManagerName { get; set; } = string.Empty;

        public int ServiceProviderId { get; set; }

        public string ServiceProviderName { get; set; } = string.Empty;

        public string? MessageToProvider { get; set; }

        public int StatusId { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public string? ServiceProviderResponse { get; set; }

        public DateTime? RespondedAtUtc { get; set; }

        public int? RespondedByUserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public List<GetServiceProviderRequestItemDTO> Items { get; set; } = new();
    }

    public class GetServiceProviderRequestItemDTO
    {
        public int Id { get; set; }

        public int RequestToLocationManagerItemId { get; set; }

        public int ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public int? ServiceTypeId { get; set; }

        [Required(ErrorMessage = "Service Type ID is required.")]   
        [MinLength(1, ErrorMessage = "At least one service type must be selected.")]
        public string? ServiceTypeName { get; set; }

        //public string? CustomServiceType { get; set; }

        public int? RequestedQuantity { get; set; }

        public int? AvailableQuantity { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Details { get; set; }
    }
}
