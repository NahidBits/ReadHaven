using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ReadHaven.Models.Book;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ReadHaven.Services
{
    public class BookService
    {
        private readonly IWebHostEnvironment _env;
        private readonly GenericRepository<Book> _bookRepository;

        public BookService(IWebHostEnvironment env, GenericRepository<Book> bookRepository)
        {
            _env = env;
            _bookRepository = bookRepository;
        }

        public async Task<List<Book>> GetBooksWithSearchAsync(BookSearchModel searchModel)
        {
            IQueryable<Book> query = _bookRepository.GetQueryable();

            if (!string.IsNullOrEmpty(searchModel.Title))
            {
                query = query.Where(b => b.Title.Contains(searchModel.Title));
            }

            if (!string.IsNullOrEmpty(searchModel.Genre))
            {
                query = query.Where(b => b.Genre.Contains(searchModel.Genre));
            }

            if (searchModel.PriceSort.HasValue)
            {
                if (searchModel.PriceSort == 1)
                {
                    query = query.OrderBy(b => b.Price);
                }
                else if (searchModel.PriceSort == 2)
                {
                    query = query.OrderByDescending(b => b.Price);
                }
            }

            return await query.ToListAsync();
        }


        public async Task AddBookWithImageAsync(Book book, IFormFile imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "book");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                book.ImagePath = "/uploads/book/" + uniqueFileName;
            }

            await _bookRepository.AddAsync(book);
        }

    }
}