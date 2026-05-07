using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.Entities
{
    public class LocationBookingRequest : SharedEntity
    {
        public int BookingRequestStatusId { get; set; }
        [ForeignKey("BookingRequestStatusId")]
        public LockupItem BookingStatus { get; set; }
        public DateTime ShootingDate { get; set; }
        public string RequestDetails { get; set; }
        public int LocationOwnerId { get; set; }
        [ForeignKey("LocationOwnerId")]
        public LocationOwnerProfile LocationOwner {  get; set; }
        public int LocationManagerId { get; set; }
        [ForeignKey("LocationManagerId")]
        public LocationManagerProfile LocationManager {  get; set; }
        public int ProductionCompanyId { get; set; }
        [ForeignKey("ProductionCompanyId")]
        public ProductionCompanyProfile ProductionCompany {  get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsCancelled { get; set; }
    }
}
