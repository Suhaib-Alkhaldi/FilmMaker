using FilmMaker.Common;
using FilmMaker.DTO.ServiceProviderBooking;

namespace FilmMaker.Services.Interface
{
    public interface IServiceProviderRequestService
    {
        Task<ApiResponse<GetServiceProviderRequestDTO>> SendServiceRequestToProvider(SendServiceRequestToProviderDTO request,int currentUserId);
        Task<ApiResponse<List<GetServiceProviderRequestDTO>>> GetMySentServiceProviderRequests(int currentUserId);

        Task<ApiResponse<GetServiceProviderRequestDTO>> GetMySentServiceProviderRequestById(int requestId,int currentUserId);

        Task<ApiResponse<GetServiceProviderRequestDTO>> UpdateServiceProviderRequest(UpdateServiceProviderRequestDTO request,int currentUserId);

        Task<ApiResponse<bool>> CancelServiceProviderRequest(int requestId,int currentUserId);

        Task<ApiResponse<List<GetServiceProviderRequestDTO>>> GetMyReceivedServiceProviderRequests(int currentUserId);

        Task<ApiResponse<GetServiceProviderRequestDTO>> GetMyReceivedServiceProviderRequestById(int requestId,int currentUserId);

        Task<ApiResponse<GetServiceProviderRequestDTO>> RespondToServiceProviderRequest(RespondServiceProviderRequestDTO request,int currentUserId);
    }
}
