using Microsoft.AspNetCore.Mvc;
using Store;

namespace Store.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly IBookRepository bookRepositry;
        public SearchController(IBookRepository bookRepositry)
        {
            this.bookRepositry = bookRepositry;
        }
        public IActionResult Index(string query)
        {
            var books = bookRepositry.GetAllByTitle(query);
            return View(books);
        }
    }
}
