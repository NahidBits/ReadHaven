using ReadHaven.Models;
using ReadHaven.Models.Cart;
using ReadHaven.Models.Order;
using ReadHaven.ViewModels;

namespace ReadHaven.Models.Payment
{
    public class Report
    {
        public ReadHaven.Models.Order.Order Order { get; set; }
        public List<CartItemViewModel> Items { get; set; }
        public PaymentTransaction Payment { get; set; }
    }
}
