using educlient.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using static educlient.Models.csCase;

namespace educlient.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThongKeController : ControllerBase
    {
        private readonly IThongKeDevService _thongKeDevService;
        private readonly IThongKeSupService _thongKeSupService;
        private readonly IThongKeAqTechService _thongKeAqTechService;

        private readonly IConfiguration config;
        const string TFS_TOKEN_BASE64 = "QVFcdGZzdXNlcjpyY3phdmVsczJ6ZGw2bDZqdDZ6cXRxdGp0YW1wMzQ1NDQyYm9ycXk3cGNyd2doem1icHFx";
        public static string TFS_HOST = Startup.tfsUrl;
        public ThongKeController(IConfiguration cf, IThongKeDevService ThongKeService, IThongKeSupService thongKeSupService, IThongKeAqTechService thongKeAqTechService)
        {
            config = cf;
            _thongKeDevService = ThongKeService;
            _thongKeSupService = thongKeSupService;
            _thongKeAqTechService = thongKeAqTechService;
        }

        [HttpPost, Route("w-coderCaseReport")]
        public Task<XuLyCaseDataResult> CoderCaseReport()
        {
            return _thongKeDevService.CoderCaseReport();
        }

        [HttpPost, Route("w-supCaseReport")]
        public Task<XuLyCaseSupDataResult> supCsData()
        {
            return _thongKeSupService.SupportCaseReport();
        }
        [HttpPost, Route("w-AqCaseReport")]
        public Task<AQReportResult> AqCaseData()
        {
            return _thongKeAqTechService.AqReport();
        }
        [HttpPost, Route("w-PhanBoSoCaseTheoThoiGianChoCoder")]
        public Task<CaseTheoThoiGianChoResult> PhanBoSoCaseTheoThoiGianChoCoder()
        {
            return _thongKeDevService.PhanBoSoCaseTheoThoiGianCoder();
        }

    }
}

