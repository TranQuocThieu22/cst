using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static educlient.Models.csCase;

namespace educlient.Services
{
    public interface IBaoCaoTheoChuKyService
    {
        Task<BaoCaoChuKyChoDevResult> ChuKyDevReport(DateTime startDate, DateTime endDate);
        Task<BaoCaoChuKyChoSupResult> ChuKySupReport(DateTime startDate, DateTime endDate);
    }
    public class BaoCaoTheoChuKyService : IBaoCaoTheoChuKyService
    {
        private readonly IConfiguration config;
        const string TFS_TOKEN_BASE64 = "QVFcdGZzdXNlcjpyY3phdmVsczJ6ZGw2bDZqdDZ6cXRxdGp0YW1wMzQ1NDQyYm9ycXk3cGNyd2doem1icHFx";
        public static string TFS_HOST = Startup.tfsUrl;
        private readonly IThongKeDevService _thongKeDevService;
        private readonly IThongKeSupService _thongKeSupService;
        public BaoCaoTheoChuKyService(IConfiguration cf, IThongKeDevService ThongKeService, IThongKeSupService thongKeSupService)
        {
            config = cf;
            _thongKeDevService = ThongKeService;
            _thongKeSupService = thongKeSupService;
        }
        public async Task<BaoCaoChuKyChoDevResult> ChuKyDevReport(DateTime startDate, DateTime endDate)
        {
            var wiqlQuery = BuildWiqlQuery(startDate, endDate);

            var caseIds = await _thongKeDevService.GetCaseIds(wiqlQuery);

            var caseDetails = await GetCaseDetails(caseIds);

            var thongTinCases = _thongKeDevService.ProcessCaseDetails(caseDetails);

            var data1 = await CalculatedataEstimateTimeAndActualTime(thongTinCases, startDate, endDate);
            var data2 = await CalculateTongCaseAndLoaiCaseDataDO(thongTinCases);

            return new BaoCaoChuKyChoDevResult { code = 200, message = "success", result = true, dataEstimateTimeAndActualTime = data1, dataTongCaseAndLoaiCase = data2 };
        }

        public async Task<BaoCaoChuKyChoSupResult> ChuKySupReport(DateTime startDate, DateTime endDate)
        {
            var supQuery = BuildSupWiqlQuery(startDate, endDate);

            var supCaseIds = await _thongKeDevService.GetCaseIds(supQuery);

            var supCaseDetails = await GetCaseDetails(supCaseIds);

            var supThongTinCases = _thongKeSupService.ProcessCaseDetails(supCaseDetails);
            var caseSupXuLy = await CaseSupXuLy(supThongTinCases);

            return new BaoCaoChuKyChoSupResult { code = 200, message = "success", result = true, data = caseSupXuLy };
        }
        public string BuildWiqlQuery(DateTime startDate, DateTime endDate)
        {
            return $@"{{
        ""query"": ""SELECT [System.Id]
        FROM WorkItems
        WHERE [System.TeamProject] = 'Edusoft.Net-CS'
        AND [System.WorkItemType] <> ''
        AND ([System.State] = 'Đóng case' OR [System.State] = 'Đã xử lý')
        AND [System.AssignedTo] NOT IN (
            'tiepnhansup <AQ\\tiepnhansup>',
            'thanh <AQ\\thanh>',
            'havt <AQ\\havt>',
            'giaminh <AQ\\duongminh>',
            'root <AQ\\root>',
            'tiepnhandev <AQ\\tiepnhandev>',
            'admin <AQ\\admin>',
            'thuyduong <AQ\\thuyduong>',
            'thuannam <AQ\\thuannam>',
            'mtien <AQ\\mtien>',
            'tiepnhansale <AQ\\tiepnhansale>'
        )
        AND [Microsoft.VSTS.Common.ResolvedDate] >= '{startDate:yyyy-MM-ddTHH:mm:ss.0000000}'
        AND [Microsoft.VSTS.Common.ResolvedDate] <= '{endDate:yyyy-MM-ddTHH:mm:ss.0000000}'
        ORDER BY [Microsoft.VSTS.Common.ResolvedDate]""
    }}";

        }
        public string BuildSupWiqlQuery(DateTime startDate, DateTime endDate)
        {
            return $@"{{
    ""query"": ""SELECT [System.Id], [System.WorkItemType], [System.Title], [System.AssignedTo], [System.State], [System.Tags]
    FROM WorkItems
    WHERE [System.TeamProject] = 'Edusoft.Net-CS'
    AND (
        (
            [System.WorkItemType] <> ''
            AND [AQ.CaseAnalyst] IN (
                'thanh <AQ\\thanh>',
                'havt <AQ\\havt>',
                'thuannam <AQ\\thuannam>',
                'thuyduong <AQ\\thuyduong>',
                'mtien <AQ\\mtien>',
                'giaminh <AQ\\duongminh>'
            )
            AND [System.CreatedDate] >= '{startDate:yyyy-MM-ddTHH:mm:ss.0000000}'
            AND [System.CreatedDate] <= '{endDate:yyyy-MM-ddTHH:mm:ss.0000000}'
            AND [AQ.ReqState] = '01 – Đã phân tích xong'
        )
        OR (
            [AQ.CaseTester] IN (
                'thanh <AQ\\thanh>',
                'havt <AQ\\havt>',
                'thuannam <AQ\\thuannam>',
                'thuyduong <AQ\\thuyduong>',
                'mtien <AQ\\mtien>',
                'giaminh <AQ\\duongminh>'
            )
            AND [Microsoft.VSTS.Common.ResolvedDate] >= '{startDate:yyyy-MM-ddTHH:mm:ss.0000000}'
            AND [Microsoft.VSTS.Common.ResolvedDate] <= '{endDate:yyyy-MM-ddTHH:mm:ss.0000000}'
            AND [AQ.TestState] = '01 - Đã kiểm tra và kết quả đúng'
        )
        OR (
            [System.AssignedTo] IN (
                'thanh <AQ\\thanh>',
                'havt <AQ\\havt>',
                'thuannam <AQ\\thuannam>',
                'thuyduong <AQ\\thuyduong>',
                'mtien <AQ\\mtien>',
                'giaminh <AQ\\duongminh>'
            )
            AND [Microsoft.VSTS.Common.ResolvedDate] >= '{startDate:yyyy-MM-ddTHH:mm:ss.0000000}'
            AND [Microsoft.VSTS.Common.ResolvedDate] <= '{endDate:yyyy-MM-ddTHH:mm:ss.0000000}'
            AND (
                [System.State] = 'Đóng case' OR [System.State] = 'Đã xử lý'
            )
        )
    )
    ORDER BY [Microsoft.VSTS.Common.ResolvedDate]""
}}";


        }


        public async Task<List<EstimateTimeAndActualTimeDataDO>> CalculatedataEstimateTimeAndActualTime(List<ThongTinCase> data, DateTime startDate, DateTime endDate)
        {
            var result = data
       .Where(x => !string.IsNullOrEmpty(x.assignedto))
       .GroupBy(x => x.assignedto)
       .Select(group => new EstimateTimeAndActualTimeDataDO
       {
           NguoiXuLy = group.Key,
           TotalEstimateTime = group.Sum(x => x.estimatetime ?? 0),
           TotalActualTime = group.Sum(x => CalculateActualTime(x, startDate, endDate)),
           TotalTongCase = group.Count()
       })
       .OrderByDescending(x => x.TotalEstimateTime)
       .ToList();


            return result;
        }
        public async Task<List<caseXuLySupDataDO>> CaseSupXuLy(List<ThongTinCaseSup> data)
        {
            var validNguoiXuLy = new HashSet<string>
    {
        "thanh <AQ\\thanh>",
        "havt <AQ\\havt>",
        "thuannam <AQ\\thuannam>",
        "thuyduong <AQ\\thuyduong>",
        "mtien <AQ\\mtien>",
        "giaminh <AQ\\duongminh>"
    };

            var result = validNguoiXuLy.Select(person => new caseXuLySupDataDO
            {
                NguoiXuLy = person,
                TotalTongCase = 0,
                ToTallPhanTich = 0,
                TotalTest = 0,
                TotalHuongDan = 0
            }).ToList();

            foreach (var caseItem in data)
            {
                var involvedPeople = new HashSet<string>();

                // Check PhanTich
                if (validNguoiXuLy.Contains(caseItem.caseanalyst))
                {
                    UpdatePersonStats(result, caseItem.caseanalyst, "PhanTich");
                    involvedPeople.Add(caseItem.caseanalyst);
                }

                // Check Test
                if (validNguoiXuLy.Contains(caseItem.casetester))
                {
                    UpdatePersonStats(result, caseItem.casetester, "Test");
                    involvedPeople.Add(caseItem.casetester);
                }

                // Check HuongDan (assignedto)
                if (validNguoiXuLy.Contains(caseItem.assignedto))
                {
                    UpdatePersonStats(result, caseItem.assignedto, "HuongDan");
                    involvedPeople.Add(caseItem.assignedto);
                }

                // Increment TotalTongCase for each involved person
                foreach (var person in involvedPeople)
                {
                    result.First(r => r.NguoiXuLy == person).TotalTongCase++;
                }
            }

            return result;
        }
        public Task<List<TongCaseAndLoaiCaseDataDO>> CalculateTongCaseAndLoaiCaseDataDO(List<ThongTinCase> data)
        {
            var result = data
                .Where(x => !string.IsNullOrEmpty(x.assignedto))
                .GroupBy(x => x.assignedto)
                .Select(group => new TongCaseAndLoaiCaseDataDO
                {
                    NguoiXuLy = group.Key,
                    TongCase = group.Count(),
                    BFAndEX = group.Count(x => x.loaicase != null &&
               (x.loaicase.Contains("BF", StringComparison.OrdinalIgnoreCase) ||
                x.loaicase.Contains("EX", StringComparison.OrdinalIgnoreCase))),
                    NF = group.Count(x => x.loaicase != null && x.loaicase.Contains("NF", StringComparison.OrdinalIgnoreCase)),
                    CV = group.Count(x => x.loaicase != null && x.loaicase.Contains("CV", StringComparison.OrdinalIgnoreCase))
                })
                .ToList();

            return Task.FromResult(result);
        }
        public async Task<TfsCaseDetailModel> GetCaseDetails(List<workItemBase> caseIds)
        {
            const int batchSize = 200; // Adjust this value based on your needs
            string sTfsFieldList = "System.AssignedTo,System.Id,AQ.Customer,System.CreatedDate,System.Title,System.State,AQ.TargetDate,AQ.ReleaseDate,Microsoft.VSTS.Common.StateChangeDate,AQ.MailTo,AQ.Priority,AQ.CaseType,AQ.Module,AQ.Comment,AQ.ContractType,AQ.PriorityType,AQ.UserGuideRequested,AQ.ReviewCase,System.State,AQ.EstimateTime,AQ.DetailDate1,AQ.DetailDate2,AQ.DetailDate3,AQ.DetailDate4,AQ.DetailDate5,AQ.DetailDate6,AQ.DetailDate7,AQ.DetailDate8,AQ.DetailDate9,AQ.DetailDate10,AQ.DetailActualTime1,AQ.DetailActualTime2,AQ.DetailActualTime3,AQ.DetailActualTime4,AQ.DetailActualTime5,AQ.DetailActualTime6,AQ.DetailActualTime7,AQ.DetailActualTime8,AQ.DetailActualTime9,AQ.DetailActualTime10,AQ.CaseAnalyst,AQ.CaseTester";

            var result = new TfsCaseDetailModel
            {
                count = 0,
                value = new List<workItem0>()
            };

            for (int i = 0; i < caseIds.Count; i += batchSize)
            {
                var batch = caseIds.Skip(i).Take(batchSize).ToList();
                string sCaseList = string.Join(",", batch.Select(s => s.id));

                var tmpjson = await DoTfsQueryData(TFS_HOST,
                    $"tfs/aq/Edusoft.Net-CS/_apis/wit/workitems?ids={sCaseList}&fields={sTfsFieldList}",
                    "", "", TFS_TOKEN_BASE64);

                var batchResult = JsonConvert.DeserializeObject<TfsCaseDetailModel>(tmpjson);

                if (batchResult != null)
                {
                    result.count += batchResult.count;
                    result.value.AddRange(batchResult.value);
                }
            }

            return result;
        }
        public async Task<string> DoTfsQueryData(string pTfsHost, string pbaseUrl, string pQueryAppend, string pPOSTBody, string pBasicToken)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(pTfsHost);
                    client.DefaultRequestHeaders.Add("User-Agent", "AQTech TFS program");
                    var u = config["tfsAcc"];
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(UTF8Encoding.UTF8.GetBytes(u)));
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var baseUrl = pbaseUrl;
                    if (pQueryAppend.Length > 0)
                        baseUrl += pQueryAppend;

                    HttpResponseMessage res;

                    if (pPOSTBody.Length > 0)
                    {
                        string reqBody = pPOSTBody;
                        var reqBody0 = new StringContent(reqBody, Encoding.UTF8, "application/json");
                        res = await client.PostAsync(baseUrl, reqBody0);
                    }
                    else
                        res = await client.GetAsync(baseUrl);

                    try
                    {
                        res.EnsureSuccessStatusCode();
                        using (HttpContent content = res.Content)
                        {
                            string data = await content.ReadAsStringAsync();
                            return data; // nullable
                        }
                    }
                    finally
                    {
                        res.Dispose();
                    }
                }
            }
            catch
            {
                // throw xx;
                return "";
            }
        }

        private float CalculateActualTime(ThongTinCase caseData, DateTime startDate, DateTime endDate)
        {
            float totalActualTime = 0;

            var detailDates = new[]
            {
        caseData.detaildate1, caseData.detaildate2, caseData.detaildate3, caseData.detaildate4, caseData.detaildate5,
        caseData.detaildate6, caseData.detaildate7, caseData.detaildate8, caseData.detaildate9, caseData.detaildate10
    };

            var detailActualTimes = new[]
            {
        caseData.detailactualtime1, caseData.detailactualtime2, caseData.detailactualtime3, caseData.detailactualtime4, caseData.detailactualtime5,
        caseData.detailactualtime6, caseData.detailactualtime7, caseData.detailactualtime8, caseData.detailactualtime9, caseData.detailactualtime10
    };

            bool hasDetailData = false;

            for (int i = 0; i < 10; i++)
            {
                if (detailDates[i].HasValue && detailDates[i].Value >= startDate && detailDates[i].Value <= endDate)
                {
                    totalActualTime += detailActualTimes[i] ?? 0;
                    hasDetailData = true;
                }
            }

            if (!hasDetailData)
            {
                totalActualTime = caseData.actualtime ?? 0;
            }

            return totalActualTime;
        }
        private void UpdatePersonStats(List<caseXuLySupDataDO> result, string person, string role)
        {
            var personStats = result.First(r => r.NguoiXuLy == person);
            switch (role)
            {
                case "PhanTich":
                    personStats.ToTallPhanTich++;
                    break;
                case "Test":
                    personStats.TotalTest++;
                    break;
                case "HuongDan":
                    personStats.TotalHuongDan++;
                    break;
            }
        }


    }

    public class BaoCaoChuKyChoDevResult : ApiResultBaseDO
    {
        public List<EstimateTimeAndActualTimeDataDO> dataEstimateTimeAndActualTime { get; set; }
        public List<TongCaseAndLoaiCaseDataDO> dataTongCaseAndLoaiCase { get; set; }
    }
    public class BaoCaoChuKyChoSupResult : ApiResultBaseDO
    {
        public List<caseXuLySupDataDO> data { get; set; }
    }
    public class caseXuLySupDataDO
    {
        public int TotalTongCase { get; set; }
        public int ToTallPhanTich { get; set; }
        public int TotalTest { get; set; }
        public int TotalHuongDan { get; set; }
        public string NguoiXuLy { get; set; }

    }
    public class EstimateTimeAndActualTimeDataDO
    {
        public int TotalTongCase { get; set; }
        public float TotalEstimateTime { get; set; }
        public float TotalActualTime { get; set; }
        public string NguoiXuLy { get; set; }
    }
    public class TongCaseAndLoaiCaseDataDO
    {
        public int TongCase { get; set; }
        public int BFAndEX { get; set; }
        public int NF { get; set; }
        public int CV { get; set; }
        public string NguoiXuLy { get; set; }
    }
}

