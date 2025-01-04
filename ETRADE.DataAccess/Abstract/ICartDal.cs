using ETRADE.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.DataAccess.Abstract
{
    public interface ICartDal :IRepository<Cart> // Interface arası implementasyon yapılmasına gerek yok.Sadece miras alınır.
    {
        void ClearCart(string cartId); //sipariş verildikten sonra sepet temizleme
        void DeleteFromCart(int cartId,int productId); //sepetten ürün silme
        Cart GetCartByUserId(string userId);                                          
    }
}
