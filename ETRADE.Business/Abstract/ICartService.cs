using ETRADE.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.Business.Abstract
{
    public interface ICartService
    {
        void InitialCart(string userId); // Kartı ilk haline getir.
        Cart GetCartByUserId(string userId); // Kullanıcıya ait sepeti getir. 
        void AddToCart(string userId, int productId, int quantity); // Ürün ekleme
        void DeleteFromCart(string userId, int productId); // Sepetten ürün silme
        void ClearCart(string cartId); // Sepeti temizleme

    }
}
