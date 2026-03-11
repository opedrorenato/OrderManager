namespace Domain
{
    public class Order
    {
        public string Symbol { get; set; } = null!;
        public string Side { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
