namespace FilmMaker.DTO.ServiceProvider
{
    public class CreateServiceDTO
    {
        public string ServiceName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int ServiceTypeId { get; set; }

    }
}
