using ETRADE.Business.Abstract;
using ETRADE.DataAccess.Abstract;
using ETRADE.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.Business.Concrete
{
    public class CartManager : ICartService
    {
        private ICartDal _cartDal; // Dependency Injection
        public CartManager(ICartDal cartDal)
        {
            _cartDal = cartDal;
        }

        public void AddToCart(string userId, int productId, int quantity)
        {
            var cart = GetCartByUserId(userId); // Kullanıcıya ait sepeti getir.
            if (cart is not null)
            {
                var index = cart.CartItems.FindIndex(x => x.ProductId == productId);//sepette ürün var mı? 
                //sepette o ürün yoksa
                if (index < 0)
                {
                    cart.CartItems.Add(
                        new CartItem()
                        {
                            ProductId = productId,
                            Quantity = quantity,
                            CartId = cart.Id
                        }
                    );
                }
                else //sepette o ürün varsa
                {
                    cart.CartItems[index].Quantity += quantity;
                }
            }
            _cartDal.Update(cart); // DataAcces aracılığıyla sepeti günceller.IRepository'den gelen metot.Somutlaştırıldı.
        }

        public void ClearCart(string cartId)
        {
            _cartDal.ClearCart(cartId);
        }

        public void DeleteFromCart(string userId, int productId)
        {
            var cart = GetCartByUserId(userId);
            if (cart != null)
            {
                _cartDal.DeleteFromCart(cart.Id, productId);
            }
        }

        public Cart GetCartByUserId(string userId)
        {
            return _cartDal.GetCartByUserId(userId);
        }

        public void InitialCart(string userId) // Cart'ı ilk haline getir.
        {
            Cart cart = new Cart()
            {
                UserId = userId,
            };
            _cartDal.Create(cart);

        }
    }
}
