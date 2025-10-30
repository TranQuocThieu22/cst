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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static MongoDB.Bson.Serialization.Serializers.SerializerHelper;


namespace educlient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DanhSachTruongController : ControllerBase
    {
        private readonly IDbLiteContext database;
        private readonly IConfiguration config;

        public DanhSachTruongController(IDbLiteContext dataContext, IConfiguration cf)
        {
            database = dataContext;
            config = cf;
        }

        async Task<string> GetDataTruong(string url, string apiKey)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(apiKey))
            {
                return "Url or Api key is required!";
            }
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        return "";
                    }
                    return await response.Content.ReadAsStringAsync();
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

        async Task<string> FetchApiAddinTruong(string IDTruong, string url, string apiKey)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(IDTruong))
            {
                return "Url, Api key, and idTruong are required!";
            }
            try 
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                    var payload = new {IDTruong = IDTruong };
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

        [HttpGet, Route("GetApiTruong")]
        public async Task<SchoolDataApiResult> GetAll()
        {
            var apiKey = config["apiKeyListTruong"];
            var url = config["urlListTruong"];
            string jsonData = await GetDataTruong(url, apiKey);

            if (string.IsNullOrEmpty(jsonData))
            {
                return new SchoolDataApiResult
                {
                    message = "Failed to retrieve data or API returned empty",
                    code = 404,
                    result = false,
                    data = new List<SchoolDataApiDTO>()
                };
            }

            try
            {
                var apiResponse = JsonConvert.DeserializeObject<SchoolDataApiResult>(jsonData);
                return apiResponse;
            }
            catch (JsonException ex)
            {
                return new SchoolDataApiResult
                {
                    message = "Error parsing API data: " + ex.Message,
                    code = 500,
                    result = false,
                    data = new List<SchoolDataApiDTO>()
                };
            }
        }

        [HttpPost, Route("GetApiAddinTruong")]
        public async Task<ApiResultBaseDO> FetchAddinTruong([FromBody] SchoolAddinInput input)
        {
            var apiKey = config["apiKeyListAddinTruong"];
            var url = config["urlListAddinTruong"];
            var jsonData = await FetchApiAddinTruong(input.IDTruong, url, apiKey);
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
                var apiRespone = JsonConvert.DeserializeObject<SchoolAddinApiResult>(jsonData);
                apiRespone.message = "Success!";
                return apiRespone;
            }
            catch
            {
                return new SchoolAddinApiResult
                {
                    message = "Error parsing API data",
                    code = 500,
                    result = false,
                    data = new List<SchoolAddinDTO>()
                };
            }
        }

        [HttpPost]
        public ApiResultBaseDO AddSchoolProfile([FromBody] SchoolProfileInsertDTO[] inputData)
        {
            if (inputData.Any(item => string.IsNullOrWhiteSpace(item.IdTruong) ||
                               string.IsNullOrWhiteSpace(item.MaTruong) ||
                               string.IsNullOrWhiteSpace(item.TenTruong)) 
                || inputData == null || inputData.Length == 0) 
            {
                return new ApiResultBaseDO
                {
                    message = "Lỗi: Không tìm thấy thông tin trường",
                    code = 400, 
                    result = false,
                };
            }

            var duplicateIdsInInput = inputData
                .GroupBy(x => x.IdTruong)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateIdsInInput.Any())
            {
                return new ApiResultBaseDO
                {
                    message = $"Lỗi: Dữ liệu đầu vào bị trùng lặp IdTruong: {string.Join(", ", duplicateIdsInInput)}",
                    code = 400,
                    result = false,
                };
            }

            var schooleProfileTable = database.Table<SchoolProfile>();
            var inputIds = inputData.Select(x => x.IdTruong).ToList();

            var existingIdsInDb = schooleProfileTable
                .Query()
                .Where(x => inputIds.Contains(x.IdTruong))
                .Select(x => x.IdTruong)
                .ToList();

            if (existingIdsInDb.Any())
            {
                return new ApiResultBaseDO
                {
                    message = $"Lỗi: Các IdTruong sau đã tồn tại trong cơ sở dữ liệu: {string.Join(", ", existingIdsInDb)}",
                    code = 409,
                    result = false,
                };
            }

            var schoolProfiles = inputData.Select(item => new SchoolProfile
            {
                IdTruong = item.IdTruong,
                MaTruong = item.MaTruong,
                TenTruong = item.TenTruong,
                ThoiDiemTrienKhai = item.ThoiDiemTrienKhai,
                SoNamDungEdusoft = item.SoNamDungEdusoft,
                NgayHetHanNangCap = item.NgayHetHanNangCap,
                DiaChiTruong = item.DiaChiTruong,
                HieuTruong = item.HieuTruong,
                HieuPho = item.HieuPho,
                TruongPhongDaoTao = item.TruongPhongDaoTao,
                TruongPhongKhaoThi = item.TruongPhongKhaoThi,
                TruongPhongTaiVu = item.TruongPhongTaiVu,
                Admin = item.Admin,
                GhiChuKinhDoanh = item.GhiChuKinhDoanh,
                GhiChuKyThuat = item.GhiChuKyThuat,
                GhiChuChamSoc = item.GhiChuChamSoc,
                DanhSachAddin = item.DanhSachAddin,
                LuuYXuLyDacThu = item.LuuYXuLyDacThu,
                ServerInfo = item.ServerInfo
            }).ToList();

            if (schoolProfiles.Count == 1)
            {
                var newId = schooleProfileTable.Insert(schoolProfiles[0]);
                var existingRecord = schooleProfileTable.FindById(newId);
                var schoolProfile = new SchoolProfileDTO
                {
                    IdTruong = existingRecord.IdTruong,
                    MaTruong = existingRecord.MaTruong,
                    TenTruong = existingRecord.TenTruong,
                    ThoiDiemTrienKhai = existingRecord.ThoiDiemTrienKhai,
                    SoNamDungEdusoft = existingRecord.SoNamDungEdusoft,
                    NgayHetHanNangCap = existingRecord.NgayHetHanNangCap,
                    DiaChiTruong = existingRecord.DiaChiTruong,
                    HieuTruong = existingRecord.HieuTruong,
                    HieuPho = existingRecord.HieuPho,
                    TruongPhongDaoTao = existingRecord.TruongPhongDaoTao,
                    TruongPhongKhaoThi = existingRecord.TruongPhongKhaoThi,
                    TruongPhongTaiVu = existingRecord.TruongPhongTaiVu,
                    Admin = existingRecord.Admin,
                    GhiChuKinhDoanh = existingRecord.GhiChuKinhDoanh,
                    GhiChuKyThuat = existingRecord.GhiChuKyThuat,
                    GhiChuChamSoc = existingRecord.GhiChuChamSoc,
                    DanhSachAddin = existingRecord.DanhSachAddin,
                    LuuYXuLyDacThu = existingRecord.LuuYXuLyDacThu,
                    ServerInfo = existingRecord.ServerInfo
                };
                var returnList = new List<SchoolProfileDTO>();
                returnList.Add(schoolProfile);

                return new SchoolProfileInsertResultDTO
                {
                    message = "Insert Success",
                    code = 200,
                    result = true,
                    data = returnList,
                    numberOfNewRecord = 1
                };
            }
            else
            {                
                var numberOfNewRecord = schooleProfileTable.InsertBulk(schoolProfiles);
                return new SchoolProfileInsertResultDTO
                {
                    message = "Insert Success",
                    code = 200,
                    result = true,
                    data = new List<SchoolProfileDTO>(),
                    numberOfNewRecord = numberOfNewRecord

                };
            }
        }

        [HttpGet]
        public ApiResultBaseDO GetSchoolProfiles()
        {
            var schooleProfileTable = database.Table<SchoolProfile>();
            var schoolProfiles = schooleProfileTable.FindAll().ToList();
            var resultData = schoolProfiles.Select(existingRecord => new SchoolProfileDTO
            {
                IdTruong = existingRecord.IdTruong,
                MaTruong = existingRecord.MaTruong,
                TenTruong = existingRecord.TenTruong,
                ThoiDiemTrienKhai = existingRecord.ThoiDiemTrienKhai,
                SoNamDungEdusoft = existingRecord.SoNamDungEdusoft,
                NgayHetHanNangCap = existingRecord.NgayHetHanNangCap,
                DiaChiTruong = existingRecord.DiaChiTruong,
                HieuTruong = existingRecord.HieuTruong,
                HieuPho = existingRecord.HieuPho,
                TruongPhongDaoTao = existingRecord.TruongPhongDaoTao,
                TruongPhongKhaoThi = existingRecord.TruongPhongKhaoThi,
                TruongPhongTaiVu = existingRecord.TruongPhongTaiVu,
                Admin = existingRecord.Admin,
                GhiChuKinhDoanh = existingRecord.GhiChuKinhDoanh,
                GhiChuKyThuat = existingRecord.GhiChuKyThuat,
                GhiChuChamSoc = existingRecord.GhiChuChamSoc,
                DanhSachAddin = existingRecord.DanhSachAddin,
                LuuYXuLyDacThu = existingRecord.LuuYXuLyDacThu,
                ServerInfo = existingRecord.ServerInfo
            }).ToList();
            return new SchoolProfileResultDTO
            {
                message = "Success",
                code = 200,
                result = true,
                data = resultData
            };
        }

        [HttpPatch, Route("{id}")]
        public ApiResultBaseDO UpdateSchoolProfile(string id, [FromBody] SchoolProfileUpdateDTO inputData)
        {
            if (inputData == null)
            {
                return new ApiResultBaseDO
                {
                    message = "Lỗi: Dữ liệu đầu vào không được rỗng",
                    code = 400, 
                    result = false,
                };
            }

            var schoolProfileTable = database.Table<SchoolProfile>();
            var existingRecord = schoolProfileTable.FindById(id);
            if (existingRecord == null)
            {
                return new ApiResultBaseDO
                {
                    code = 404,
                    message = "Data not found"
                };
            }

            existingRecord.ThoiDiemTrienKhai = inputData.ThoiDiemTrienKhai;
            existingRecord.SoNamDungEdusoft = inputData.SoNamDungEdusoft;
            existingRecord.NgayHetHanNangCap = inputData.NgayHetHanNangCap;
            existingRecord.DiaChiTruong = inputData.DiaChiTruong;
            existingRecord.HieuTruong = inputData.HieuTruong;
            existingRecord.HieuPho = inputData.HieuPho;
            existingRecord.TruongPhongDaoTao = inputData.TruongPhongDaoTao;
            existingRecord.TruongPhongKhaoThi = inputData.TruongPhongKhaoThi;
            existingRecord.TruongPhongTaiVu = inputData.TruongPhongTaiVu;
            existingRecord.Admin = inputData.Admin;
            existingRecord.GhiChuKinhDoanh = inputData.GhiChuKinhDoanh;
            existingRecord.GhiChuKyThuat = inputData.GhiChuKyThuat;
            existingRecord.GhiChuChamSoc = inputData.GhiChuChamSoc;
            existingRecord.DanhSachAddin = inputData.DanhSachAddin;
            existingRecord.LuuYXuLyDacThu = inputData.LuuYXuLyDacThu;
            existingRecord.ServerInfo = inputData.ServerInfo;

            bool updateSuccess = schoolProfileTable.Update(existingRecord);

            if (!updateSuccess)
            {
                return new ApiResultBaseDO
                {
                    message = "Lỗi: Cập nhật thất bại",
                    code = 500, 
                    result = false,
                };
            }

            var updatedSchoolProfileDTO = new SchoolProfileDTO
            {
                IdTruong = existingRecord.IdTruong,
                MaTruong = existingRecord.MaTruong,
                TenTruong = existingRecord.TenTruong,
                ThoiDiemTrienKhai = existingRecord.ThoiDiemTrienKhai,
                SoNamDungEdusoft = existingRecord.SoNamDungEdusoft,
                NgayHetHanNangCap = existingRecord.NgayHetHanNangCap,
                DiaChiTruong = existingRecord.DiaChiTruong,
                HieuTruong = existingRecord.HieuTruong,
                HieuPho = existingRecord.HieuPho,
                TruongPhongDaoTao = existingRecord.TruongPhongDaoTao,
                TruongPhongKhaoThi = existingRecord.TruongPhongKhaoThi,
                TruongPhongTaiVu = existingRecord.TruongPhongTaiVu,
                Admin = existingRecord.Admin,
                GhiChuKinhDoanh = existingRecord.GhiChuKinhDoanh,
                GhiChuKyThuat = existingRecord.GhiChuKyThuat,
                GhiChuChamSoc = existingRecord.GhiChuChamSoc,
                DanhSachAddin = existingRecord.DanhSachAddin,
                LuuYXuLyDacThu = existingRecord.LuuYXuLyDacThu,
                ServerInfo = existingRecord.ServerInfo
            };

            var returnList = new List<SchoolProfileDTO> { updatedSchoolProfileDTO };

            return new SchoolProfileUpdateResultDTO
            {
                message = "Update Success",
                code = 200,
                result = true,
                data = returnList,
                numberOfNewRecord = 1
            };
        }
    }

    public class SchoolProfileDTO
    {
        public string IdTruong { get; set; }
        public string MaTruong { get; set; }
        public string TenTruong { get; set; }
        public DateTime? ThoiDiemTrienKhai { get; set; }
        public int? SoNamDungEdusoft { get; set; }
        public DateTime? NgayHetHanNangCap { get; set; }
        public string DiaChiTruong { get; set; }
        public ContactPerson HieuTruong { get; set; }
        public ContactPerson HieuPho { get; set; }
        public ContactPerson TruongPhongDaoTao { get; set; }
        public ContactPerson TruongPhongKhaoThi { get; set; }
        public ContactPerson TruongPhongTaiVu { get; set; }
        public ContactPerson Admin { get; set; }
        public string GhiChuKinhDoanh { get; set; }
        public string GhiChuKyThuat { get; set; }
        public string GhiChuChamSoc { get; set; }
        public AddinModule DanhSachAddin { get; set; }
        public LuuYDacThu LuuYXuLyDacThu { get; set; }
        public ThongTinServer ServerInfo { get; set; }
    }

    public class SchoolProfileResultDTO : ApiResultBaseDO
    {
        public List<SchoolProfileDTO> data { get; set; }
    }

    public class SchoolDataApiDTO
    {
        public string IdTruong { get; set; }
        public string MaTruong { get; set; }
        public string TenTruong { get; set; }
    }

    public class SchoolDataApiResult : ApiResultBaseDO
    {
        public List<SchoolDataApiDTO> data { get; set; }
    }

    public class SchoolProfileInsertDTO
    {
        public string IdTruong { get; set; }
        public string MaTruong { get; set; }
        public string TenTruong { get; set; }
        public DateTime? ThoiDiemTrienKhai { get; set; }
        public int? SoNamDungEdusoft { get; set; }
        public DateTime? NgayHetHanNangCap { get; set; }
        public string DiaChiTruong { get; set; }
        public ContactPerson HieuTruong { get; set; }
        public ContactPerson HieuPho { get; set; }
        public ContactPerson TruongPhongDaoTao { get; set; }
        public ContactPerson TruongPhongKhaoThi { get; set; }
        public ContactPerson TruongPhongTaiVu { get; set; }
        public ContactPerson Admin { get; set; }
        public string GhiChuKinhDoanh { get; set; }
        public string GhiChuKyThuat { get; set; }
        public string GhiChuChamSoc { get; set; }
        public AddinModule DanhSachAddin { get; set; }
        public LuuYDacThu LuuYXuLyDacThu { get; set; }
        public ThongTinServer ServerInfo { get; set; }
    }

    public class SchoolProfileInsertResultDTO : ApiResultBaseDO
    {
        public List<SchoolProfileDTO> data { get; set; }
        public int numberOfNewRecord { get; set; }
    }

    public class SchoolProfileUpdateDTO
    {
        //public string IdTruong { get; set; }
        //public string MaTruong { get; set; }
        //public string TenTruong { get; set; }
        public DateTime ThoiDiemTrienKhai { get; set; }
        public int? SoNamDungEdusoft { get; set; }
        public DateTime NgayHetHanNangCap { get; set; }
        public string DiaChiTruong { get; set; }
        public ContactPerson HieuTruong { get; set; }
        public ContactPerson HieuPho { get; set; }
        public ContactPerson TruongPhongDaoTao { get; set; }
        public ContactPerson TruongPhongKhaoThi { get; set; }
        public ContactPerson TruongPhongTaiVu { get; set; }
        public ContactPerson Admin { get; set; }
        public string GhiChuKinhDoanh { get; set; }
        public string GhiChuKyThuat { get; set; }
        public string GhiChuChamSoc { get; set; }
        public AddinModule DanhSachAddin { get; set; }
        public LuuYDacThu LuuYXuLyDacThu { get; set; }
        public ThongTinServer ServerInfo { get; set; }
    }

    public class SchoolProfileUpdateResultDTO : ApiResultBaseDO
    {
        public List<SchoolProfileDTO> data { get; set; }
        public int numberOfNewRecord { get; set; }
    }

    public class SchoolAddinDTO
    {
        public string IDAddin { get; set; }
        public string IDAddinParent { get; set; }
        public string MaAddin { get; set; }
        public string TenAddin { get; set; }
        public string GhiChuAddin { get; set; }
        public bool IsDaMua { get; set; }
        public ThongTinDaMua ThongTinDaMua { get; set; }
    }

    public class ThongTinDaMua
    {
        public DateTime? NgayMua { get; set; }
        public string UserCapAddin { get; set; }
        public string GhiChuCapAddin { get; set; }
        public string DanhSachMaDVPC { get; set; }
    }

    public class SchoolAddinApiResult : ApiResultBaseDO
    {
        public List<SchoolAddinDTO> data { get; set; }
    }

    public class SchoolAddinInput
    {
        public string IDTruong { get; set; }
    }
}
