using FilmMaker.Common;
using FilmMaker.DTO.RequestToLocationManagerToBookService;

namespace FilmMaker.Services.Interface
{
    public interface IRequestToLocationManagerToBookServiceService
    {
        Task<ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>> CreateServiceRequestToLocationManager(CreateRequestToLocationManagerToBookServiceDTO request,int currentUserId);
        Task<ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>> UpdateServiceRequestToLocationManager(UpdateRequestToLocationManagerToBookServiceDTO request,int currentUserId);
        Task<ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>> GetMySentServiceRequestsToLocationManager(int currentUserId);
        Task<ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>> GetMyServiceRequestToLocationManagerById(int requestId,int currentUserId);
        Task<ApiResponse<bool>> CancelServiceRequestToLocationManager(int requestId,int currentUserId);

        Task<ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>> GetMyReceivedServiceRequestsToLocationManager(int currentUserId);

        Task<ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>> GetReceivedServiceRequestToLocationManagerById(int requestId,int currentUserId);

        Task<ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>> RespondToServiceRequestToLocationManager(RespondRequestToLocationManagerToBookServiceDTO request,int currentUserId);
    }
}
