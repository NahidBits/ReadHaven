using Microsoft.AspNetCore.Mvc;
using ReadHaven.Models.Cart;
using ReadHaven.Services;
using ReadHaven.ViewModels;

namespace ReadHaven.Controllers
{
    [Route("[controller]")]
    public class BookCartController : BaseController
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

            if (IsAuthenticated)
            {
                cartItems = _cartService.GetCartItemsForUser(UserId);
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
                .Where(c => !IsAuthenticated || c.UserId == UserId)
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

        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart(Guid bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
                return NotFound();

            if (IsAuthenticated)
            {
                _cartService.AddToCartForUser(UserId, book);
            }
            else
            {
                _cartService.AddToCartForGuest(book);
            }

            return Ok(new { success = true, message = "Book added to cart." });
        }

        [HttpGet("GetCartItemCount")]
        public IActionResult GetCartItemCount()
        {
            int count = IsAuthenticated
                ? _cartService.GetCartItemCountForUser(UserId)
                : _cartService.GetCartItemCountForGuest();

            return Ok(count);
        }

        [HttpPost("ChangeCartItemQuantity")]
        public IActionResult ChangeCartItemQuantity(Guid id, int quantity)
        {
            if (IsAuthenticated)
            {
                _cartService.ChangeCartItemQuantityForUser(id, quantity);
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
            if (IsAuthenticated)
            {
                _cartService.RemoveCartItemForUser(id);
            }
            else
            {
                _cartService.RemoveCartItemForGuest(id);
            }

            return Ok(new { success = true, message = "Cart item removed." });
        }
    }
}
