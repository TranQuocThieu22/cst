using Microsoft.AspNetCore.Mvc;
using System;
using System.Text;
using Newtonsoft.Json;
using System.Security.Cryptography;

[ApiController]
[Route("api/[controller]")]
public class SsoController : ControllerBase
{
    private string secretKey = "p994ZjcG4tirIF75JDFqO6YkYvu2Ris0rfP3U2OJqUIELsvWRL5Kt58xaFpzGFRG";

    public SsoController(){}

    [HttpGet("sso-cst-url")]
    public IActionResult GetSsoUrl()
    {
        var payload = new
        {
            MaTruong = "AQ",
            TenTruong = "Anh Quân",
            Roles = "admin",
            Exp = DateTime.UtcNow.AddMinutes(3)
        };
        var json = JsonConvert.SerializeObject(payload);
        var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        var signature = Sign(payloadBase64, secretKey);

        var token = $"{payloadBase64}.{signature}";

        var encodedToken = Uri.EscapeDataString(token);

        var url = $"https://cst.aqtech.vn/api/main/sso-login?token={encodedToken}";

        return Ok(new { autoLoginUrl = url });
    }

    private string Sign(string data, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using (var hmac = new HMACSHA256(keyBytes))
        {
            var hash = hmac.ComputeHash(dataBytes);
            return Convert.ToBase64String(hash);
        }
    }

}