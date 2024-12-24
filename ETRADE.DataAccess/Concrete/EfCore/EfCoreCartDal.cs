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
                var cmd = @"delete from CartItem where CardId=@p0";
                context.Database.ExecuteSqlRaw(cmd, cartId);
            }
        }

        public void DeleteFromCart(string cartId, int productId) //spesific silme
        {
            using (var context = new DataContext())
            {
                var cmd = @"delete from CartItem where CartId=@p0 and ProductId=p1";
                context.Database.ExecuteSqlRaw(cmd, cartId, productId);
            }
        }

        public Cart GetCartByUserId(string userId) //useridye göre cart getirecek
        {
            using (var context = new DataContext())
            {
                return context.Carts //sepet tablosuna git
                    .Include(i => i.CartItems) //eğer içinde bir şeyler varsa
                    .ThenInclude(i => i.Product) //içinde product varsa
                    .ThenInclude(i => i.Images) //resimleri varsa
                    .FirstOrDefaultAsync(i => i.UserId == userId);//gönderdiğim kullanıcı idye göre sepeti döndür.
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
