namespace ETRADE.Entities
{
    public class OrderItem //siparişin elemanları
    {
        public int Id { get; set; }
        public int OrderId { get; set; } //sipariş elemanından o siparişin kime ait olduğu şehir vs erişilebilecek
        public Order Order { get; set; }
        public Product Product { get; set; } //siparişe ait ürüne ulaşılabilecek ürün adı vs
        public int ProductId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}