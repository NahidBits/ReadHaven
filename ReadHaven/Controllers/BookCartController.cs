using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReadHaven.Models.Cart;
using ReadHaven.Models.User;
using ReadHaven.Services;
using ReadHaven.ViewModels;

namespace ReadHaven.Controllers
{
    [Route("BookCart")]
    public class BookCartController : Controller
    {
        private readonly CartService _cartService;
        private readonly AppDbContext _context;

        public BookCartController(CartService cartService, AppDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetCartItems")]
        public IActionResult GetCartItem()
        {
            List<CartItem> cartItems;
            Guid? userId = null;

            if (User.Identity.IsAuthenticated)
            {
                var user = GetCurrentUser();
                if (user == null)
                {
                    cartItems = _cartService.GetCartItemsForGuest();
                }
                else
                {
                    userId = user.Id;
                    cartItems = _cartService.GetCartItemsForUser(user.Id);
                }
            }
            else
            {
                cartItems = _cartService.GetCartItemsForGuest();
            }

            var bookIds = cartItems.Select(c => c.BookId).Distinct().ToList();

            var books = _context.Books
                .Where(b => bookIds.Contains(b.Id))
                .ToDictionary(b => b.Id, b => b.Title);

            var cartItemView = cartItems
                .Where(c => !userId.HasValue || c.UserId == userId.Value)
                .Select(c => new CartItemViewModel
                {
                    Id = c.Id,
                    BookId = c.BookId,
                    BookTitle = books.ContainsKey(c.BookId) ? books[c.BookId] : "Unknown",
                    Quantity = c.Quantity,
                    UnitPrice = c.Price
                })
                .ToList();

            return Ok(cartItemView);
        }

        // POST: /Cart/AddToCart
        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart(Guid bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                return NotFound();
            }

            if (User.Identity.IsAuthenticated)
            {
                var user = GetCurrentUser();

                if (user == null)
                {
                    _cartService.AddToCartForGuest(book);
                }
                else
                {
                    _cartService.AddToCartForUser(user.Id, book);
                }
            }
            else
            {
                // For guests, add the book to cart in session.
                _cartService.AddToCartForGuest(book);
            }

            return Ok(new { success = true, message = "Book added to cart." });
        }

        [HttpGet("GetCartItemCount")]
        public IActionResult GetCartItemCount()
        {
            int count;

            if (User.Identity.IsAuthenticated)
            {
                var user = GetCurrentUser();
                count = user != null ? _cartService.GetCartItemCountForUser(user.Id) : 0;
            }
            else
            {
                count = _cartService.GetCartItemCountForGuest();
            }

            return Ok(count);
        }

        [HttpPost("ChangeCartItemQuantity")]
        public IActionResult ChangeCartItemQuantity(Guid id, int quantity)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = GetCurrentUser();
                if (user != null)
                {
                    _cartService.ChangeCartItemQuantityForUser(id, quantity);
                }
            }
            else
            {
                _cartService.ChangeCartItemQuantityForGuest(id, quantity);
            }
            return Ok(new { success = true, message = "Cart item quantity updated." });
        }

        [HttpPost("RemoveCartItem")]
        public IActionResult RemoveCartItem(Guid id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = GetCurrentUser();
                if (user != null)
                {
                    _cartService.RemoveCartItemForUser(id);
                }
            }
            else
            {
                _cartService.RemoveCartItemForGuest(id);
            }
            return Ok(new { success = true, message = "Cart item quantity updated." });
        }

        public User GetCurrentUser()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            return user;
        }
    }
}
