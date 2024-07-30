using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventory.Domain.Common;

namespace Inventory.Application.DTO.Response.ActivityTracker
{
    public class BaseActivityTracker : BaseAuditableEntity
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public bool OperationState { get; set; }
    }
}
