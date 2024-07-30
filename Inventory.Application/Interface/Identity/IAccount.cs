
using Inventory.Application.DTO.Request.ActivityTracker;
using Inventory.Application.DTO.Request.Identity;
using Inventory.Application.DTO.Response;
using Inventory.Application.DTO.Response.ActivityTracker;
using Inventory.Application.DTO.Response.Identity;

namespace Inventory.Application.Interface.Identity
{
    public interface IAccount
    {
        Task<ServiceResponse> LoginAsync(LoginUserRequestDTO model);
        Task<ServiceResponse> CreateUserAsync(CreateUserRequestDTO model);
        Task<IEnumerable<GetUserWithClaimResponseDTO>> GetUsersWithClaimsAsync();
        Task SetUpAsync();
        Task<ServiceResponse> UpdateUserAsync(ChangeUserClaimRequestDTO model);
        Task SaveActivityAsync(ActivityTrackerRequestDTO model);
        Task<IEnumerable<ActivityTrackerResponseDTO>> GetActivitiesAsync();
        Task<IEnumerable<IGrouping<DateTimeOffset, ActivityTrackerResponseDTO>>> GroupActivitiesAsync();
    }
}
