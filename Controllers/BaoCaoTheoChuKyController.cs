using educlient.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace educlient.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaoCaoTheoChuKyController : ControllerBase
    {
        private readonly IBaoCaoTheoChuKyService _BaoCaoTheoChuKyService;
        private readonly IConfiguration config;
        public BaoCaoTheoChuKyController(IConfiguration cf, IBaoCaoTheoChuKyService BaoCaoTheoChuKyService)
        {
            config = cf;
            _BaoCaoTheoChuKyService = BaoCaoTheoChuKyService;
        }
        [HttpPost, Route("dev")]
        public Task<BaoCaoChuKyChoDevResult> BaoCaoDevReport([FromBody] BaoCaoReportInput Date)
        {
            return _BaoCaoTheoChuKyService.ChuKyDevReport(Date.FromDate, Date.ToDate);
        }
        [HttpPost, Route("sup")]
        public Task<BaoCaoChuKyChoSupResult> BaoCaoSupReport([FromBody] BaoCaoReportInput Date)
        {
            return _BaoCaoTheoChuKyService.ChuKySupReport(Date.FromDate, Date.ToDate);
        }
    }
    public class BaoCaoReportInput
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}
