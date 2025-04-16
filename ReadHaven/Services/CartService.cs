using Microsoft.AspNetCore.Http;
using ReadHaven.Models.Cart;
using ReadHaven.Models.Book;

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

        private HttpContext HttpContext => _httpContextAccessor.HttpContext;

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

        public int GetCartItemCountForGuest()
        {
            var cartItems = GetCartItemsForGuest();
            return cartItems.Sum(item => item.Quantity);
        }

        // Returns the total number of cart items for a logged-in user (database-based)
        public int GetCartItemCountForUser(Guid userId)
        {
            return _context.CartItems
                           .Where(c => c.UserId == userId)
                           .Sum(c => c.Quantity);
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

        public void ChangeCartItemQuantityForUser(Guid id, int quantity)
        {
            var existingItem = _context.CartItems.FirstOrDefault(c => c.Id == id);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;

                if (existingItem.Quantity <= 0)
                {
                    _context.CartItems.Remove(existingItem);
                }
                else
                {
                    _context.CartItems.Update(existingItem);
                }

                _context.SaveChanges();
            }
        }

        public void ChangeCartItemQuantityForGuest(Guid id, int quantity)
        {
            var cartItems = GetCartItemsForGuest();
            var existingItem = cartItems.FirstOrDefault(c => c.BookId == id);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;

                
                if (existingItem.Quantity <= 0)
                {
                    cartItems.Remove(existingItem); 
                }

               
                HttpContext.Session.SetObjectAsJson("Cart", cartItems);
            }
        }
        public void RemoveCartItemForGuest(Guid id)
        {
            var cartItems = GetCartItemsForGuest();
            var itemToRemove = cartItems.FirstOrDefault(c => c.BookId == id);

            if (itemToRemove != null)
            {
                cartItems.Remove(itemToRemove);
                HttpContext.Session.SetObjectAsJson("Cart", cartItems); 
            }
        }

        public void RemoveCartItemForUser(Guid id)
        {
            var cartItem = _context.CartItems.FirstOrDefault(c => c.Id == id);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem); 
                _context.SaveChanges(); 
            }
        }
    }
}
