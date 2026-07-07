using System.ComponentModel.DataAnnotations;

namespace FilmMaker.DTO.ServiceProvider
{
    public class CreateServiceDTO
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? AvailableQuantity { get; set; }
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "Service Type ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Service Type ID.")]
        public int? ServiceTypeId { get; set; }
       // public string? CustomServiceType { get; set; }
        public List<int> MediaIds { get; set; } = new();

    }
}
