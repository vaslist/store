using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Store.Tests
{
    public class BookServiceTests
    {
        [Fact]
        public void GetAllByQuery_WithIsbn_CallGetAllByIsbn()
        {
            var bookRepositoryStub= new Mock<IBookRepository>();
            bookRepositoryStub.Setup(x => x.GetAllByIsbn(It.IsAny<string>()))
                .Returns(new[] { new Book(1, "", "", "", "Описание книги Art of programming", 0m) });

            bookRepositoryStub.Setup(x => x.GetAllByTitleOrAuthor(It.IsAny<string>()))
               .Returns(new[] { new Book(2, "", "", "", "Description Fowler refactoring process", 0m) });

            BookService bookService = new BookService(bookRepositoryStub.Object);
            var validIsbn = "ISBN 12345-12345";
            var actual = bookService.GetAllByQuery(validIsbn);
            Assert.Collection(actual, book => Assert.Equal(1, book.Id));
        }

        [Fact]
        public void GetAllByQuery_WithAuthor_CallGetAllByTitleOrAuthor()
        {
            int idOfIsbnSearch = 1;
            int idOfAuthorSearch = 2;
            var bookRepositoryStub = new Mock<IBookRepository>();
            
            bookRepositoryStub.Setup(x => x.GetAllByIsbn(It.IsAny<string>()))
                .Returns(new[] { new Book(idOfIsbnSearch, "", "", "", "", 0m) });

            bookRepositoryStub.Setup(x => x.GetAllByTitleOrAuthor(It.IsAny<string>()))
               .Returns(new[] { new Book(idOfAuthorSearch, "", "", "", "", 0m) });

            BookService bookService = new BookService(bookRepositoryStub.Object);
            var author = "Ritchie";
            var actual = bookService.GetAllByQuery(author);
            Assert.Collection(actual, book => Assert.Equal(2, book.Id));
        }
    }
}
