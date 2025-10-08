using LibraryMVCWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LibraryMVCWeb.Controllers
{
    public class HomeController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            // 🚀 Đảm bảo luôn trả về model cho view
            var model = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(model);
        }
    }
}
