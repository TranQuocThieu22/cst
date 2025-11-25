using educlient.Data;
using LiteDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Threading.Tasks;

namespace educlient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DanhSachAddinController : ControllerBase
    {
        private readonly IDbLiteContext database;
        private readonly IConfiguration config;

        public DanhSachAddinController(IDbLiteContext dataContext, IConfiguration cf)
        {
            database = dataContext;
            config = cf;
        }

        async Task<string> FetchApiAddin(string empObj, string url, string apiKey)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(apiKey))
            {
                return "Url, Api key are required!";
            }
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                    var payload = new { empObj = empObj };
                    string jsonData = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    HttpResponseMessage respone = await client.PostAsync(url, content);
                    if (!respone.IsSuccessStatusCode)
                    {
                        return "";
                    }
                    return await respone.Content.ReadAsStringAsync();
                }

            }
            catch (HttpRequestException)
            {
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        async Task<string> UpdateApiAddin(AddinInputUpdateDTO input, string url, string apiKey) 
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(apiKey))
            {
                return "Url, Api key are required!";
            }
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                    string jsonData = JsonConvert.SerializeObject(input);
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    HttpResponseMessage respone = await client.PostAsync(url, content);
                    if (!respone.IsSuccessStatusCode)
                    {
                        return "";
                    }
                    return await respone.Content.ReadAsStringAsync();
                }

            }
            catch (HttpRequestException)
            {
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        private string SaveFile(IFormFile file, ILiteStorage<string> storage)
        {
            if (file == null) return null;

            var id = Guid.NewGuid().ToString();

            using (var stream = file.OpenReadStream())
            {
                storage.Upload(id, file.FileName, stream);
            }

            return id;
        }

        [HttpPost, Route("FetchDanhSachAddin")]
        public async Task<DanhSachAddinResultDTO> FetchAddins([FromBody] AddinInputDTO input)
        {
            var apiKey = config["apiKey"];
            var url = config["urlListAddinTruong"];
            var jsonData = await FetchApiAddin(input.empObj, url, apiKey);
            if (string.IsNullOrEmpty(jsonData))
            {
                return new DanhSachAddinResultDTO
                {
                    message = "Failed to retrieve data or API returned empty",
                    code = 404,
                    result = false
                };
            }
            try
            {
                var apiRespone = JsonConvert.DeserializeObject<DanhSachAddinResultDTO>(jsonData);

                if (apiRespone.data != null && apiRespone.data.Count > 0)
                {
                    var phuLucAddin = database.Table<AQDanhMucAddin>();
                    var phuLucAddinList = phuLucAddin.FindAll().ToList();
                    var phuLucAddinDict = phuLucAddinList.ToDictionary(x => x.IDAddin, x => x);

                    foreach (var item in apiRespone.data)
                    {
                        if (phuLucAddinDict.ContainsKey(item.IDAddin))
                        {
                            var phuLucInfo = phuLucAddinDict[item.IDAddin];
                            item.FileWordName = phuLucInfo.FileWordName;
                            item.FilePdfName = phuLucInfo.FilePdfName;
                        }
                    }
                }

                apiRespone.message = "Success!";
                return apiRespone;
            }
            catch
            {
                return new DanhSachAddinResultDTO
                {
                    message = "Error parsing API data",
                    code = 500,
                    result = false,
                    data = new List<DanhSachAddin>()
                };
            }
        }

        [HttpPost, Route("UpdateAddin")]
        public async Task<ApiResultBaseDO> UpdateAddin([FromBody] AddinInputUpdateDTO input)
        {
            var apiKey = config["apiKey"];
            var url = config["urlUpdateAddin"];
            var jsonData = await UpdateApiAddin(input, url, apiKey);
            if (string.IsNullOrEmpty(jsonData))
            {
                return new ApiResultBaseDO
                {
                    message = "Failed to retrieve data or API returned empty",
                    code = 404,
                    result = false
                };
            }
            try
            {
                var apiRespone = JsonConvert.DeserializeObject<ApiResultBaseDO>(jsonData);
                apiRespone.message = "Success!";
                return apiRespone;
            }
            catch
            {
                return new DanhSachAddinResultDTO
                {
                    message = "Error parsing API data",
                    code = 500,
                    result = false,
                };
            }
        }

        [HttpPost("UpdateAddinFiles")]
        public IActionResult UpdateAddinFiles([FromForm] AddinFileDTO input)
        {
            if (input == null || string.IsNullOrEmpty(input.IDAddin))
            {
                return BadRequest(new { result = false, message = "IDAddin is required" });
            }

            var col = database.Table<AQDanhMucAddin>();
            var record = col.FindOne(x => x.IDAddin == input.IDAddin)
                         ?? new AQDanhMucAddin { IDAddin = input.IDAddin };

            try
            {
                // Xử lý File Word
                if (input.FileWord != null)
                {
                    // Xóa file cũ trong Storage nếu đã tồn tại
                    if (!string.IsNullOrEmpty(record.FileWordId))
                        database.FileStorage.Delete(record.FileWordId);

                    // Lưu file mới
                    record.FileWordId = SaveFile(input.FileWord, database.FileStorage);
                    record.FileWordName = input.FileWord.FileName;
                }

                // Xử lý File PDF
                if (input.FilePdf != null)
                {
                    // Xóa file cũ trong Storage nếu đã tồn tại
                    if (!string.IsNullOrEmpty(record.FilePdfId))
                        database.FileStorage.Delete(record.FilePdfId);

                    // Lưu file mới
                    record.FilePdfId = SaveFile(input.FilePdf, database.FileStorage);
                    record.FilePdfName = input.FilePdf.FileName;
                }

                // Cập nhật record metadata vào bảng AQDanhMucAddin
                col.Upsert(record);
                return Ok(new { result = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { result = false, message = ex.Message });
            }
        }


        [HttpGet("GetFileMeta/{idAddin}")]
        public IActionResult GetFileMeta(string idAddin)
        {
            var col = database.Table<AQDanhMucAddin>();
            var record = col.FindOne(x => x.IDAddin == idAddin);

            if (record == null)
            {
                // Trả về rỗng nếu chưa có record, nhưng result vẫn true để frontend không báo lỗi
                return Ok(new { fileWordName = "", filePdfName = "" });
            }

            return Ok(new
            {
                fileWordName = record.FileWordName,
                filePdfName = record.FilePdfName
            });
        }

        [HttpGet("DownloadFile")]
        public IActionResult DownloadFile(string idAddin, string fileType)
        {
            if (string.IsNullOrEmpty(idAddin) || string.IsNullOrEmpty(fileType))
                return BadRequest("Thiếu thông tin");

            var col = database.Table<AQDanhMucAddin>();
            var record = col.FindOne(x => x.IDAddin == idAddin);

            if (record == null)
                return NotFound("Không tìm thấy thông tin Addin");

            string fileId = null;
            string fileName = "document";

            // Xác định lấy file nào
            if (fileType.ToLower() == "word")
            {
                fileId = record.FileWordId;
                fileName = record.FileWordName;
            }
            else if (fileType.ToLower() == "pdf")
            {
                fileId = record.FilePdfId;
                fileName = record.FilePdfName;
            }

            if (string.IsNullOrEmpty(fileId))
                return NotFound("File chưa được upload");

            // Mở stream từ LiteDB
            var fileInfo = database.FileStorage.FindById(fileId);
            if (fileInfo == null) return NotFound("File không tồn tại trong hệ thống");

            var stream = database.FileStorage.OpenRead(fileId);

            // Trả về file stream. 
            // "application/octet-stream" để trình duyệt tự hiểu là file tải về
            return File(stream, "application/octet-stream", fileName ?? fileInfo.Filename);
        }

        public class DanhSachAddinResultDTO : ApiResultBaseDO
        {
            public List<DanhSachAddin> data { get; set; }
        }

        public class AddinInputDTO
        {
            public string empObj { get; set; }

        }

        public class AddinInputUpdateDTO
        {
            public string IDAddin { get; set; }
            public string GhiChuAddin { get; set; }
            public string GhiChuSale { get; set; }
            public string GhiChuDev { get; set; }
            public string GhiChuSupport { get; set; }
            public double? DonGia { get; set; }
        }

        public class AddinUpdateWithFileDTO
        {
            public string IDAddin { get; set; }
            public string GhiChuAddin { get; set; }
            public string GhiChuSale { get; set; }
            public string GhiChuDev { get; set; }
            public string GhiChuSupport { get; set; }
            public double? DonGia { get; set; }

            public IFormFile FileWord { get; set; }
            public IFormFile FilePdf { get; set; }
        }

        public class AddinFileDTO
        {
            public string IDAddin { get; set; }
            public IFormFile FileWord { get; set; }
            public IFormFile FilePdf { get; set; }
        }

    }
}
