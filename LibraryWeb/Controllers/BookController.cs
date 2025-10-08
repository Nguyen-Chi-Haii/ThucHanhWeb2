using LibraryMVCWeb.Models.DTO;
using LibraryWeb.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LibraryMVCWeb.Controllers
{
    public class BookController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;

        public BookController(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // ------------------- GET: Danh sách sách -------------------
        public async Task<IActionResult> Index(
        string? filterOn,
        string? filterQuery,
        string? sortBy,
        bool isAscending = true)
            {
                var client = _httpClientFactory.CreateClient();
                var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");

                if (string.IsNullOrEmpty(token))
                    return RedirectToAction("Login", "Account");

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // 🟢 Gọi API lấy toàn bộ sách
                var response = await client.GetAsync("http://localhost:5264/api/Book/get-all-books");
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "❌ Không thể tải danh sách sách!";
                    return View(new List<BookDTO>());
                }

                var json = await response.Content.ReadAsStringAsync();
                var books = JsonSerializer.Deserialize<List<BookDTO>>(json, _jsonOptions) ?? new List<BookDTO>();

                // 🟢 Bộ lọc
                if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
                {
                    filterQuery = filterQuery.ToLower();

                    books = filterOn.ToLower() switch
                    {
                        "title" => books.Where(b => b.Title?.ToLower().Contains(filterQuery) == true).ToList(),
                        "author" => books.Where(b => b.AuthorNames != null && b.AuthorNames.Any(a => a.ToLower().Contains(filterQuery))).ToList(),
                        "publisher" => books.Where(b => b.PublisherName?.ToLower().Contains(filterQuery) == true).ToList(),
                        _ => books
                    };
                }

                // 🟢 Sắp xếp
                if (!string.IsNullOrWhiteSpace(sortBy))
                {
                    books = sortBy.ToLower() switch
                    {
                        "title" => isAscending ? books.OrderBy(b => b.Title).ToList() : books.OrderByDescending(b => b.Title).ToList(),
                        "rate" => isAscending ? books.OrderBy(b => b.Rate).ToList() : books.OrderByDescending(b => b.Rate).ToList(),
                        _ => books
                    };
                }

                // 🟢 Giữ lại giá trị đã chọn để hiển thị lại trong View
                ViewBag.FilterOn = filterOn;
                ViewBag.FilterQuery = filterQuery;
                ViewBag.SortBy = sortBy;
                ViewBag.IsAscending = isAscending;

                return View(books);
            }


        // ------------------- GET: Trang thêm sách -------------------
        [HttpGet]
        public async Task<IActionResult> AddBook()
        {
            await LoadDropdownData();
            return View();
        }

        // ------------------- POST: Gửi form thêm sách -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBook(AddBookDTO model)
        {
            TempData.Remove("SuccessMessage");
            TempData.Remove("ErrorMessage");
            if (!ModelState.IsValid)
            {
                await LoadDropdownData();
                TempData["ErrorMessage"] = "⚠️ Dữ liệu nhập chưa hợp lệ!";  // 🆕 Đổi key
                return View(model);
            }
            model.DateAdded = DateTime.Now;

            var client = _httpClientFactory.CreateClient();
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 🆕 Debug: Log model và JSON để check Rate
            Console.WriteLine($"🆕 Model Rate: {model.Rate}");  // Xem Output console
            var jsonString = JsonSerializer.Serialize(model, _jsonOptions);  // Dùng _jsonOptions nếu có camelCase
            Console.WriteLine($"🆕 JSON Payload: {jsonString}");  // Check "rate": số hay null

            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://localhost:5264/api/Book/add-book", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "✅ Thêm sách thành công!";  // 🆕 Đổi key
                return RedirectToAction("Index");
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"❌ Thêm sách thất bại! Chi tiết: {errorResponse}";  // 🆕 Đổi key + Giữ chi tiết lỗi từ API
                await LoadDropdownData();
                return View(model);
            }
        }

        // ------------------- Load danh sách dropdown -------------------
        private async Task LoadDropdownData()
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var authorsResponse = await client.GetAsync("http://localhost:5264/api/Authors/get-all-author");
            var publishersResponse = await client.GetAsync("http://localhost:5264/get-all-publisher");

            List<AuthorDTO> authors = new();
            List<PublisherDTO> publishers = new();

            if (authorsResponse.IsSuccessStatusCode)
            {
                var authorsJson = await authorsResponse.Content.ReadAsStringAsync();
                authors = JsonSerializer.Deserialize<List<AuthorDTO>>(authorsJson, _jsonOptions) ?? new List<AuthorDTO>();
            }

            if (publishersResponse.IsSuccessStatusCode)
            {
                var publishersJson = await publishersResponse.Content.ReadAsStringAsync();
                publishers = JsonSerializer.Deserialize<List<PublisherDTO>>(publishersJson, _jsonOptions) ?? new List<PublisherDTO>();
            }

            // Lưu cả raw lists (dùng để tạo SelectList có selected value sau này)
            ViewBag.AuthorsList = authors;
            ViewBag.PublishersList = publishers;

            // Tạo default SelectList / MultiSelectList (no selected)
            ViewBag.Authors = new MultiSelectList(authors, "Id", "FullName");
            ViewBag.Publishers = new SelectList(publishers, "Id", "Name");
        }

        // ------------------- GET: Trang chỉnh sửa sách -------------------
        [HttpGet]
        public async Task<IActionResult> EditBook(string id)
        {
            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Gọi đúng endpoint API lấy sách theo id
            var response = await client.GetAsync($"http://localhost:5264/api/Book/get-book-by-id/{id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "❌ Không thể tải dữ liệu sách.";
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            var book = JsonSerializer.Deserialize<UpdateBookDTO>(json, _jsonOptions);

            // Load dropdowns (raw lists + list helpers)
            await LoadDropdownData();

            // Lấy raw lists rồi tạo SelectList/MultiSelectList có selected value
            var publishers = ViewBag.PublishersList as List<PublisherDTO> ?? new List<PublisherDTO>();
            var authors = ViewBag.AuthorsList as List<AuthorDTO> ?? new List<AuthorDTO>();

            // Chú ý: convert selected values to string để tránh mismatch kiểu (nếu cần)
            var selectedPublisher = book != null ? book.PublisherID.ToString() : null;
            var selectedAuthorIds = book?.AuthorIds?.Select(x => x.ToString()).ToArray() ?? Array.Empty<string>();

            ViewBag.Publishers = new SelectList(publishers, "Id", "Name", selectedPublisher);
            ViewBag.Authors = new MultiSelectList(authors, "Id", "FullName", selectedAuthorIds);

            return View(book);
        }




        // ------------------- POST: Gửi form chỉnh sửa -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(string id, UpdateBookDTO model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownData();
                TempData["ErrorMessage"] = "⚠️ Dữ liệu nhập chưa hợp lệ!";
                return View(model);
            }

            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var jsonContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"http://localhost:5264/api/Book/update-book-by-id/{id}", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "✅ Cập nhật sách thành công!";
                return RedirectToAction("Index");
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"❌ Cập nhật thất bại! Chi tiết: {errorResponse}";
                await LoadDropdownData();
                return View(model);
            }
        }

        // ------------------- DELETE: Xóa sách -------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBook(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "⚠️ ID sách không hợp lệ!";
                return RedirectToAction("Index");
            }

            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"http://localhost:5264/api/Book/delete-book-by-id/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "✅ Xóa sách thành công!";
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"❌ Xóa thất bại! Chi tiết: {errorResponse}";
            }

            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> GetBookDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "Thiếu ID sách" });

            var client = _httpClientFactory.CreateClient();
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JWToken");

            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"http://localhost:5264/api/Book/get-book-by-id/{id}");
            if (!response.IsSuccessStatusCode)
                return NotFound(new { message = "Không tìm thấy sách." });

            var json = await response.Content.ReadAsStringAsync();
            var book = JsonSerializer.Deserialize<BookDTO>(json, _jsonOptions);

            return Json(book);
        }
    }
}
