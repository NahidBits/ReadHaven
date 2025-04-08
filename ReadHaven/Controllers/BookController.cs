using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadHaven.Models.Book;
using ReadHaven.Models.User;
using ReadHaven.Services;
using ReadHaven.ViewModels;

namespace ReadHaven.Controllers
{
    // [Authorize]
   // [Route("[controller]")]
    public class BookController : Controller
    {
        private readonly AppDbContext _context;
        private readonly GenericRepository<Book> _bookRepository;

        public BookController(AppDbContext context, GenericRepository<Book> bookRepository)
        {
            _context = context;
            _bookRepository = bookRepository;
        }


        // GET: /Book
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var books = await _bookRepository.GetAllAsync();
            return View(books);
        }

        // POST: /Book/create
        [HttpPost("create")]
        public async Task<IActionResult> Create(Book book)
        {
            if (ModelState.IsValid)
                return NotFound();

                await _bookRepository.AddAsync(book);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Book/update
        [HttpPost("update")]
        public async Task<IActionResult> Update(Book book)
        {
            if (ModelState.IsValid)
                return NotFound(); 

            await _bookRepository.UpdateAsync(book);
            return RedirectToAction(nameof(Index));
        }

        // POST: /Book/delete/{id}
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
        
        public IActionResult Privacy()
        {
            return View();
        }
        /*
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }*/
    }
}
