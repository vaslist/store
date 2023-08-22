using Microsoft.AspNetCore.Mvc;
using Store;
using Store.Web.App;

namespace Store.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly BookService bookService;
        public SearchController(BookService bookService)
        {
            this.bookService = bookService;
        }

        public async Task<IActionResult> Index(string query)
        {
            var books = await bookService.GetAllByQueryAsync(query);

            return View(books);
        }
    }
}
