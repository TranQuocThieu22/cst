using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static educlient.Models.csCase;

namespace educlient.Services
{
    public interface IThongKeAqTechService
    {
        Task<AQReportResult> AqReport(DateInput Date);
    }
    public class ThongKeAqTechService : IThongKeAqTechService
    {
        private readonly IThongKeDevService _thongKeDevService;
        private readonly IThongKeSupService _thongKeSupService;
        private readonly IConfiguration config;
        const string TFS_TOKEN_BASE64 = "QVFcdGZzdXNlcjpyY3phdmVsczJ6ZGw2bDZqdDZ6cXRxdGp0YW1wMzQ1NDQyYm9ycXk3cGNyd2doem1icHFx";
        public static string TFS_HOST = Startup.tfsUrl;
        public ThongKeAqTechService(IConfiguration cf, IThongKeDevService ThongKeService, IThongKeSupService thongKeSupService)
        {
            config = cf;
            _thongKeDevService = ThongKeService;
            _thongKeSupService = thongKeSupService;
        }
        public async Task<AQReportResult> AqReport(DateInput Date)
        {
            List<XuLyCasedataDO> dev = await DevFunct(Date);
            List<XuLyCaseSupdataDO> sup = await SupFunct();
            List<AQReportDataDO> aqReport = CalAqReport(sup, dev);
            return new AQReportResult { code = 200, message = "success", result = true, data = aqReport };
        }
        public async Task<List<XuLyCasedataDO>> DevFunct(DateInput Date)
        {
            var wiqlQuery = _thongKeDevService.BuildWiqlQuery();
            var wiqlCaseTodayQuery = _thongKeDevService.BuildWiqlCaseInDayQuery(Date.data);

            var caseIds = await _thongKeDevService.GetCaseIds(wiqlQuery);
            var caseIdsByTodayCase = await _thongKeDevService.GetCaseIds(wiqlCaseTodayQuery);
            if (caseIds == null || !caseIds.Any())
            {
                return new List<XuLyCasedataDO>();
            }
            var caseDetails = await _thongKeDevService.GetCaseDetails(caseIds);
            var casesTodayDetails = await _thongKeDevService.GetCaseDetails(caseIdsByTodayCase);

            var thongTinCases = _thongKeDevService.ProcessCaseDetails(caseDetails);
            var thongTinCasesToday = _thongKeDevService.ProcessCaseDetails(casesTodayDetails);

            var soNgayBaoTri = await _thongKeDevService.tinhSoNgayTreBaoTri(thongTinCases);
            var dataCaseTreVaCanXuLy = _thongKeDevService.tinhdataXuLyCase(soNgayBaoTri);
            var dataTienDoCongViec = _thongKeDevService.TinhSoLuongCaseTodayTheoUser(dataCaseTreVaCanXuLy, thongTinCasesToday, Date.data);
            Console.Write(dataTienDoCongViec);

            return dataTienDoCongViec;
        }
        public async Task<List<XuLyCaseSupdataDO>> SupFunct()
        {
            var wiqlQuery = _thongKeSupService.BuildWiqlQuery();
            var CanTestwiqlQuery = _thongKeSupService.BuildWiqlCanTestQuery();
            var CanGanTagwiqlQuery = _thongKeSupService.BuildWiqlGanTagQuery();
            var TestTreWiqlTestTreQuery = _thongKeSupService.BuildWiqlTestTreQuery();
            var PhanTichTreWiqlTestTreQuery = _thongKeSupService.BuildWiqlPhanTichTreQuery();

            var caseIds = await _thongKeSupService.GetCaseIds(wiqlQuery);
            var caseTestIds = await _thongKeSupService.GetCaseIds(CanTestwiqlQuery);
            var caseGanTagIds = await _thongKeSupService.GetCaseIds(CanGanTagwiqlQuery);
            var caseTestTreIds = await _thongKeSupService.GetCaseIds(TestTreWiqlTestTreQuery);
            var casePhanTichTreIds = await _thongKeSupService.GetCaseIds(PhanTichTreWiqlTestTreQuery);

            if (caseIds == null || !caseIds.Any())
            {
                return new List<XuLyCaseSupdataDO>();
            }

            var caseDetails = await _thongKeSupService.GetCaseDetails(caseIds);
            var caseTestDetails = await _thongKeSupService.GetCaseDetails(caseTestIds);
            var caseGanTagDetails = await _thongKeSupService.GetCaseDetails(caseGanTagIds);
            var caseTestTreDetails = await _thongKeSupService.GetCaseDetails(caseTestTreIds);
            var casePhanTichTreDetails = await _thongKeSupService.GetCaseDetails(casePhanTichTreIds);

            var thongTinCases = _thongKeSupService.ProcessCaseDetails(caseDetails);
            var thongTinCasesTest = _thongKeSupService.ProcessCaseDetails(caseTestDetails);
            var thongTinCasesGanTag = _thongKeSupService.ProcessCaseDetails(caseGanTagDetails);
            var thongTinCasesTestTre = _thongKeSupService.ProcessCaseDetails(caseTestTreDetails);
            var thongTinCasesPhanTichTre = _thongKeSupService.ProcessCaseDetails(casePhanTichTreDetails);

            var SoLuongCanXuLy = _thongKeSupService.SummarizeCanXuLyData(thongTinCases, thongTinCasesTest, thongTinCasesGanTag);
            var SoLuongTestTreCase = _thongKeSupService.CountXuLyTreByCaseTester(thongTinCasesTestTre);
            var SoLuongPhanTichTreCase = _thongKeSupService.CountPhanTichTreByCaseAnalyst(thongTinCasesPhanTichTre);
            var caseXuLyTre = _thongKeSupService.CalculateXuLyTre(thongTinCasesPhanTichTre, thongTinCasesTestTre);

            var supReport = _thongKeSupService.SupReport(caseXuLyTre, SoLuongCanXuLy);
            return supReport;
        }

        public List<AQReportDataDO> CalAqReport(List<XuLyCaseSupdataDO> supData, List<XuLyCasedataDO> devData)
        {
            // Calculate totals for SUP data
            int supTongCase = supData.Sum(x => x.canXuLy);
            int supTreHan = supData.Sum(x => x.XuLyTre);
            int supConHan = Math.Max(supTongCase - supTreHan, 0);
            int supNhanSu = supData.Count > 0 ? supData.Select(x => x.assignedto).Distinct().Count() : 0;

            // Calculate totals for DEV data
            int devTongCase = devData.Sum(x => x.canXuLy);
            int devTreHan = devData.Sum(x => x.XuLyTre);
            int devConHan = Math.Max(devTongCase - devTreHan, 0);
            int devNhanSu = devData.Count > 0 ? devData.Select(x => x.assignedto).Distinct().Count() : 0;

            // Calculate overall totals
            int aqTongCase = supTongCase + devTongCase;
            int aqTreHan = supTreHan + devTreHan;
            int aqConHan = aqTongCase - aqTreHan;
            int aqNhanSu = supNhanSu + devNhanSu;

            // Create the report data
            var reportData = new List<AQReportDataDO>
    {
        new AQReportDataDO
        {
            Name = "AQ",
            TongCase = aqTongCase,
            TreHan = aqTreHan,
            ConHan = aqConHan,
            NhanSu = aqNhanSu
        },
        new AQReportDataDO
        {
            Name = "DEV",
            TongCase = devTongCase,
            TreHan = devTreHan,
            ConHan = devConHan,
            NhanSu = devNhanSu
        },
        new AQReportDataDO
        {
            Name = "SUP",
            TongCase = supTongCase,
            TreHan = supTreHan,
            ConHan = supConHan,
            NhanSu = supNhanSu
        }
    };

            return reportData;
        }
    }
}


