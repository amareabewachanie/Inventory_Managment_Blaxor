using System.Security.Claims;
using Inventory.Application.Extensions.Identity;
using Microsoft.AspNetCore.Identity;

namespace Inventory.Web.Components.Layout.Identity
{
    internal static class SignOutEndpoint
    {
        public static IEndpointConventionBuilder MapSignOutEndpoint(this IEndpointRouteBuilder endpoint)
        {
            ArgumentNullException.ThrowIfNull(endpoint);
            var accountGroup = endpoint.MapGroup("/Account");
            accountGroup.MapPost("/Logout",
                async (ClaimsPrincipal user, SignInManager<ApplicationUser> signInManager) =>
                {
                    await signInManager.SignOutAsync();
                    return TypedResults.LocalRedirect("/Account/Login");
                });
            return accountGroup;
        }
    }
}
