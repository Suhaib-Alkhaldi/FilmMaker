using FilmMaker.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.DTO.Profile.Request
{
    public class UpdateLocationOwnerProfileRequestDto
    {
        public string? Name { get; set; }

        public string? PhoneNumber { get; set; }

        public string? IBAN { get; set; }
    }
}
