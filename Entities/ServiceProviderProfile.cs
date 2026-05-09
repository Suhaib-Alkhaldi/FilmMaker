using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class ServiceProviderProfile : SharedEntity
    {
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }
        public DateTime RegisterDate { get; set; } = DateTime.Now;
        public ICollection<ServiceProviderServiceType> ServiceTypes { get; set; }
    }
}
