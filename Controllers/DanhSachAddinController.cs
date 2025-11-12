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


        [HttpPost, Route("FetchDanhSachAddin")]
        public async Task<DanhSachAddinResultDTO> FetchAddins([FromBody] AddinInputDTO input)
        {
            var apiKey = config["apiKeyListAddinTruong"];
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

        public class DanhSachAddinResultDTO : ApiResultBaseDO
        {
            public List<DanhSachAddin> data { get; set; }
        }

        public class AddinInputDTO
        {
            public string empObj { get; set; }

        }

    }
}
