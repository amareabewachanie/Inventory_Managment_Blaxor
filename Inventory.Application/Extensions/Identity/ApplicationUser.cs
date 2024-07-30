using Microsoft.AspNetCore.Identity;

namespace Inventory.Application.Extensions.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
