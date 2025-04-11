using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // POST: update
        [Authorize(Roles = "Admin")]
        [HttpPost("update")]
        public async Task<IActionResult> Update(Book book)
        {
            if (!ModelState.IsValid)
                return NotFound();

            await _bookRepository.UpdateAsync(book);
            return RedirectToAction(nameof(Index));
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

        // Privacy Page (Can be removed or updated as necessary)
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
