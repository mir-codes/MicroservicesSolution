namespace Order.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using Order.Application.Interfaces;
using Order.Infrastructure.Data;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<List<Domain.Entities.Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<List<Domain.Entities.Order>> GetByUserEmailAsync(string userEmail)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserEmail == userEmail)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Domain.Entities.Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Domain.Entities.Order> CreateAsync(Domain.Entities.Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<Domain.Entities.Order> UpdateAsync(Domain.Entities.Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null) return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }
}
