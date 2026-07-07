using System.ComponentModel.DataAnnotations;

namespace FilmMaker.DTO.ServiceProvider
{
    public class GetServiceDTO
    {
        public int Id { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public int? AvailableQuantity { get; set; }

        public decimal Price { get; set; }
        [Required(ErrorMessage = "Service Type ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Service Type ID.")]
        public int? ServiceTypeId { get; set; }

        public string ServiceTypeName { get; set; } = string.Empty;

        //public string? CustomServiceType { get; set; }

        //public bool IsCustomServiceType { get; set; }

        public int ServiceProviderId { get; set; }

        public string ServiceProviderName { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }
    }
}
