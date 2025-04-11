using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReadHaven.Models.Cart;
using ReadHaven.Models.User;
using ReadHaven.Services;

namespace ReadHaven.Controllers
{
    public class BookCartController : Controller
    {
        private readonly CartService _cartService;
        private readonly AppDbContext _context;

        public BookCartController(CartService cartService, AppDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        public IActionResult Index()
        {
            List<CartItem> cartItems;

            if (User.Identity.IsAuthenticated)
            {
                var user = GetCurrentUser();

                if (user == null)
                {
                    cartItems = _cartService.GetCartItemsForGuest();
                }
                else
                {
                    cartItems = _cartService.GetCartItemsForUser(user.Id);
                }
            }
            else
            {
                cartItems = _cartService.GetCartItemsForGuest();
            }

            return View(cartItems);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
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

            return RedirectToAction("Index", "Book");
        }

        

        public User GetCurrentUser()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            return user;
        }
    }
}
