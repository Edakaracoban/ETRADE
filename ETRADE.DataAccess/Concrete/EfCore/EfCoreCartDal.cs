using ETRADE.DataAccess.Abstract;
using ETRADE.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.DataAccess.Concrete.EfCore //sepetle ilgili CRUD İŞLEMLERİ
{
    public class EfCoreCartDal : EfCoreGenericRepository<Cart, DataContext>, ICartDal
    {
        public void ClearCart(string cartId) //sepeti içindeki tüm itemleri silme
        {
            using (var context = new DataContext())
            {
                var cmd = @"delete from CartItem where CartId=@p0";
                context.Database.ExecuteSqlRaw(cmd, cartId);
            }
        }

        public void DeleteFromCart(int cartId, int productId) //spesific silme
        {
            using (var context = new DataContext())
            {
                var cmd = @"delete from CartItem where CartId=@p0 and ProductId=@p1";
                context.Database.ExecuteSqlRaw(cmd, cartId, productId);
            }
        }

        public Cart GetCartByUserId(string userId)
        {
            using (var context = new DataContext())
            {
                return context.Carts
                       .Include(i => i.CartItems)
                       .ThenInclude(i => i.Product)
                       .ThenInclude(i => i.Images)
                       .FirstOrDefault(i => i.UserId == userId);
            }
        }

        public override void Update(Cart entity)
        {
            using (var context = new DataContext())
            {
                context.Carts.Update(entity);
                context.SaveChanges();
            }
        }

    }
}
