namespace ReadHaven.ViewModels
{
    public class CartItemViewModel
    {
        public Guid Id { get; set; }        
        public Guid BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
