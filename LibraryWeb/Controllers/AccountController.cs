using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LibraryMVCWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("http://localhost:5264/api/User/Login", new
            {
                username = email,   // ⚠️ key phải là "username" (không phải Email)
                password = password
            });

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
                return View();
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            var token = json.GetProperty("jwtToken").GetString();

            // ✅ Lưu token vào session
            HttpContext.Session.SetString("JWToken", token);

            return RedirectToAction("Index", "Book");
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
