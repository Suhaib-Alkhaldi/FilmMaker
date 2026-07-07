using System.ComponentModel.DataAnnotations;

namespace FilmMaker.DTO.Profile.Request
{
    public class UpdateServiceProviderProfileRequestDto
    {
        public string? Name { get; set; }

        public string? PhoneNumber { get; set; }

        public string? IBAN { get; set; }

        [Required(ErrorMessage = "Service Type ID is required.")]   
        [MinLength(1, ErrorMessage = "At least one service type must be selected.")]
        public List<int>? ServiceTypeIds { get; set; }

        //public List<string>? CustomServiceTypes { get; set; }

        public List<int>? CitiesIds { get; set; }
    }
}
