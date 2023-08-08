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
                .Returns(new[] { new Book(1, "", "", "") });

            bookRepositoryStub.Setup(x => x.GetAllByTitleOrAuthor(It.IsAny<string>()))
               .Returns(new[] { new Book(2, "", "", "") });

            BookService bookService = new BookService(bookRepositoryStub.Object);
            var validIsbn = "ISBN 12345-12345";
            var actual = bookService.GetAllByQuery(validIsbn);
            Assert.Collection(actual, book => Assert.Equal(1, book.Id));
        }

        [Fact]
        public void GetAllByQuery_WithAuthor_CallGetAllByTitleOrAuthor()
        {
            var bookRepositoryStub = new Mock<IBookRepository>();
            bookRepositoryStub.Setup(x => x.GetAllByIsbn(It.IsAny<string>()))
                .Returns(new[] { new Book(1, "", "", "") });

            bookRepositoryStub.Setup(x => x.GetAllByTitleOrAuthor(It.IsAny<string>()))
               .Returns(new[] { new Book(2, "", "", "") });

            BookService bookService = new BookService(bookRepositoryStub.Object);
            var invalidIsbn = "ISB 12345-12345";
            var actual = bookService.GetAllByQuery(invalidIsbn);
            Assert.Collection(actual, book => Assert.Equal(2, book.Id));
        }
    }
}
