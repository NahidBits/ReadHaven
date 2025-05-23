﻿namespace ReadHaven.Models.Book
{
    public class BookSearchModel
    {
        public string? Title { get; set; }
        public string? Genre { get; set; }
        public int? PriceSort { get; set; }
        public int StartIndex { get; set; } = 1;
        public int EndIndex { get; set; }
    }
}
