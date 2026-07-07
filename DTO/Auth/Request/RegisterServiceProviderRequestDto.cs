using System.ComponentModel.DataAnnotations;

namespace FilmMaker.DTO.Auth.Request
{
    public class RegisterServiceProviderRequestDto:BaseRegisterDto
    {
        [Required(ErrorMessage = "Service Type ID is required.")]   
        [MinLength(1, ErrorMessage = "At least one service type must be selected.")]
        public List<int> ServiceTypeIds { get; set; } 
        public List<int> CitiesIds { get; set; }
       // public List<string> CustomServiceTypes { get; set; } 
    }
}
