using LibraryMVCWeb.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json; // Cần thêm namespace này cho ReadFromJsonAsync

namespace LibraryMVCWeb.Controllers
{
    public class BookController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public BookController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Đã sửa lại tên tham số cho khớp với form và ảnh
        public async Task<IActionResult> IndexAsync(
            [FromQuery] string filterOn = null,
            [FromQuery] string filterQuery = null, // Tên tham số này phải khớp với input name="filterQuery"
            [FromQuery] string sortBy = null,
            [FromQuery] bool isAscending = true) // Đã đổi giá trị mặc định thành 'true' như trong ảnh
        {
            List<BookDTO> response = new List<BookDTO>();
            try
            {
                var client = _httpClientFactory.CreateClient();

                // 1. Dựng chuỗi truy vấn (query string) với các tham số
                string apiEndpoint = "https://localhost:7245/api/Books/get-all-books"; // Đã sửa URL theo ảnh

                string queryString = $"?filterOn={filterOn}&filterQuery={filterQuery}&sortBy={sortBy}&isAscending={isAscending}";

                // 2. Gửi yêu cầu GET kèm theo chuỗi truy vấn
                var httpResponseMess = await client.GetAsync(apiEndpoint + queryString);

                httpResponseMess.EnsureSuccessStatusCode();

                // 3. Đọc và thêm dữ liệu
                response.AddRange(await httpResponseMess.Content.ReadFromJsonAsync<IEnumerable<BookDTO>>());
            }
            catch (Exception ex)
            {
                // Thay vì log, theo ảnh, bạn đang return View("Error")
                // Đảm bảo bạn có View tên "Error.cshtml"
                return View("Error");
            }

            return View(response);
        }

        // ... các action khác (addBook, editBook, delBook) sẽ ở đây
    }
}