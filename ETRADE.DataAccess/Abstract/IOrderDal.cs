using ETRADE.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.DataAccess.Abstract
{
    public interface IOrderDal:IRepository<Order>
    {
        List<Order> GetOrders(string userId, string UserName); //Kullanıcının idisine göre tüm siparişleri liste halinde döndürecek
    }
}
