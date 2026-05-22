using demoWebAPI.Models;

namespace demoWebAPI.Data.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId);
    }
}