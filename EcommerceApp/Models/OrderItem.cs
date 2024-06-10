namespace ECommerceApp.Models
{
    public class OrderItem
    {
        public string Product { get; set; }
        public int Quantity { get; set; }
        public decimal PriceEach { get; set; }
    }
}
