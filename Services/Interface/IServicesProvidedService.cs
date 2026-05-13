using FilmMaker.Common;
using FilmMaker.DTO.ServiceProvider;

namespace FilmMaker.Services.Interface
{
    public interface IServicesProvidedService
    {
        Task<ApiResponse<bool>> AddService(CreateServiceDTO serviceDto, int currentUserId);

        Task<ApiResponse<bool>> UpdateService(UpdateServiceDTO serviceDto, int currentUserId);

        Task<ApiResponse<bool>> DeleteService(int serviceId, int currentUserId);
        Task<ApiResponse<bool>> RestoreDeletedService(int serviceId, int currentUserId);
        Task<ApiResponse<GetServiceDTO?>> GetServiceById(int serviceId);

        Task<ApiResponse<List<GetServiceDTO>>> GetAllServices();

        Task<ApiResponse<List<GetServiceDTO>>> GetMyServicesByProvider(int currentUserId);
        Task<ApiResponse<List<GetServiceDTO>>> GetServicesByProvider(int currentUserId);
        Task<ApiResponse<List<GetServiceDTO>>> GetServicesByServiceType(int ServiceType);

        Task<ApiResponse<bool>> SetServiceActive(int serviceId, int currentUserId);
        Task<ApiResponse<bool>> SetServiceInactive(int serviceId, int currentUserId);
    }
}
