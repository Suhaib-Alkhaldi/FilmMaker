namespace FilmMaker.DTO.Lookup.Location
{
    public class LocationFilterDto
    {
        public string? City { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
