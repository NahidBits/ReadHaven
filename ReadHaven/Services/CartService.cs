using Microsoft.AspNetCore.Http;
using ReadHaven.Models.Cart;
using ReadHaven.Models.Book;
using ReadHaven.Helpers;

namespace ReadHaven.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;

        public CartService(IHttpContextAccessor httpContextAccessor, AppDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        // Get the current HttpContext
        private HttpContext HttpContext => _httpContextAccessor.HttpContext;

        // Get Cart for Guest (session-based)
        public List<CartItem> GetCartItemsForGuest()
        {
            var cartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            return cartItems ?? new List<CartItem>();
        }

        // Get Cart for Logged-in User (database-based)
        public List<CartItem> GetCartItemsForUser(Guid userId)
        {
            return _context.CartItems.Where(c => c.UserId == userId).ToList();
        }

        // Add Book to Cart for Guest
        public void AddToCartForGuest(Book book)
        {
            var cartItems = GetCartItemsForGuest();
            var existingItem = cartItems.FirstOrDefault(c => c.BookId == book.Id);
            if (existingItem == null)
            {
                cartItems.Add(new CartItem
                {
                    BookId = book.Id,
                    Price = book.Price,
                    Quantity = 1
                });
            }
            else
            {
                existingItem.Quantity++;
            }

            HttpContext.Session.SetObjectAsJson("Cart", cartItems);
        }

        // Add Book to Cart for Logged-in User
        public void AddToCartForUser(Guid userId, Book book)
        {
            var cartItem = _context.CartItems.FirstOrDefault(c => c.UserId == userId && c.BookId == book.Id);
            if (cartItem == null)
            {
                _context.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    BookId = book.Id,
                    Price = book.Price,
                    Quantity = 1
                });
            }
            else
            {
                cartItem.Quantity++;
            }

            _context.SaveChanges();
        }
    }
}
