using educlient.Data;
using educlient.Models;
using educlient.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace educlient.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KetQuaLamViecCaNhanController : Controller
    {
        private readonly IDbLiteContext database;
        private readonly IKetQuaLamViecCaNhan ketQuaLamViecCaNhanService;
        private readonly IConfiguration config;
        const string TFS_TOKEN_BASE64 = "QVFcdGZzdXNlcjpyY3phdmVsczJ6ZGw2bDZqdDZ6cXRxdGp0YW1wMzQ1NDQyYm9ycXk3cGNyd2doem1icHFx";
        public static string TFS_HOST = Startup.tfsUrl;
        public KetQuaLamViecCaNhanController(IConfiguration cf, IKetQuaLamViecCaNhan KetQuaLamViecCaNhanService)
        {
            config = cf;
            ketQuaLamViecCaNhanService = KetQuaLamViecCaNhanService;
        }

        [HttpPost("KetQuaLamViecCaNhan")]
        public async Task<KetQuaLamViecCaNhanResult> KetQuaLamViecCaNhanApi(KetQuaLamViecCaNhanInput data)
        {

            return await ketQuaLamViecCaNhanService.KetQuaLamViecCaNhanReturn(data.user, data.year);
        }

        // POST api/<KetQuaLamViecCaNhanController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<KetQuaLamViecCaNhanController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<KetQuaLamViecCaNhanController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
