namespace User.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<List<Domain.Entities.User>> GetAllAsync();
        Task<Domain.Entities.User?> GetByIdAsync(Guid id);
        Task<Domain.Entities.User?> GetByEmailAsync(string email);
        Task<Domain.Entities.User> CreateAsync(Domain.Entities.User user);
        Task<Domain.Entities.User> UpdateAsync(Domain.Entities.User user);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
