using System.Net;
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

    [Route("[controller]")]
    public class BookReviewController : Controller
    {
        private readonly AppDbContext _context;
        private readonly GenericRepository<Book> _bookRepository;
        private readonly GenericRepository<BookReview> _reviewRepository;


        public BookReviewController(AppDbContext context, GenericRepository<Book> bookRepository, GenericRepository<BookReview> reviewRepository)
        {
            _context = context;
            _bookRepository = bookRepository;
            _reviewRepository = reviewRepository;
        }

        [HttpGet("GetReviewsByBook")]
        public async Task<IActionResult> GetReviewsByBook(Guid id)
        {
            var reviewViewModels = _context.BookReviews
                                  .Where(r => r.BookId == id)
                                  .Select(r => new BookReviewViewModel
                                  {
                                      Id = r.Id,
                                      ReviewText = r.ReviewText,
                                      Rating = r.Rating,
                                      Date = DateOnly.FromDateTime(r.CreatedAt),
                                      UserId = r.UserId,
                                      UserName = _context.Users
                                 .Where(u => u.Id == r.UserId)
                                 .Select(u => u.Username)
                                 .FirstOrDefault() ?? "Unknown"
                                  })
                                 .ToList();
            return Ok(reviewViewModels);
        }

        [Authorize]
        [HttpPost("Save")]
        public async Task<IActionResult> AddOrUpdateReview(BookReview review)
        {
            if (!ModelState.IsValid)
                return NotFound();

            var existingReview = await _context.BookReviews
                .FirstOrDefaultAsync(r => r.BookId == review.BookId && r.UserId == review.UserId);

            if (existingReview != null)
            {
                // Update existing
                existingReview.Rating = review.Rating;
                existingReview.ReviewText = review.ReviewText;
                await _reviewRepository.UpdateAsync(existingReview);
            }
            else
            {
                await _reviewRepository.AddAsync(review);
            }

            return Ok(new { success = true });
        }

        [Authorize]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteReview(Guid id)
        {
            var review = await _context.BookReviews
                .FirstOrDefaultAsync(r => r.Id == id);

            if (review == null) return NotFound();
            await _reviewRepository.DeleteAsync(review);

            return Ok(new { success = true });
        }

        public User CurrentUser()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);

            return user;
        }
    }
}
