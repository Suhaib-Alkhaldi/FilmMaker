using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class LocationManagerCity : SharedEntity
    {
        public int LocationManagerProfileId { get; set; }

        [ForeignKey("LocationManagerProfileId")]
        public LocationManagerProfile LocationManagerProfile { get; set; } = null!;

        public int CityId { get; set; }

        [ForeignKey("CityId")]
        public LockupItem City { get; set; } = null!;
    }
}
