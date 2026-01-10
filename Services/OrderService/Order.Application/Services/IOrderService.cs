using Order.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Services
{
    public interface IOrderService
    {
        Task<List<OrderDto>> GetAllOrdersAsync();
        Task<List<OrderDto>> GetMyOrdersAsync(string userEmail);
        Task<OrderDto?> GetOrderByIdAsync(Guid id);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto, string userEmail);
        Task<OrderDto?> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDto updateDto);
        Task<bool> CancelOrderAsync(Guid id, string userEmail);
    }
}
