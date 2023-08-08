using System.Linq;

namespace Store.Memory
{
    public class BookRepository : IBookRepository
    {
        private readonly Book[] books = new Book[]
        {
            new Book(1,"ISBN 12345-12345","D. Knuth","Art of programming"),
            new Book(2,"ISBN 23456-12345","M. Fowler", "Refactoring"),
            new Book(3,"ISBN 12345-12346","B. Kernighan, T. Ritchi","С programming lenguage")
        };

        public Book[] GetAllByIsbn(string isbn)
        {
            return books.Where(b=>b.Isbn == isbn).ToArray();
        }

        public Book[] GetAllByTitleOrAuthor(string query)
        {
            return books.Where(b=>b.Title.Contains(query)
                               || b.Author.Contains(query))
                        .ToArray();
        }
    }
}