﻿using Microsoft.AspNetCore.Mvc;

namespace Store.YandexKassa.Areas.YandexKassa.Controllers
{
    [Area("YandexKassa")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // /YandexKassa/Home/CallBack
        public IActionResult Callback()
        {
            return View();
        }
    }
}
