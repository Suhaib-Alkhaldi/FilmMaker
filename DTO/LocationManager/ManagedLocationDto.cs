namespace FilmMaker.DTO.LocationManager
{
    public class ManagedLocationDto
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }
}
