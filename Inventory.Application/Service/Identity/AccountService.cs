using Inventory.Application.DTO.Request.ActivityTracker;
using Inventory.Application.DTO.Request.Identity;
using Inventory.Application.DTO.Response;
using Inventory.Application.DTO.Response.ActivityTracker;
using Inventory.Application.DTO.Response.Identity;
using Inventory.Application.Interface.Identity;

namespace Inventory.Application.Service.Identity
{
    public class AccountService : IAccountService
    {
        private readonly IAccount _account;

        public AccountService(IAccount account)
        {
            _account = account;
        }

        public async Task<ServiceResponse> CreateUserAsync(CreateUserRequestDTO model) =>
            await _account.CreateUserAsync(model);

        public async Task<ServiceResponse> LoginAsync(LoginUserRequestDTO model) => await _account.LoginAsync(model);

        public async Task<IEnumerable<GetUserWithClaimResponseDTO>> GetUsersWithClaimsAsync() =>
            await _account.GetUsersWithClaimsAsync();

        public async Task SetUpAsync() => await _account.SetUpAsync();

        public async Task<ServiceResponse> UpdateUserAsync(ChangeUserClaimRequestDTO model) =>
            await _account.UpdateUserAsync(model);

        public async Task SaveActivityAsync(ActivityTrackerRequestDTO model) => await _account.SaveActivityAsync(model);

        public async Task<IEnumerable<ActivityTrackerResponseDTO>> GetActivitiesAsync() =>
            await _account.GetActivitiesAsync();

        public async Task<IEnumerable<IGrouping<DateTimeOffset, ActivityTrackerResponseDTO>>> GroupActivitiesAsync() =>
            (await _account.GetActivitiesAsync()).GroupBy(e=>e.Created).AsEnumerable();
    }
}
