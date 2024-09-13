using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Store.Data.EF
{
    public class BookRepository : IBookRepository
    {
        private readonly DbContextFactory dbContextFactory;

        public BookRepository(DbContextFactory dbContextFactory)
        {
            this.dbContextFactory = dbContextFactory;
        }


        public async Task<Book[]> GetAllByIdsAsync(IEnumerable<int> bookIds)
        {
            var dbContext = dbContextFactory.Create(typeof(BookRepository));

            var dtos = await dbContext.Books
                                 .Where(book => bookIds.Contains(book.Id))
                                 .ToArrayAsync();

            return dtos.Select(Book.Mapper.Map)
                       .ToArray();
        }


        public async Task<Book[]> GetAllByIsbnAsync(string isbn)
        {
            var dbContext = dbContextFactory.Create(typeof(BookRepository));

            if (Book.TryFormatIsbn(isbn, out string formattedIsbn))
            {
                var booksDto = await dbContext.Books
                                .Where(book => book.Isbn == formattedIsbn)
                                .ToArrayAsync();
                return booksDto.Select(Book.Mapper.Map)
                               .ToArray();
            }
            return new Book[0];
        }

        public async Task<Book[]> GetAllByTitleOrAuthorAsync(string? titleOrAuthor)
        {
            var dbContext = dbContextFactory.Create(typeof(BookRepository));
            BookDto[] dtos;
            if (titleOrAuthor.IsNullOrEmpty())
            {
                dtos = await dbContext.Books.ToArrayAsync();
            }
            else
            {
                var parameter = new SqlParameter("@titleOrAuthor", titleOrAuthor);
                dtos = await dbContext.Books
                                .FromSqlRaw("SELECT * FROM Books WHERE CONTAINS((Author, Title), @titleOrAuthor)",
                                            parameter)
                                .ToArrayAsync();
            }
            return dtos.Select(Book.Mapper.Map)
                            .ToArray();
        }

        public async Task<Book> GetByIdAsync(int id)
        {
            var dbContext = dbContextFactory.Create(typeof(BookRepository));

            var dto = await dbContext.Books
                               .SingleAsync(book => book.Id == id);

            return Book.Mapper.Map(dto);
        }
    }
}
