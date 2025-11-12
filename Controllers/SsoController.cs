using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Text;

// DTO để lưu trữ thông tin đăng nhập an toàn
public class SsoCredentials
{
    public string username { get; set; }
    public string password { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class SsoController : ControllerBase
{
    private readonly IMemoryCache _cache;

    private const string TARGET_USERNAME = "aq";
    private const string TARGET_PASSWORD = "67788469";
    private const string TARGET_LOGIN_URL = "https://cst.aqtech.vn/api/main/login";

    public SsoController(IMemoryCache cache)
    {
        _cache = cache;
    }

    /**
     * Endpoint 1: Được gọi bởi Angular
     * Nó tạo một mã dùng một lần và trả về link để mở.
     */
    [HttpGet("get-autologin-url")]
    public IActionResult GetAutologinUrl()
    {
        // 1. Tạo một mã token ngẫu nhiên, dùng một lần
        var oneTimeCode = Guid.NewGuid().ToString();

        // 2. Lưu trữ thông tin đăng nhập vào cache với mã này
        // Token chỉ sống 60 giây
        _cache.Set(oneTimeCode, new SsoCredentials
        {
            username = TARGET_USERNAME,
            password = TARGET_PASSWORD
        }, TimeSpan.FromSeconds(60));

        // 3. Trả về URL mà Angular sẽ mở
        // Chú ý: URL này trỏ về endpoint "RedirectAndLogin" bên dưới
        var redirectUrl = Url.Action("RedirectAndLogin", "Sso",
                            new { code = oneTimeCode },
                            Request.Scheme);

        return Ok(new { autoLoginUrl = redirectUrl });
    }

    /**
     * Endpoint 2: Được gọi bởi trình duyệt (khi Angular mở tab mới)
     * Nó trả về một trang HTML tự động submit form.
     */
    [HttpGet("redirect-and-login")]
    public IActionResult RedirectAndLogin([FromQuery] string code)
    {
        // 1. Lấy thông tin đăng nhập từ cache
        if (string.IsNullOrEmpty(code) || !_cache.TryGetValue(code, out SsoCredentials credentials))
        {
            // Không tìm thấy code (đã hết hạn hoặc không hợp lệ)
            return Content("Link đăng nhập không hợp lệ hoặc đã hết hạn.", "text/html");
        }

        // 2. XÓA token ngay lập tức (chỉ dùng 1 lần)
        _cache.Remove(code);

        // 3. Tạo trang HTML động
        var html = $@"
            <html>
                <head><title>Đang chuyển hướng...</title></head>
                <body onload=""document.forms[0].submit()"">
                    <noscript>Vui lòng bấm nút 'Tiếp tục' để đăng nhập.</noscript>
                    <p>Đang tự động đăng nhập, vui lòng chờ...</p>
                    
                    <form action=""{TARGET_LOGIN_URL}"" method=""POST"">
                        <input type=""hidden"" name=""username"" value=""{credentials.username}"" />
                        <input type=""hidden"" name=""password"" value=""{credentials.password}"" />
                        
                        <button type=""submit"">Tiếp tục</button>
                    </form>
                </body>
            </html>";

        // 4. Trả về HTML
        return Content(html, "text/html", Encoding.UTF8);
    }
}