
using System.ComponentModel.DataAnnotations;

namespace Inventory.Application.DTO.Request.Identity
{
    public class LoginUserRequestDTO
    {
        [EmailAddress]
        public string Email { get; set;}
        [Required]
        [MinLength(8), MaxLength(100)]
        public string Password { get; set;}
    }
}
