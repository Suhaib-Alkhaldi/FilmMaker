namespace FilmMaker.DTO.RequestToLocationManagerToBookService
{
    public class ReadRequestToLocationManagerToBookServiceDTO
    {
        public int Id { get; set; }
        public string ProductionCompany { get; set; }

        public string ServiceType { get; set; }

        public string LocationManager { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Notes { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? LocationOnGoogleMaps { get; set; } = string.Empty;
    }
}
