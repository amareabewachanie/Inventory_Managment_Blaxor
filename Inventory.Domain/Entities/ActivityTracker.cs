
using Inventory.Domain.Common;

namespace Inventory.Domain.Entities
{
    public class ActivityTracker : BaseAuditableEntity
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public bool OperationState { get; set; } = false;

    }
}
