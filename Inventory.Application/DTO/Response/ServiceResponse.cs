using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory.Application.DTO.Response
{
    public record ServiceResponse(bool Flag, string? Message);
}
