using ETRADE.DataAccess.Abstract;
using ETRADE.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.DataAccess.Concrete.EfCore
{
    public class EfCoreOrderDal : EfCoreGenericRepository<Order, DataContext>, IOrderDal
    {
        public List<Order> GetOrders(string userId) //useridye göre sipariş dönülecek
        {
            using (var context = new DataContext())
            {
                var orders = context.Orders
                            .Include(i => i.OrderItems)
                            .ThenInclude(i => i.Product)
                            .ThenInclude(i => i.Images)
                            .AsQueryable();
                if (!string.IsNullOrEmpty(userId))
                {
                    orders=orders.Where(i => i.UserId == userId);
                }
                return orders.ToList(); 
            }

        }
    }
}
