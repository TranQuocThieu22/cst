using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static educlient.Models.csCase;

namespace educlient.Services
{
    public interface IThongKeSupService
    {
        Task<XuLyCaseSupDataResult> SupportCaseReport(DateInput date);
        string BuildWiqlQuery();
        string BuildWiqlQueryCaseLamTrongNgay(string Date);
        string BuildWiqlPhanTichTreQuery();
        string BuildWiqlTestTreQuery();
        string BuildWiqlCanTestQuery();
        string BuildWiqlGanTagQuery();
        Task<List<workItemBase>> GetCaseIds(string wiqlQuery);
        Task<TfsCaseDetailModel> GetCaseDetails(List<workItemBase> caseIds);
        List<ThongTinCaseSup> ProcessCaseDetails(TfsCaseDetailModel caseDetails);
        DataTable CreateDataTable();
        void PopulateDataRow(DataRow dr, KeyValuePair<string, string> kvp);
        Task<string> DoTfsQueryData(string pTfsHost, string pbaseUrl, string pQueryAppend, string pPOSTBody, string pBasicToken);
        List<CanXuLyDataDO> GetSoLuongCanPhanTich(List<ThongTinCaseSup> data);
        List<CanXuLyDataDO> GetSoLuongCanTest(List<ThongTinCaseSup> data);
        List<CanXuLyDataDO> GetSoLuongGanTag(List<ThongTinCaseSup> data);
        List<XuLyCaseSupdataDO> SummarizeCanXuLyData(List<ThongTinCaseSup> thongTinCases, List<ThongTinCaseSup> thongTinCasesTest, List<ThongTinCaseSup> thongTinCasesGanTag);
        List<XuLyCaseSupdataDO> CountXuLyTreByCaseTester(List<ThongTinCaseSup> thongTinCases);
        List<XuLyCaseSupdataDO> CountPhanTichTreByCaseAnalyst(List<ThongTinCaseSup> thongTinCases);
        List<XuLyCaseSupdataDO> CalculateXuLyTre(List<ThongTinCaseSup> thongTinCasesPhanTichTre, List<ThongTinCaseSup> thongTinCasesTestTre);
        List<XuLyCaseSupdataDO> GetSoLuongCaseSupLam(List<ThongTinCaseSup> data, string date);
        List<XuLyCaseSupdataDO> SupReport(List<XuLyCaseSupdataDO> combinedData, List<XuLyCaseSupdataDO> SoLuongCanXuLy, List<XuLyCaseSupdataDO> CaseSupLamTrongNgay);
    }

    public class ThongKeSupService : IThongKeSupService
    {
        private readonly IConfiguration config;
        const string TFS_TOKEN_BASE64 = "QVFcdGZzdXNlcjpyY3phdmVsczJ6ZGw2bDZqdDZ6cXRxdGp0YW1wMzQ1NDQyYm9ycXk3cGNyd2doem1icHFx";
        public static string TFS_HOST = Startup.tfsUrl;
        public ThongKeSupService(IConfiguration cf)
        {
            config = cf;
        }
        public async Task<XuLyCaseSupDataResult> SupportCaseReport(DateInput date)
        {

            var wiqlQuery = BuildWiqlQuery();
            var CanTestwiqlQuery = BuildWiqlCanTestQuery();
            var CanGanTagwiqlQuery = BuildWiqlGanTagQuery();
            var TestTreWiqlTestTreQuery = BuildWiqlTestTreQuery();
            var PhanTichTreWiqlTestTreQuery = BuildWiqlPhanTichTreQuery();
            //var TongCaseCuaSupQuery = BuildWiqlQueryCaseLamTrongNgay(date.data);
            var TongCaseCuaSupQuery = BuildWiqlQueryCaseLamTrongNgay2(date.data);


            var caseIds = await GetCaseIds(wiqlQuery);
            var caseTestIds = await GetCaseIds(CanTestwiqlQuery);
            var caseGanTagIds = await GetCaseIds(CanGanTagwiqlQuery);
            var caseTestTreIds = await GetCaseIds(TestTreWiqlTestTreQuery);
            var casePhanTichTreIds = await GetCaseIds(PhanTichTreWiqlTestTreQuery);
            var TongCaseDaLamCuaSup = await GetCaseIds(TongCaseCuaSupQuery);

            if (caseIds == null || !caseIds.Any())

                if (caseIds == null || !caseIds.Any())
                {
                    return new XuLyCaseSupDataResult { data = new List<XuLyCaseSupdataDO>() };
                }

            var caseDetails = await GetCaseDetails(caseIds);
            var caseTestDetails = await GetCaseDetails(caseTestIds);
            var caseGanTagDetails = await GetCaseDetails(caseGanTagIds);
            var caseTestTreDetails = await GetCaseDetails(caseTestTreIds);
            var casePhanTichTreDetails = await GetCaseDetails(casePhanTichTreIds);
            var TongCaseDaLamCuaSupDetails = await GetCaseDetails(TongCaseDaLamCuaSup);

            var thongTinCases = ProcessCaseDetails(caseDetails);
            var thongTinCasesTest = ProcessCaseDetails(caseTestDetails);
            var thongTinCasesGanTag = ProcessCaseDetails(caseGanTagDetails);
            var thongTinCasesTestTre = ProcessCaseDetails(caseTestTreDetails);
            var thongTinCasesPhanTichTre = ProcessCaseDetails(casePhanTichTreDetails);
            var thongTinTongCaseCuaSup = ProcessCaseDetails(TongCaseDaLamCuaSupDetails);



            //var SoLuongCanPhanTich = GetSoLuongCanPhanTich(thongTinCases);
            //var SoLuongCaseCanTest = GetSoLuongCanTest(thongTinCasesTest);
            //var SoLuongCaseGanTag = GetSoLuongGanTag(thongTinCasesGanTag);
            var SoLuongCaseSupXuLyTrongNgay = GetSoLuongCaseSupLam(thongTinTongCaseCuaSup, date.data);
            var SoLuongCanXuLy = SummarizeCanXuLyData(thongTinCases, thongTinCasesTest, thongTinCasesGanTag);
            var SoLuongTestTreCase = CountXuLyTreByCaseTester(thongTinCasesTestTre);
            var SoLuongPhanTichTreCase = CountPhanTichTreByCaseAnalyst(thongTinCasesPhanTichTre);
            var caseXuLyTre = CalculateXuLyTre(thongTinCasesPhanTichTre, thongTinCasesTestTre);

            var supReport = SupReport(caseXuLyTre, SoLuongCanXuLy, SoLuongCaseSupXuLyTrongNgay);

            return new XuLyCaseSupDataResult { code = 200, message = "success", result = true, data = supReport };
        }
        public string BuildWiqlPhanTichTreQuery()
        {
            return $@"{{
    ""query"": ""SELECT [System.Id]
    FROM WorkItems 
    WHERE [System.TeamProject] = 'Edusoft.Net-CS' 
    AND [System.WorkItemType] <> '' 
    AND ([AQ.CaseAnalyst] IN (
        'thanh <AQ\\thanh>', 'havt <AQ\\havt>', 'thuannam <AQ\\thuannam>', 'thuyduong <AQ\\thuyduong>', 'mtien <AQ\\mtien>', 'giaminh <AQ\\duongminh>'
    ) 
    AND [AQ.Priority] <> '5 - Cần theo dõi'
    AND [System.State] = 'Mở Case' 
    AND [System.CreatedDate] <= @today - 2 
    AND [System.AssignedTo] = 'tiepnhansup <AQ\\tiepnhansup>') 
    AND [AQ.ReqState] <> '01 – Đã phân tích xong'
    ORDER BY [System.State]""
}}";
        }
        public string BuildWiqlTestTreQuery()
        {
            return $@"{{
    ""query"": ""SELECT [System.Id]
    FROM WorkItems 
    WHERE [System.TeamProject] = 'Edusoft.Net-CS' 
    AND [System.WorkItemType] <> '' 
    AND ([System.State] = 'Đã xử lý' 
    AND [AQ.CaseTester] IN (
        'thanh <AQ\\thanh>', 'havt <AQ\\havt>', 'thuannam <AQ\\thuannam>', 'thuyduong <AQ\\thuyduong>', 'mtien <AQ\\mtien>', 'giaminh <AQ\\duongminh>'
    ) 
    AND [Microsoft.VSTS.Common.ResolvedDate] <= @today - 3) 
    AND [AQ.TestState] <> '01 - Đã kiểm tra và kết quả đúng' 
    ORDER BY [System.State]""
}}";
        }


        public string BuildWiqlCanTestQuery()
        {
            return $@"{{
                ""query"": ""SELECT [System.Id]
                FROM WorkItems
                WHERE [System.TeamProject] = 'Edusoft.Net-CS'
                AND [System.State] = 'Đã xử lý'
                AND [System.AssignedTo] IN (
                    'thanhtrung <AQ\\thanhtrung>', 'chivtan <AQ\\chivtan>', 'quocthieu <AQ\\quocthieu>','thieu <AQ\\thieu>', 'mtrung <AQ\\mtrung>', 'mtien <AQ\\mtien>','lamto <AQ\\lamto>', 'thuannam <AQ\\thuannam>', 'giaminh <AQ\\duongminh>','thuyduong <AQ\\thuyduong>', 'havt <AQ\\havt>', 'kimtuyen <AQ\\kimtuyen>','nvhanh <AQ\\nvhanh>', 'admin <AQ\\admin>', 'dat <AQ\\dat>','minhlam <AQ\\minhlam>', 'tin <AQ\\tin>'
                )
                AND [AQ.TestState] <> '01 - Đã kiểm tra và kết quả đúng'

                ORDER BY [AQ.CaseTester]""
}}";
        }
        public string BuildWiqlQueryCaseLamTrongNgay(string Date)
        {
            return $@"{{
            ""query"": ""SELECT [System.Id]
            FROM WorkItems
            WHERE [System.TeamProject] = @project
            AND [System.WorkItemType] <> ''
            AND (
                ([AQ.CaseAnalyst] IN ('thanh <AQ\\thanh>', 'havt <AQ\\havt>', 'thuannam <AQ\\thuannam>', 'thuyduong <AQ\\thuyduong>', 'mtien <AQ\\mtien>', 'giaminh <AQ\\duongminh>') AND [AQ.DateKQPhanTich] = '{Date}')
                OR ([System.AssignedTo] IN ('thuannam <AQ\\thuannam>', 'mtien <AQ\\mtien>', 'giaminh <AQ\\duongminh>', 'thanh <AQ\\thanh>', 'thuyduong <AQ\\thuyduong>', 'havt <AQ\\havt>') AND [Microsoft.VSTS.Common.ResolvedDate] = '{Date}')
                OR ([AQ.CaseTester] IN ('thanh <AQ\\thanh>', 'thuannam <AQ\\thuannam>', 'mtien <AQ\\mtien>', 'giaminh <AQ\\duongminh>', 'thuyduong <AQ\\thuyduong>', 'havt <AQ\\havt>') AND [AQ.DateKQTest] = '{Date}')
            )
            ORDER BY [AQ.CaseAnalyst]""
            }}";
        }
        public string BuildWiqlQueryCaseLamTrongNgay2(string Date)
        {
            return $@"{{
            ""query"": ""SELECT [System.Id]
            FROM WorkItems
            WHERE [System.TeamProject] = @project
            AND [System.WorkItemType] <> ''
            AND (
                ([AQ.CaseAnalyst] IN ('thanh <AQ\\thanh>', 'havt <AQ\\havt>', 'thuannam <AQ\\thuannam>', 'thuyduong <AQ\\thuyduong>', 'mtien <AQ\\mtien>', 'giaminh <AQ\\duongminh>') 
                AND [AQ.ReqState] = '01 – Đã phân tích xong' 
                AND [AQ.DateKQPhanTich] = '{Date}')
                OR ([System.AssignedTo] IN ('thuannam <AQ\\thuannam>', 'mtien <AQ\\mtien>', 'giaminh <AQ\\duongminh>', 'thanh <AQ\\thanh>', 'thuyduong <AQ\\thuyduong>', 'havt <AQ\\havt>') 
                AND [Microsoft.VSTS.Common.ResolvedDate] = '{Date}' 
                AND ([System.State] = 'Đã xử lý' OR [System.State] = 'Đóng case'))
                OR ([AQ.CaseTester] IN ('thanh <AQ\\thanh>', 'thuannam <AQ\\thuannam>', 'mtien <AQ\\mtien>', 'giaminh <AQ\\duongminh>', 'thuyduong <AQ\\thuyduong>', 'havt <AQ\\havt>') 
                AND [AQ.DateKQTest] = '{Date}' 
                AND ([AQ.TestState] = '01 - Đã kiểm tra và kết quả đúng' OR [AQ.TestState] = '02 - Đã kiểm tra và cần thực hiện lại Case'))
            )
            ORDER BY [AQ.CaseAnalyst]""
                    }}";
        }
        public string BuildWiqlGanTagQuery()
        {
            return $@"{{
                ""query"": ""SELECT [System.Id]
                FROM WorkItems
                WHERE [System.TeamProject] = 'Edusoft.Net-CS'
                AND [System.WorkItemType] <> ''
                AND [System.State] NOT IN (
                    'Đóng case', 'Active', 'Inactive', 'Completed', 'Đóng',
                    'Chuẩn bị meeting', 'Đã meeting', 'Closed', 'Đã xử lý'
                )
                    AND [System.State] IN (
                                'Đang xử lý', 'Mở Case'
                            )
                    AND [AQ.CaseType] <> 'CV - Trao đổi hoặc chưa phân loại'
                AND [System.AssignedTo] IN (
                        'thanh <AQ\\thanh>',
                        'havt <AQ\\havt>',
                        'giaminh <AQ\\duongminh>',
                        'thuyduong <AQ\\thuyduong>',
                        'thuannam <AQ\\thuannam>',
                        'mtien <AQ\\mtien>'
                    )

                ORDER BY [System.AssignedTo]""}}";
        }


        public string BuildWiqlQuery()
        {

            return $@"{{
    ""query"": ""SELECT [System.Id]
    FROM WorkItems
    WHERE [System.TeamProject] = 'Edusoft.Net-CS'
    AND [System.WorkItemType] <> ''
    
    AND [AQ.CaseAnalyst] IN (
        'thanh <AQ\\thanh>', 'havt <AQ\\havt>', 'thuannam <AQ\\thuannam>',
        'thuyduong <AQ\\thuyduong>', 'mtien <AQ\\mtien>', 'giaminh <AQ\\duongminh>'
    )
    AND [System.State] = 'Mở Case'
    AND ([AQ.ReqState] = '' OR [AQ.ReqState] = '02 – Chưa phân tích xong')        
    AND 
        (
        ([System.AssignedTo] = 'tiepnhansup <AQ\\tiepnhansup>'
        AND ([AQ.ReqState] = '' OR [AQ.ReqState] = '02 – Chưa phân tích xong')        
        

        )
    OR 
        (
            (
                [System.State] = 'Đã xử lý'
                AND [AQ.CaseTester] IN 
                (
                    'thanh <AQ\\thanh>', 'havt <AQ\\havt>', 'mtien <AQ\\mtien>',
                    'thuyduong <AQ\\thuyduong>', 'thuannam <AQ\\thuannam>', 'giaminh <AQ\\duongminh>'
                )
                AND [AQ.TestState] <> '01 - Đã kiểm tra và kết quả đúng'
            )
            OR 
                (
                    [System.AssignedTo] IN 
                    (
                        'thanh <AQ\\thanh>', 'havt <AQ\\havt>', 'mtien <AQ\\mtien>',
                        'thuyduong <AQ\\thuyduong>', 'thuannam <AQ\\thuannam>', 'giaminh <AQ\\duongminh>'
                    )
                        AND ([System.State] = 'Mở Case' OR [System.State] = 'Đang xử lý'))
                )
        )
    ORDER BY [System.State]""
}}";


        }

        public async Task<List<workItemBase>> GetCaseIds(string wiqlQuery)
        {
            var jsonCasesId = await DoTfsQueryData(TFS_HOST
                                            , "tfs/aq/" + "Edusoft.Net-CS" + "/" + "Edusoft.Net-CS%20Team" + "/_apis/wit/wiql?api-version=4.1"
                                            , "", wiqlQuery, TFS_TOKEN_BASE64);

            if (string.IsNullOrEmpty(jsonCasesId))
                return null;

            var lstCasesId = JsonConvert.DeserializeObject<TfsCaseListModel>(jsonCasesId);
            return lstCasesId.workItems;
        }

        public async Task<TfsCaseDetailModel> GetCaseDetails(List<workItemBase> caseIds)
        {

            string sTfsFieldList = "System.Title,System.Id,System.CreatedDate,AQ.ReqState,AQ.CaseAnalyst,AQ.Customer,System.State,AQ.CaseTester,System.AssignedTo,Microsoft.VSTS.Common.ResolvedDate,AQ.Priority,AQ.CaseType,AQ.Module,AQ.Comment,AQ.ContractType,AQ.PriorityType,System.AssignedTo,AQ.DateKQTest,AQ.DateKQPhanTich";
            string sCaseList = string.Join(",", caseIds.Select(s => s.id));

            var tmpjson = await DoTfsQueryData(TFS_HOST,
                $"tfs/aq/Edusoft.Net-CS/_apis/wit/workitems?ids={sCaseList}&fields={sTfsFieldList}",
                "", "", TFS_TOKEN_BASE64);

            return JsonConvert.DeserializeObject<TfsCaseDetailModel>(tmpjson);
        }

        public List<ThongTinCaseSup> ProcessCaseDetails(TfsCaseDetailModel caseDetails)
        {
            if (caseDetails == null || caseDetails.count == 0)
                return new List<ThongTinCaseSup>();

            var dt = CreateDataTable();

            foreach (var r in caseDetails.value)
            {
                var dr = dt.NewRow();
                foreach (KeyValuePair<string, string> kvp in r.fields)
                {
                    PopulateDataRow(dr, kvp);
                }
                dt.Rows.Add(dr);
            }

            var jsonString = JsonConvert.SerializeObject(dt);
            return JsonConvert.DeserializeObject<List<ThongTinCaseSup>>(jsonString);
        }

        public DataTable CreateDataTable()
        {
            DataTable dt = new DataTable("tblTfsData");
            dt.Columns.Add("macase", typeof(int));
            dt.Columns.Add("matruong", typeof(string));
            dt.Columns.Add("ngaynhan", typeof(DateTime));
            dt.Columns.Add("trangthai", typeof(string));
            dt.Columns.Add("ngaydukien", typeof(DateTime));
            dt.Columns.Add("loaihopdong", typeof(string));
            dt.Columns.Add("mucdo", typeof(string));
            dt.Columns.Add("hieuluc", typeof(string));
            dt.Columns.Add("ngayemail", typeof(DateTime));
            dt.Columns.Add("mailto", typeof(string));
            dt.Columns.Add("loaicase", typeof(string));
            dt.Columns.Add("phanhe", typeof(string));
            dt.Columns.Add("comment", typeof(string));
            dt.Columns.Add("assignedto", typeof(string));
            dt.Columns.Add("caseanalyst", typeof(string));
            dt.Columns.Add("casetester", typeof(string));
            dt.Columns.Add("tinhnangmoi", typeof(bool));
            dt.Columns.Add("datekqphantich", typeof(string));
            dt.Columns.Add("datekqtest", typeof(string));
            dt.Columns.Add("resolveddate", typeof(string));
            return dt;
        }

        public void PopulateDataRow(DataRow dr, KeyValuePair<string, string> kvp)
        {
            switch (kvp.Key.ToLower())
            {
                case "system.id":
                    dr["macase"] = kvp.Value;
                    break;
                case "aq.caseanalyst":
                    dr["caseanalyst"] = kvp.Value;
                    break;
                case "aq.casetester":
                    dr["casetester"] = kvp.Value;
                    break;
                case "system.assignedto":
                    dr["assignedto"] = kvp.Value;
                    break;
                case "system.state":
                    dr["trangthai"] = kvp.Value;
                    break;
                case "aq.customer":
                    dr["matruong"] = kvp.Value;
                    break;
                case "system.createddate":
                    dr["ngaynhan"] = kvp.Value;
                    break;
                case "aq.targetdate":
                    dr["ngaydukien"] = kvp.Value;
                    break;
                case "aq.contracttype":
                    dr["loaihopdong"] = kvp.Value;
                    break;
                case "aq.prioritytype":
                    dr["mucdo"] = kvp.Value;
                    break;
                case "aq.releasedate":
                    dr["hieuluc"] = kvp.Value;
                    break;
                case "microsoft.vsts.common.resolveddate":
                    dr["resolveddate"] = kvp.Value;
                    break;
                case "aq.mailto":
                    dr["mailto"] = kvp.Value;
                    break;
                case "aq.casetype":
                    dr["loaicase"] = kvp.Value;
                    break;
                case "aq.module":
                    dr["phanhe"] = kvp.Value;
                    break;
                case "aq.comment":
                    dr["comment"] = (string.IsNullOrEmpty(kvp.Value) || kvp.Value == "0" || kvp.Value == "1") ? "" : kvp.Value;
                    break;
                case "aq.datekqphantich":
                    dr["datekqphantich"] = kvp.Value;
                    break;
                case "aq.datekqtest":
                    dr["datekqtest"] = kvp.Value;
                    break;
            }
        }

        // You'll need to implement this method to make API calls to TFS
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
        public List<CanXuLyDataDO> GetSoLuongCanPhanTich(List<ThongTinCaseSup> data)
        {


            List<CanXuLyDataDO> xuLyCaseSupdataDO = data
        .GroupBy(c => c.caseanalyst)
        .Select(g => new CanXuLyDataDO
        {
            assignedto = g.Key,
            caseList = g.Select(c => c.macase).ToList(),
            CanPhanTich = g.Count()
        })
        .ToList();
            return xuLyCaseSupdataDO;
        }
        public List<CanXuLyDataDO> GetSoLuongCanTest(List<ThongTinCaseSup> data)
        {
            List<CanXuLyDataDO> xuLyCaseSupdataDO = data
        .GroupBy(c => c.casetester)
        .Select(g => new CanXuLyDataDO
        {
            assignedto = g.Key,
            caseList = g.Select(c => c.macase).ToList(),
            CanTest = g.Count()
        })
        .ToList();
            return xuLyCaseSupdataDO;
        }
        //    public List<XuLyCaseSupdataDO> GetSoLuongCaseSupLam(List<ThongTinCaseSup> data,string date)
        //    {
        //        var allowedPeople = new HashSet<string>
        //{
        //    "thanh <AQ\\thanh>",
        //    "havt <AQ\\havt>",
        //    "thuannam <AQ\\thuannam>",
        //    "thuyduong <AQ\\thuyduong>",
        //    "mtien <AQ\\mtien>",
        //    "giaminh <AQ\\duongminh>"
        //};

        //        var result = data
        //            .SelectMany(c => new[]
        //            {
        //        new { Person = c.caseanalyst, Case = c.macase },
        //        new { Person = c.casetester, Case = c.macase },
        //        new { Person = c.assignedto, Case = c.macase }
        //            })
        //            .Where(x => !string.IsNullOrEmpty(x.Person) && allowedPeople.Contains(x.Person))
        //            .GroupBy(x => x.Person)
        //            .Select(g => new XuLyCaseSupdataDO
        //            {
        //                assignedto = g.Key,
        //                //caseList = g.Select(x => x.Case).Distinct().ToList(),
        //                CaseLamTrongNgay = g.Select(x => x.Case).Distinct().Count(),
        //                CaseList = g.Select(x => x.Case).Distinct().ToList()
        //            })
        //            .ToList();

        //        return result;
        //    }
        public List<XuLyCaseSupdataDO> GetSoLuongCaseSupLam(List<ThongTinCaseSup> data, string date)
        {
            var allowedPeople = new HashSet<string>
    {
        "thanh <AQ\\thanh>",
        "havt <AQ\\havt>",
        "thuannam <AQ\\thuannam>",
        "thuyduong <AQ\\thuyduong>",
        "mtien <AQ\\mtien>",
        "giaminh <AQ\\duongminh>"
    };

            DateTime inputDate;
            if (!DateTime.TryParse(date, out inputDate))
            {
                throw new ArgumentException("Invalid date format", nameof(date));
            }

            var result = data
                .SelectMany(c => new[]
                {
            new { Person = c.caseanalyst, Case = c.macase, Date = DateTime.TryParse(c.datekqphantich, out var d1) ? d1 : (DateTime?)null },
            new { Person = c.casetester, Case = c.macase, Date = DateTime.TryParse(c.datekqtest, out var d2) ? d2 : (DateTime?)null },
            new { Person = c.assignedto, Case = c.macase, Date = DateTime.TryParse(c.resolveddate, out var d3) ? d3 : (DateTime?)null }
                })
                .Where(x => !string.IsNullOrEmpty(x.Person) &&
                            allowedPeople.Contains(x.Person) &&
                            x.Date.HasValue &&
                            x.Date.Value.Date == inputDate.Date)
                .GroupBy(x => x.Person)
                .Select(g => new XuLyCaseSupdataDO
                {
                    assignedto = g.Key,
                    CaseLamTrongNgay = g.Select(x => x.Case).Count(),
                    CaseList = g.Select(x => x.Case).Distinct().ToList()
                })
                .ToList();

            return result;
        }
        public List<CanXuLyDataDO> GetSoLuongGanTag(List<ThongTinCaseSup> data)
        {
            List<CanXuLyDataDO> xuLyCaseSupdataDO = data
        .GroupBy(c => c.assignedto)
        .Select(g => new CanXuLyDataDO
        {
            assignedto = g.Key,
            caseList = g.Select(c => c.macase).ToList(),
            DuocGan = g.Count()
        })
        .ToList();
            return xuLyCaseSupdataDO;
        }

        public List<XuLyCaseSupdataDO> SummarizeCanXuLyData(List<ThongTinCaseSup> thongTinCases, List<ThongTinCaseSup> thongTinCasesTest, List<ThongTinCaseSup> thongTinCasesGanTag)
        {
            var canPhanTich = GetSoLuongCanPhanTich(thongTinCases);
            var canTest = GetSoLuongCanTest(thongTinCasesTest);
            var duocGan = GetSoLuongGanTag(thongTinCasesGanTag);
            foreach (var item in canPhanTich)
            {
                if (item.assignedto == "thanh <AQ\\thanh>")
                {

                    foreach (var item1 in item.caseList)

                    {
                        Debug.WriteLine($"canPhanTich: {item.assignedto}: caseList: {item1} ");
                    }
                }
            }
            foreach (var item in canTest)
            {
                if (item.assignedto == "thanh <AQ\\thanh>")
                {

                    foreach (var item1 in item.caseList)

                    {
                        Debug.WriteLine($"canTest: {item.assignedto}: caseList: {item1} ");
                    }
                }
            }
            foreach (var item in duocGan)
            {
                if (item.assignedto == "thanh <AQ\\thanh>")
                {

                    foreach (var item1 in item.caseList)

                    {
                        Debug.WriteLine($"duocGan: {item.assignedto}: caseList: {item1} ");
                    }
                }
            }
            var allowedAssignedTo = new List<string>
                {
                    "thanh <AQ\\thanh>",
                    "havt <AQ\\havt>",
                    "giaminh <AQ\\duongminh>",
                    "thuyduong <AQ\\thuyduong>",
                    "thuannam <AQ\\thuannam>",
                    "mtien <AQ\\mtien>"
                };

            var combinedData = canPhanTich
                .Concat(canTest)
                .Concat(duocGan)
                .Where(x => allowedAssignedTo.Contains(x.assignedto))
                .GroupBy(x => x.assignedto)
                .Select(g => new XuLyCaseSupdataDO
                {
                    assignedto = g.Key,
                    canXuLy = g.SelectMany(x => x.caseList).Distinct().Count(),
                    XuLyTre = 0, // Set default values for these fields
                    PhanTichTre = 0,
                    TestTre = 0
                })
                .ToList();

            return combinedData;
        }
        //public List<CaseSupLamTrongNgay> CountTongSoCaseSupLamTrongNgay(List<ThongTinCaseSup> thongTinCases)
        //{

        //}
        public List<XuLyCaseSupdataDO> CountXuLyTreByCaseTester(List<ThongTinCaseSup> thongTinCases)
        {
            // Group the cases by casetester
            var groupedByCaseTester = thongTinCases
                .Where(tc => !string.IsNullOrEmpty(tc.casetester)) // Ensure casetester is not null or empty
                .GroupBy(tc => tc.casetester)
                .Select(g => new
                {
                    CaseTester = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // Map the counts to XuLyCaseSupdataDO
            var xuLyCaseSupdata = groupedByCaseTester.Select(g => new XuLyCaseSupdataDO
            {
                assignedto = g.CaseTester,
                canXuLy = 0, // Default value
                XuLyTre = 0,
                PhanTichTre = 0, // Default value
                TestTre = g.Count // Default value
            }).ToList();

            return xuLyCaseSupdata;
        }
        public List<XuLyCaseSupdataDO> CountPhanTichTreByCaseAnalyst(List<ThongTinCaseSup> thongTinCases)
        {
            // Group the cases by casetester
            var groupedByCaseTester = thongTinCases
                .Where(tc => !string.IsNullOrEmpty(tc.caseanalyst)) // Ensure casetester is not null or empty
                .GroupBy(tc => tc.caseanalyst)
                .Select(g => new
                {
                    CaseAnalyst = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // Map the counts to XuLyCaseSupdataDO
            var xuLyCaseSupdata = groupedByCaseTester.Select(g => new XuLyCaseSupdataDO
            {
                assignedto = g.CaseAnalyst,
                canXuLy = 0, // Default value
                XuLyTre = 0,
                PhanTichTre = g.Count, // Default value
                TestTre = 0 // Default value
            }).ToList();

            return xuLyCaseSupdata;
        }
        public List<XuLyCaseSupdataDO> CalculateXuLyTre(List<ThongTinCaseSup> thongTinCasesPhanTichTre, List<ThongTinCaseSup> thongTinCasesTestTre)
        {
            var phanTichTreData = CountPhanTichTreByCaseAnalyst(thongTinCasesPhanTichTre);
            var testTreData = CountXuLyTreByCaseTester(thongTinCasesTestTre);

            // Combine the results
            var combinedData = phanTichTreData
                .Concat(testTreData)
                .GroupBy(x => x.assignedto)
                .Select(g => new XuLyCaseSupdataDO
                {
                    assignedto = g.Key,
                    canXuLy = 0, // Default value
                    XuLyTre = g.Sum(x => x.PhanTichTre + x.TestTre),
                    PhanTichTre = g.Sum(x => x.PhanTichTre),
                    TestTre = g.Sum(x => x.TestTre)
                })
                .ToList();

            return combinedData;
        }
        public List<XuLyCaseSupdataDO> SupReport(List<XuLyCaseSupdataDO> combinedData, List<XuLyCaseSupdataDO> SoLuongCanXuLy, List<XuLyCaseSupdataDO> CaseSupLamTrongNgay)
        {
            var finalData = combinedData
                .Concat(SoLuongCanXuLy)
                .Concat(CaseSupLamTrongNgay)
                .GroupBy(x => x.assignedto)
                .Select(g => new XuLyCaseSupdataDO
                {
                    assignedto = g.Key,
                    canXuLy = g.Sum(x => x.canXuLy), // Default value
                    XuLyTre = g.Sum(x => x.PhanTichTre + x.TestTre),
                    PhanTichTre = g.Sum(x => x.PhanTichTre),
                    TestTre = g.Sum(x => x.TestTre),
                    CaseLamTrongNgay = g.Sum(x => x.CaseLamTrongNgay)
                })
                .ToList();
            return finalData;
        }
    }
}
