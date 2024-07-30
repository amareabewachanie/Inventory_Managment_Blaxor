using System.ComponentModel.DataAnnotations;

namespace Inventory.Application.DTO.Response.ActivityTracker
{
    public class ActivityTrackerResponseDTO : BaseActivityTracker
    {
        [Required]
        public string UserName { get; set; }
    }
}
