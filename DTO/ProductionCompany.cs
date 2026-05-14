using System.ComponentModel.DataAnnotations;

namespace FilmMaker.DTOs.ProductionCompany
{
    

    public class RegisterProductionCompanyRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(8)]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; }

        [Required, MaxLength(150)]
        public string CompanyName { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string City { get; set; }

        public List<int>? ProductionTypeIds { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }
    }

    public class RegisterProductionCompanyResponse
    {
        public int UserId { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
    }

   

    public class LoginRequest
    {
        [MaxLength(200)]
        public string? Email { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public DateTime TokenExpiry { get; set; }
    }

    
    public class ProductionCompanyProfileResponse
    {
        public int UserId { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string? Bio { get; set; }
        public List<ProductionTypeDto> ProductionTypes { get; set; }
        public DateTime RegisterDate { get; set; }
    }

    public class ProductionTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

   

    public class UpdateProductionCompanyProfileRequest
    {
        [Required, MaxLength(150)]
        public string CompanyName { get; set; }

        [Required, Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string Country { get; set; }

        [Required]
        public string City { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        public List<int> ProductionTypeIds { get; set; } = new();
    }

    public class UpdateProductionCompanyProfileResponse
    {
        public string CompanyName { get; set; }
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string? Bio { get; set; }
        public List<ProductionTypeDto> ProductionTypes { get; set; }
    }



    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required, MinLength(8)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmNewPassword { get; set; }
    }

    public class ChangePasswordResponse
    {
        public string NewToken { get; set; }
        public DateTime TokenExpiry { get; set; }
    }
}