using System.Security.Claims;
using Inventory.Application.Extensions.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Inventory.Web.Components.Layout.Identity
{
    public class IdentityRevalidatingAuthStateProvider : RevalidatingServerAuthenticationStateProvider
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IOptions<IdentityOptions> _options;

        public IdentityRevalidatingAuthStateProvider(ILoggerFactory loggerFactory,
            IServiceScopeFactory serviceScopeFactory, IOptions<IdentityOptions> options):base(loggerFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _options = options;
        }

        protected async override Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
        {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            return await ValidateSecurityStampAsync(userManager, authenticationState.User);
        }

        protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(20);

        private async Task<bool> ValidateSecurityStampAsync(UserManager<ApplicationUser> userManager, ClaimsPrincipal principal)
        {
            var user = await userManager.GetUserAsync(principal);
            if (user is null)
            {
                return false;
            }

            if (!userManager.SupportsUserSecurityStamp)
            {
                return true;
            }

            var principalStamp = principal.FindFirstValue(_options.Value.ClaimsIdentity.SecurityStampClaimType);
            var userStamp = await userManager.GetSecurityStampAsync(user);
            return principalStamp == userStamp;
        }
    }
}
