using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class User : SharedEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string IBAN { get; set; }
        public int RoleId { get; set; }
        [ForeignKey("RoleId")]
        public Role Roles { get; set; }
        public LocationOwnerProfile? LocationOwnerProfile { get; set; }
        public LocationManagerProfile? LocationManagerProfile { get; set; }
        public ProductionCompanyProfile? ProductionCompanyProfile { get; set; }
        public ServiceProviderProfile? ServiceProviderProfile { get; set; }
    }
}
