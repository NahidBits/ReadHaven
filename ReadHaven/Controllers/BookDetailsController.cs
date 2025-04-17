using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReadHaven.Models.Book;
using ReadHaven.Models.User;
using ReadHaven.Services;
using ReadHaven.ViewModels;

namespace ReadHaven.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class BookDetailsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly GenericRepository<Book> _bookRepository;
        private readonly GenericRepository<BookReview> _reviewRepository;


        public BookDetailsController(AppDbContext context, GenericRepository<Book> bookRepository, GenericRepository<BookReview> reviewRepository)
        {
            _context = context;
            _bookRepository = bookRepository;
            _reviewRepository = reviewRepository;
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null) return NotFound();

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

            return View(bookView);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateReview(BookReview review)
        {
            if (review.ReviewText == null)
                return NotFound();

            var user = CurrentUser();

            var existingReview = await _context.BookReviews
                .FirstOrDefaultAsync(r => r.BookId == review.BookId && r.UserId == user.Id);

            if (existingReview != null)
            {
                // Update existing
                existingReview.Rating = review.Rating;
                existingReview.ReviewText = review.ReviewText;
                await _reviewRepository.UpdateAsync(existingReview);
            }
            else
            {
                // Create new
                review.UserId = user.Id;
                await _reviewRepository.AddAsync(review);
            }

            return RedirectToAction("Details", new { id = review.BookId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReview(Guid reviewId, Guid bookId)
        {
            var user = CurrentUser();
            var review = await _context.BookReviews
                .FirstOrDefaultAsync(r => r.Id == reviewId && r.UserId == user.Id && r.BookId == bookId);

            if (review == null) return NotFound();
            await _reviewRepository.DeleteAsync(review);

            return RedirectToAction("Details", new { id = bookId });
        }
        public User CurrentUser()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            return user;
        }
    }
}
