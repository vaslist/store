using System.Text.RegularExpressions;

namespace Store;

public class Book
{
    public int Id { get; }
    public string Isbn { get; }
    public string Author { get; }
    public string Title { get; }

    public Book(int id, string isbn, string author, string title)
    {
        Id = id;
        Isbn = isbn;
        Author = author;
        Title = title;
    }

    internal static bool IsIsbn(string isbn)
    {
        if (string.IsNullOrEmpty(isbn)) 
            return false;
        isbn = isbn.Replace("-", "")
                   .Replace(" ", "")
                   .ToUpper();
        return Regex.IsMatch(isbn, @"^ISBN\d{10}(\d{3})?$");
    }

}
