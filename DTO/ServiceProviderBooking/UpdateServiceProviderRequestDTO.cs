namespace FilmMaker.DTO.ServiceProviderBooking
{
    public class UpdateServiceProviderRequestDTO
    {
        public int RequestId { get; set; }

        public int? ServiceProviderId { get; set; }

        public string? MessageToProvider { get; set; }

        public List<UpdateServiceProviderRequestItemDTO>? Items { get; set; } 
    }

    public class UpdateServiceProviderRequestItemDTO
    {
        public int? RequestToLocationManagerItemId { get; set; }

        public int? ServiceId { get; set; }
    }
}
