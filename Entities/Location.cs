using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class Location : SharedEntity
    {
        public string LocationName { get; set; } = string.Empty;
        public string LocationDescription { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Address { get; set; }
        public decimal DailyPrice { get; set; }
        public int LocationOwnerId { get; set; }
        [ForeignKey("LocationOwnerId")]
        public LocationOwnerProfile LocationOwner { get; set; } = null!;
        public int? LocationManagerId { get; set; }
        [ForeignKey("LocationManagerId")]
        public LocationManagerProfile? LocationManager { get; set; }
        public int LocationStatusId { get; set; }

        [ForeignKey("LocationStatusId")]
        public LockupItem LocationStatus { get; set; } = null!;
        public string? LocationOnGoogleMaps { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public ICollection<LocationMedia> Media { get; set; }
        public ICollection<LocationTermsOfUse> TermsOfUse { get; set; }
        public ICollection<LocationArchiveHistory> ArchiveHistories { get; set; }
    }
}
