//using demoWebAPI.Models;
//using demoWebAPI.Data.Repositories;
//using Microsoft.EntityFrameworkCore;

//namespace demoWebAPI.Data.Repositories
//{
//    public class OrderRepository : Repository<Order>, IOrderRepository
//    {
//        public OrderRepository( EcomDbContext context)
//            : base(context)
//        {
//        }

//        public async Task<IEnumerable<Order>> GetOrdersByUserAsync(string userId)
//        {
//            return await _dbSet
//                .Where(o => o.UserId == userId)
//                .ToListAsync();
//        }
//    }
//}