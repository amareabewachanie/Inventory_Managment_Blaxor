
using Inventory.Application.Extensions.Identity;
using Inventory.Application.Interface.Identity;
using Inventory.Infrastructure.DataAcsess;
using Inventory.Infrastructure.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventory.Infrastructure.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(context => context.UseSqlServer(configuration.GetConnectionString("Default")), ServiceLifetime.Scoped);
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            }).AddIdentityCookies();
            services.AddIdentityCore<ApplicationUser>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();
            services.AddAuthorizationBuilder()
                .AddPolicy("AdministrationPolicy", adp =>
                {
                    adp.RequireAuthenticatedUser();
                    adp.RequireRole("Admin", "Manager");
                })
               .AddPolicy("UserPolicy", udp =>
               {
                   udp.RequireAuthenticatedUser();
                   udp.RequireRole("User");
               });
            services.AddScoped<IAccount,Account>();
            return services;
        }
    }
}
