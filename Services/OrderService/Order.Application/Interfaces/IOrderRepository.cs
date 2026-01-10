using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Interfaces
{
    public interface IOrderRepository
    {
        Task<List<Domain.Entities.Order>> GetAllAsync();
        Task<List<Domain.Entities.Order>> GetByUserEmailAsync(string userEmail);
        Task<Domain.Entities.Order?> GetByIdAsync(Guid id);
        Task<Domain.Entities.Order> CreateAsync(Domain.Entities.Order order);
        Task<Domain.Entities.Order> UpdateAsync(Domain.Entities.Order order);
        Task<bool> DeleteAsync(Guid id);
    }
}
