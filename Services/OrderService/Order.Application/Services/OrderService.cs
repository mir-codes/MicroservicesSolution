using Order.Application.DTOs;
using Order.Application.Interfaces;
using Order.Domain.Entities;
using Order.Domain.Enum;

namespace Order.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<List<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToDto).ToList();
        }

        public async Task<List<OrderDto>> GetMyOrdersAsync(string userEmail)
        {
            var orders = await _orderRepository.GetByUserEmailAsync(userEmail);
            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order != null ? MapToDto(order) : null;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto, string userEmail)
        {
            var order = new Domain.Entities.Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..8]}",
                UserEmail = userEmail,
                ShippingAddress = createOrderDto.ShippingAddress,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Items = createOrderDto.Items.Select(i => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

            var createdOrder = await _orderRepository.CreateAsync(order);
            return MapToDto(createdOrder);
        }

        public async Task<OrderDto?> UpdateOrderStatusAsync(Guid id, UpdateOrderStatusDto updateDto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return null;

            order.Status = updateDto.Status;
            if (updateDto.Status == OrderStatus.Shipped)
            {
                order.ShippedDate = DateTime.UtcNow;
            }

            var updatedOrder = await _orderRepository.UpdateAsync(order);
            return MapToDto(updatedOrder);
        }

        public async Task<bool> CancelOrderAsync(Guid id, string userEmail)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null || order.UserEmail != userEmail) return false;

            if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
            {
                return false; // Cannot cancel shipped or delivered orders
            }

            order.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateAsync(order);
            return true;
        }

        private static OrderDto MapToDto(Domain.Entities.Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                UserId = order.UserId,
                UserEmail = order.UserEmail,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                OrderDate = order.OrderDate,
                ShippingAddress = order.ShippingAddress,
                Items = order.Items.Select(i => new OrderItemDto
                {
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };
        }
    }
}
