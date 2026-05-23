using FilmMaker.Common;
using FilmMaker.DTO.RequestToLocationManagerToBookService;

namespace FilmMaker.Services.Interface
{
    public interface IRequestToLocationManagerToBookServiceService
    {
        Task<ApiResponse<CreateRequestToLocationManagerToBookServiceDTO>> CreateServiceRequestToLocationManager(CreateRequestToLocationManagerToBookServiceDTO request, int currentUserId);
        Task<ApiResponse<UpdateRequestToLocationManagerToBookServiceDTO>> UpdateServiceRequestToLocationManager(UpdateRequestToLocationManagerToBookServiceDTO request,int currentUserId);
        Task<ApiResponse<bool>> DeleteServiceRequestToLocationManager(int currentUserId, int Id);
        Task<ApiResponse<bool>> RestoreDeletedServiceRequestToLocationManager(int currentUserId, int Id);
        Task<ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>> ReadServiceRequestToLocationManager(int currentUserId, bool IsDeleted);

        Task<ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>> ReadServiceRequestToLocationManagerById(int currentUserId, bool IsDeleted , int Id);

        Task<ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>> ReadServiceRequestToLocationManagerByLocationBookingId(int currentUserId,int LocationBookingId, bool IsDeleted);

    }
}
