namespace FilmMaker.DTO.ServiceProvider
{
    public class UpdateServiceDTO
    {
        public int Id { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int ServiceTypeId { get; set; }

    }
}
