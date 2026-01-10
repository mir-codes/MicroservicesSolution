using Order.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs
{
    public class UpdateOrderStatusDto
    {
        public OrderStatus Status { get; set; }
    }
}
