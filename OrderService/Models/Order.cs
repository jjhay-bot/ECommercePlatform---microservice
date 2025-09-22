namespace OrderService.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } // Intentional bug: no null check
        public decimal TotalAmount { get; set; }
    }
}
