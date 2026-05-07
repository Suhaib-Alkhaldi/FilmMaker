using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class Location : SharedEntity
    {
        public string LocationName { get; set; } = string.Empty;
        public string LocationDescription { get; set; } = string.Empty;
        public decimal DailyPrice { get; set; }
        public int LocationOwnerId { get; set; }

        [ForeignKey("LocationOwnerId")]
        public LocationOwnerProfile LocationOwner { get; set; }
        public int? LocationManagerId { get; set; }
        [ForeignKey("LocationManagerId")]
        public virtual LocationManagerProfile LocationManager { get; set; }
        public int LocationStatusId { get; set; }
        [ForeignKey("LocationStatusId")]
        public LockupItem LocationStatus { get; set; }
        public string LocationOnGoogleMaps { get; set; }
        public bool IsArchived { get; set; } = false;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public ICollection<LocationBookingRequest> LocationBookingRequests { get; set; }

        public ICollection<LocationTermsOfUse> LocationTermsOfUses { get; set; }
    }
}
