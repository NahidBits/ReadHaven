using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadHaven.Models.Book;
using ReadHaven.Services;
using ReadHaven.ViewModels;

namespace ReadHaven.Controllers
{
    public class BookController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly BookService _bookService;
        private readonly GenericRepository<Book> _bookRepository;

        public BookController(BookService bookService, GenericRepository<Book> bookRepository,AppDbContext context)
        {
            _bookService = bookService;
            _bookRepository = bookRepository;
            _context = context;
        }

        // GET: Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: GetBookList
        [HttpGet("GetBookList")]
        public async Task<IActionResult> GetBookList(BookSearchModel searchBook)
        {
            var books = await _bookService.GetBooksWithSearchAsync(searchBook);
            var allReviews = _context.BookReviews.ToList();
            var userWishlist = new Dictionary<Guid, bool>();

            if (UserId != Guid.Empty)
            {
                userWishlist = _context.Wishlists
                    .Where(w => w.UserId == UserId)
                    .ToDictionary(
                        w => w.BookId,
                        w => w.IsLoved
                    );
            }

            var bookList = books.Select(book => new
            {
                Id = book.Id,
                Title = book.Title,
                Genre = book.Genre,
                Price = book.Price,
                ImagePath = book.ImagePath,
                Rating = (int)Math.Round(allReviews
                    .Where(r => r.BookId == book.Id)
                    .Select(r => (int)r.Rating)
                    .DefaultIfEmpty(0)
                    .Average()),

                IsLoved = userWishlist.TryGetValue(book.Id, out var isLoved) && isLoved
            }).ToList();

            return Ok(bookList);
        }

        [HttpGet("GetBookCount")]
        public async Task<IActionResult> GetBookCount()
        {
            var count = await _bookRepository.GetCountAsync(); 
            return Ok(count);
        }

        [Authorize]
        [HttpPost("Wishlist")]
        public async Task<IActionResult> UpdateToWishlist(Guid bookId)
        {
            var wishlist = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == UserId && w.BookId == bookId);
            if (wishlist != null)
            {
                wishlist.IsLoved = !wishlist.IsLoved;
                _context.Wishlists.Update(wishlist);
            }
            else
            {
                wishlist = new Wishlist
                {
                    UserId = UserId,
                    BookId = bookId,
                    IsLoved = true
                };
                await _context.Wishlists.AddAsync(wishlist);
            }
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }


        // POST: create
        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> Create(Book book, IFormFile image)
        {
            if (!ModelState.IsValid || image == null)
                return NotFound();

            await _bookService.AddBookWithImageAsync(book, image);

            return Ok(new { success = true });
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return NotFound();
            return View();
        }

        [HttpGet("GetBookById")]
        public async Task<IActionResult> GetBook(Guid id)
        {
            var bookFind = await _bookRepository.GetByIdAsync(id);
            if (bookFind == null) return NotFound();

            var isLoved = false;
            if (UserId != Guid.Empty)
            {
                isLoved = _context.Wishlists.Any(w => w.UserId == UserId && w.BookId == id && w.IsLoved);
            }

            var book = new
            {
                Id = bookFind.Id,
                Title = bookFind.Title,
                Genre = bookFind.Genre,
                Price = bookFind.Price,
                ImagePath = bookFind.ImagePath,
                IsLoved = isLoved
            };

            return Ok(book);
        }

        // POST : update
        [Authorize(Roles = "Admin")]
        [HttpPost("BookUpdate")]
        public async Task<IActionResult> Update([FromForm] Book book, [FromForm] IFormFile image)
        {
            ModelState.Remove("image");
            if (!ModelState.IsValid)
                return NotFound();

            await _bookService.UpdateBookWithImageAsync(book, image);

            return Ok(new { success = true });
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book != null)
            {
                await _bookRepository.DeleteAsync(book);
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetBookSales")]
        public async Task<IActionResult> GetBookSales()
        {
            var salesData = await _context.CartItems
                .IgnoreQueryFilters()
                .Where(c => c.IsDeleted)
                .Join(_context.Books,  
                      cartItem => cartItem.BookId, 
                      book => book.Id,  
                      (cartItem, book) => new { cartItem, book })  
                .GroupBy(c => new { c.book.Id, c.book.Title,c.book.ImagePath })
                .Select(g => new
                {
                    BookId = g.Key.Id,
                    ImageUrl = string.IsNullOrEmpty(g.Key.ImagePath)? "/uploads/book/Default_image.webp": g.Key.ImagePath,
                    Title = g.Key.Title,
                    QuantitySold = g.Sum(x => x.cartItem.Quantity)  
                })
                .OrderByDescending(x => x.QuantitySold)  
                .ToListAsync(); 

            return Ok(salesData); 
        }

    }
}
