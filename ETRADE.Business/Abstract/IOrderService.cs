using ETRADE.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.Business.Abstract
{
    public interface IOrderService
    {
        List<Order> GetOrders(string userId, string UserName);
        void Create(Order entity);

    }
}
