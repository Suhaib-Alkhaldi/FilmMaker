namespace FilmMaker.DTO.ServiceProviderBooking
{
    public class SendServiceRequestToProviderDTO
    {
        public int ServiceRequestToLocationManagerId { get; set; }

        public int ServiceProviderId { get; set; }

        public string? MessageToProvider { get; set; }

        public List<SendServiceRequestToProviderItemDTO> Items { get; set; } = new();
    }

    public class SendServiceRequestToProviderItemDTO
    {
        public int RequestToLocationManagerItemId { get; set; }

        public int ServiceId { get; set; }
    }
}
