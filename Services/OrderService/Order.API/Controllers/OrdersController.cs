using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Application.DTOs;
using Order.Application.Services;
using BuildingBlocks.Middleware.Models;
using BuildingBlocks.Auth.Extensions;


namespace Order.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "admin,manager")]
        public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetAllOrders()
        {
            _logger.LogInformation("Admin fetching all orders");
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(orders));
        }

        [HttpGet("my-orders")]
        public async Task<ActionResult<ApiResponse<List<OrderDto>>>> GetMyOrders()
        {
            var email = User.GetUserEmail();
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(ApiResponse<List<OrderDto>>.ErrorResponse("User email not found"));
            }

            var orders = await _orderService.GetMyOrdersAsync(email);
            return Ok(ApiResponse<List<OrderDto>>.SuccessResponse(orders));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrderById(Guid id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound(ApiResponse<OrderDto>.ErrorResponse("Order not found"));
            }

            return Ok(ApiResponse<OrderDto>.SuccessResponse(order));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            var email = User.GetUserEmail();
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(ApiResponse<OrderDto>.ErrorResponse("User email not found"));
            }

            var order = await _orderService.CreateOrderAsync(createOrderDto, email);
            return CreatedAtAction(
                nameof(GetOrderById),
                new { id = order.Id },
                ApiResponse<OrderDto>.SuccessResponse(order, "Order created successfully")
            );
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "admin,manager")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateOrderStatus(
            Guid id,
            [FromBody] UpdateOrderStatusDto updateDto)
        {
            var order = await _orderService.UpdateOrderStatusAsync(id, updateDto);
            if (order == null)
            {
                return NotFound(ApiResponse<OrderDto>.ErrorResponse("Order not found"));
            }

            return Ok(ApiResponse<OrderDto>.SuccessResponse(order, "Order status updated"));
        }

        [HttpPost("{id}/cancel")]
        public async Task<ActionResult<ApiResponse<object>>> CancelOrder(Guid id)
        {
            var email = User.GetUserEmail();
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("User email not found"));
            }

            var result = await _orderService.CancelOrderAsync(id, email);
            if (!result)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Cannot cancel order"));
            }

            return Ok(ApiResponse<object>.SuccessResponse(null!, "Order cancelled successfully"));
        }
    }
}