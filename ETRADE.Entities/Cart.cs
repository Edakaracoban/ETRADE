using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ETRADE.Entities
{
    public class Cart //sepet
    {
        public int Id { get; set; } //sepetin idsi
        public string UserId { get; set; } //Kullanıcı id
        public List<CartItem> CartItems { get; set; } //sepetten sepetin içindeki elemanlara erişmek için
    }
    public class CartItem //sepetin içindeki elemanlar
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } //navigation property // ilişkiyi sağlamak için
        public Cart Cart { get; set; } //navigation 
        public int CartId { get; set; } // ilişkiyi sağlamak için : hangi sepetin ürünü
        public int Quantity { get; set; }
    }
}
