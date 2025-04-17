using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadHaven.Models.Book;
using ReadHaven.Services;
using ReadHaven.ViewModels;

namespace ReadHaven.Controllers
{
    public class BookController : Controller
    {
        private readonly BookService _bookService;
        private readonly GenericRepository<Book> _bookRepository;

        public BookController(BookService bookService, GenericRepository<Book> bookRepository)
        {
            _bookService = bookService;
            _bookRepository = bookRepository;
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
            return Ok(books);
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
            /*
            BookDetailsViewModel bookView = new BookDetailsViewModel();
            bookView.Book = book;
            bookView.Reviews = _context.BookReviews
            .Where(r => r.BookId == book.Id)
            .ToList();

            var currentUser = CurrentUser();

            if (currentUser != null)
            {
                var userReview = await _context.BookReviews
              .FirstOrDefaultAsync(u => u.UserId == currentUser.Id && u.BookId == book.Id);

                if (userReview != null)
                    bookView.UserReview = userReview;
            }
            */
            return View();
        }

        [HttpGet("GetBookById")]
        public async Task<IActionResult> GetBook(Guid id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return NotFound();

            return Ok(book);
        }

        // POST: update
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
    }
}