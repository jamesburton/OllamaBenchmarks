using System;
using System.Collections.Generic;
using System.Linq;

public record LibraryBook(string Isbn, string Title, string Author, bool IsAvailable);

public class Library
{
    private readonly List<LibraryBook> _books = new();

    public void AddBook(LibraryBook book)
    {
        _books.Add(book);
    }

    public bool Checkout(string isbn)
    {
        var index = _books.FindIndex(b => b.Isbn == isbn);

        if (index == -1)
        {
            return false;
        }

        var book = _books[index];

        if (!book.IsAvailable)
        {
            return false;
        }

        _books[index] = book with { IsAvailable = false };
        return true;
    }

    public bool Return(string isbn)
    {
        var index = _books.FindIndex(b => b.Isbn == isbn);

        if (index == -1)
        {
            return false;
        }

        var book = _books[index];
        _books[index] = book with { IsAvailable = true };
        return true;
    }

    public List<LibraryBook> GetAvailable()
    {
        return _books.Where(b => b.IsAvailable).ToList();
    }
}