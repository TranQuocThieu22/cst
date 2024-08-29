using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static educlient.Models.csCase;

namespace educlient.Services
{
    public interface IThongKeDevService
    {
        Task<XuLyCaseDataResult> CoderCaseReport(DateInput Date);
        string BuildWiqlQuery();
        string BuildWiqlCaseInDayQuery(string Date);
        Task<List<workItemBase>> GetCaseIds(string wiqlQuery);
        Task<TfsCaseDetailModel> GetCaseDetails(List<workItemBase> caseIds);
        List<ThongTinCase> ProcessCaseDetails(TfsCaseDetailModel caseDetails);
        Task<string> DoTfsQueryData(string pTfsHost, string pbaseUrl, string pQueryAppend, string pPOSTBody, string pBasicToken);
        Task<XuLyCasedataDO> dataXuLyCase();
        Task<List<dataTreBaoTri>> tinhSoNgayTreBaoTri(List<ThongTinCase> data);
        List<XuLyCasedataDO> TinhSoLuongCaseTodayTheoUser(List<XuLyCasedataDO> data, List<ThongTinCase> thongTinCaseToday, string date);
        List<XuLyCasedataDO> tinhdataXuLyCase(List<dataTreBaoTri> data);
        Task<CaseTheoThoiGianChoResult> PhanBoSoCaseTheoThoiGianCoder();
    }
    public class ThongKeDevService : IThongKeDevService
    {
        private readonly IConfiguration config;
        const string TFS_TOKEN_BASE64 = "QVFcdGZzdXNlcjpyY3phdmVsczJ6ZGw2bDZqdDZ6cXRxdGp0YW1wMzQ1NDQyYm9ycXk3cGNyd2doem1icHFx";
        public static string TFS_HOST = Startup.tfsUrl;
        public ThongKeDevService(IConfiguration cf)
        {
            config = cf;
        }

        public async Task<XuLyCaseDataResult> CoderCaseReport(DateInput Date)
        {
            var wiqlQuery = BuildWiqlQuery();
            var wiqlCaseTodayQuery = BuildWiqlCaseInDayQuery(Date.data);

            var caseIds = await GetCaseIds(wiqlQuery);
            var caseIdsByTodayCase = await GetCaseIds(wiqlCaseTodayQuery);
            if (caseIds == null || !caseIds.Any())
            {
                return new XuLyCaseDataResult { count = 0, data = new List<XuLyCasedataDO>() };
            }
            var caseDetails = await GetCaseDetails(caseIds);
            var casesTodayDetails = await GetCaseDetails(caseIdsByTodayCase);

            var thongTinCases = ProcessCaseDetails(caseDetails);
            var thongTinCasesToday = ProcessCaseDetails(casesTodayDetails);

            var test = await PhanBoCaseTheoThoiGianCho(thongTinCases);
            var soNgayBaoTri = await tinhSoNgayTreBaoTri(thongTinCases);
            var dataCaseTreVaCanXuLy = tinhdataXuLyCase(soNgayBaoTri);
            var dataTienDoCongViec = TinhSoLuongCaseTodayTheoUser(dataCaseTreVaCanXuLy, thongTinCasesToday, Date.data);
            Console.Write(dataTienDoCongViec);

            return new XuLyCaseDataResult { count = dataTienDoCongViec.Count, code = 200, message = "success", result = true, data = dataTienDoCongViec, };
        }
        public async Task<CaseTheoThoiGianChoResult> PhanBoSoCaseTheoThoiGianCoder()
        {
            var wiqlQuery = BuildWiqlQuery();
            var caseIds = await GetCaseIds(wiqlQuery);
            if (caseIds == null || !caseIds.Any())
            {
                return new CaseTheoThoiGianChoResult { count = 0, data = new List<CaseTheoThoiGianChoDataDO>() };
            }
            var caseDetails = await GetCaseDetails(caseIds);
            var thongTinCases = ProcessCaseDetails(caseDetails);
            var data = await PhanBoCaseTheoThoiGianCho(thongTinCases);
            var reversedData = data.AsEnumerable().Reverse().ToList();

            return new CaseTheoThoiGianChoResult { count = GetTotalSoCasePhanBoSoCaseTheoThoiGianCho(data), code = 200, message = "success", result = true, data = reversedData, };
        }
        public string BuildWiqlQuery()
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
            AND [System.AssignedTo] NOT IN (
                    'tiepnhansup <AQ\\tiepnhansup>',
                    'thanh <AQ\\thanh>',
                    'havt <AQ\\havt>',
                    'giaminh <AQ\\duongminh>',
                    'thuyduong <AQ\\thuyduong>',
                    'thuannam <AQ\\thuannam>',
                    'mtien <AQ\\mtien>',
                    'tiepnhansale <AQ\\tiepnhansale>',
                    'tiepnhandev <AQ\\tiepnhandev>',
                    'dien <AQ\\dien>',
                    'admin <AQ\\admin>',
                    'root <AQ\\root>'
                )
            ORDER BY [System.AssignedTo]""}}";
        }
        public string BuildWiqlCaseInDayQuery(string Date)
        {
            return $@"{{
                ""query"": ""SELECT [System.Id]
                FROM WorkItems
                WHERE [System.TeamProject] = 'Edusoft.Net-CS'
                AND [System.WorkItemType] <> ''
                AND (
                    [AQ.DetailDate1] = '{Date}'
                    OR [AQ.DetailDate2] =  '{Date}'
                    OR [AQ.DetailDate3] =  '{Date}'
                    OR [AQ.DetailDate4] =  '{Date}'
                    OR [AQ.DetailDate5] =  '{Date}'
                    OR [AQ.DetailDate6] =  '{Date}'
                    OR [AQ.DetailDate7] =  '{Date}'
                    OR [AQ.DetailDate8] =  '{Date}'
                    OR [AQ.DetailDate9] =  '{Date}'
                    OR [AQ.DetailDate10] =  '{Date}'
                )
                 AND [System.AssignedTo] NOT IN (
                    'tiepnhansup <AQ\\tiepnhansup>',
                    'thanh <AQ\\thanh>',
                    'havt <AQ\\havt>',
                    'giaminh <AQ\\duongminh>',
                    'thuyduong <AQ\\thuyduong>',
                    'thuannam <AQ\\thuannam>',
                    'mtien <AQ\\mtien>',
                    'tiepnhansale <AQ\\tiepnhansale>',
                    'tiepnhandev <AQ\\tiepnhandev>',
                    'dien <AQ\\dien>',
                    'admin <AQ\\admin>',
                    'root <AQ\\root>'
                )
                ORDER BY [System.AssignedTo]""
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
            string sTfsFieldList = "System.AssignedTo,System.Id,AQ.Customer,System.CreatedDate,System.Title,System.State,AQ.TargetDate,AQ.ReleaseDate,Microsoft.VSTS.Common.StateChangeDate,AQ.MailTo,AQ.Priority,AQ.CaseType,AQ.Module,AQ.Comment,AQ.ContractType,AQ.PriorityType,AQ.UserGuideRequested,AQ.ReviewCase,System.State,AQ.EstimateTime,AQ.DetailDate1,AQ.DetailDate2,AQ.DetailDate3,AQ.DetailDate4,AQ.DetailDate5,AQ.DetailDate6,AQ.DetailDate7,AQ.DetailDate8,AQ.DetailDate9,AQ.DetailDate10,AQ.DetailActualTime1,AQ.DetailActualTime2,AQ.DetailActualTime3,AQ.DetailActualTime4,AQ.DetailActualTime5,AQ.DetailActualTime6,AQ.DetailActualTime7,AQ.DetailActualTime8,AQ.DetailActualTime9,AQ.DetailActualTime10";
            string sCaseList = string.Join(",", caseIds.Select(s => s.id));

            var tmpjson = await DoTfsQueryData(TFS_HOST,
                $"tfs/aq/Edusoft.Net-CS/_apis/wit/workitems?ids={sCaseList}&fields={sTfsFieldList}",
                "", "", TFS_TOKEN_BASE64);

            return JsonConvert.DeserializeObject<TfsCaseDetailModel>(tmpjson);
        }


        public List<ThongTinCase> ProcessCaseDetails(TfsCaseDetailModel caseDetails)
        {
            if (caseDetails == null || caseDetails.count == 0)
                return new List<ThongTinCase>();

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
            return JsonConvert.DeserializeObject<List<ThongTinCase>>(jsonString);
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
            dt.Columns.Add("tinhnangmoi", typeof(bool));
            dt.Columns.Add("estimatetime", typeof(float));
            dt.Columns.Add("detaildate1", typeof(DateTime));
            dt.Columns.Add("detaildate2", typeof(DateTime));
            dt.Columns.Add("detaildate3", typeof(DateTime));
            dt.Columns.Add("detaildate4", typeof(DateTime));
            dt.Columns.Add("detaildate5", typeof(DateTime));
            dt.Columns.Add("detaildate6", typeof(DateTime));
            dt.Columns.Add("detaildate7", typeof(DateTime));
            dt.Columns.Add("detaildate8", typeof(DateTime));
            dt.Columns.Add("detaildate9", typeof(DateTime));
            dt.Columns.Add("detaildate10", typeof(DateTime));
            dt.Columns.Add("detailactualtime1", typeof(int));
            dt.Columns.Add("detailactualtime2", typeof(int));
            dt.Columns.Add("detailactualtime3", typeof(int));
            dt.Columns.Add("detailactualtime4", typeof(int));
            dt.Columns.Add("detailactualtime5", typeof(int));
            dt.Columns.Add("detailactualtime6", typeof(int));
            dt.Columns.Add("detailactualtime7", typeof(int));
            dt.Columns.Add("detailactualtime8", typeof(int));
            dt.Columns.Add("detailactualtime9", typeof(int));
            dt.Columns.Add("detailactualtime10", typeof(int));
            return dt;
        }

        public void PopulateDataRow(DataRow dr, KeyValuePair<string, string> kvp)
        {
            switch (kvp.Key.ToLower())
            {
                case "system.id":
                    dr["macase"] = kvp.Value;
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
                case "aq.estimatetime":
                    dr["estimatetime"] = kvp.Value;
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
                case "microsoft.vsts.common.statechangedate":
                    dr["ngayemail"] = kvp.Value;
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
                case "aq.detaildate1":
                    dr["detaildate1"] = kvp.Value;
                    break;
                case "aq.detaildate2":
                    dr["detaildate2"] = kvp.Value;
                    break;
                case "aq.detaildate3":
                    dr["detaildate3"] = kvp.Value;
                    break;
                case "aq.detaildate4":
                    dr["detaildate4"] = kvp.Value;
                    break;
                case "aq.detaildate5":
                    dr["detaildate5"] = kvp.Value;
                    break;
                case "aq.detaildate6":
                    dr["detaildate6"] = kvp.Value;
                    break;
                case "aq.detaildate7":
                    dr["detaildate7"] = kvp.Value;
                    break;
                case "aq.detaildate8":
                    dr["detaildate8"] = kvp.Value;
                    break;
                case "aq.detaildate9":
                    dr["detaildate9"] = kvp.Value;
                    break;
                case "aq.detaildate10":
                    dr["detaildate10"] = kvp.Value;
                    break;
                case "aq.detailactualtime1":
                    dr["detailactualtime1"] = float.Parse(kvp.Value);
                    break;
                case "aq.detailactualtime2":
                    dr["detailactualtime2"] = float.Parse(kvp.Value);
                    break;
                case "aq.detailactualtime3":
                    dr["detailactualtime3"] = float.Parse(kvp.Value);
                    break;
                case "aq.detailactualtime4":
                    dr["detailactualtime4"] = float.Parse(kvp.Value);
                    break;
                case "aq.detailactualtime5":
                    dr["detailactualtime5"] = float.Parse(kvp.Value);
                    break;
                case "aq.detailactualtime6":
                    dr["detailactualtime6"] = float.Parse(kvp.Value);
                    break;
                case "aq.detailactualtime7":
                    dr["detailactualtime7"] = float.Parse(kvp.Value);
                    break;
                case "aq.detailactualtime8":
                    dr["detailactualtime8"] = float.Parse(kvp.Value);
                    break;
                case "aq.detailactualtime9":
                    dr["detailactualtime9"] = float.Parse(kvp.Value);
                    break;
                case "aq.detailactualtime10":
                    dr["detailactualtime10"] = float.Parse(kvp.Value);
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
        public async Task<XuLyCasedataDO> dataXuLyCase()
        {
            var wiqlQuery = BuildWiqlQuery();
            var caseIds = await GetCaseIds(wiqlQuery);

            if (caseIds == null || !caseIds.Any())
            {
                return new XuLyCasedataDO { };
            }

            var caseDetails = await GetCaseDetails(caseIds);
            var thongTinCases = ProcessCaseDetails(caseDetails);
            //Console.log()
            return new XuLyCasedataDO { };
        }
        public async Task<List<dataTreBaoTri>> tinhSoNgayTreBaoTri(List<ThongTinCase> data)
        {
            DateTime now = DateTime.Now;
            List<dataTreBaoTri> listData = new List<dataTreBaoTri>();
            List<string> dsTreBaoTri = new List<string>();
            foreach (var item in data)
            {
                dataTreBaoTri dataTreBaoTri = new dataTreBaoTri { };
                dataTreBaoTri.estimatetime = item.estimatetime.HasValue ? item.estimatetime.Value : 0;
                if (!IsLoaiHopDong3Hoac4(item.loaihopdong))
                {
                    if (item.loaihopdong != "03 - HĐ bảo trì")
                    {
                        dataTreBaoTri.macase = item.macase;
                        dataTreBaoTri.assignedto = item.assignedto;
                        dataTreBaoTri.SoNgayTre = 0;
                    }
                    else if (item.loaihopdong != "01 - HĐ Triển khai")
                    {
                        dataTreBaoTri.macase = item.macase;
                        dataTreBaoTri.assignedto = item.assignedto;
                        dataTreBaoTri.SoNgayTreTrienKhai = 0;
                    }
                }
                else
                {
                    TimeSpan dateDifference = now - DateTime.Parse(item.ngaydukien);
                    dataTreBaoTri.macase = item.macase;
                    dataTreBaoTri.assignedto = item.assignedto;
                    if (dateDifference.Days < 0)
                    {
                        dataTreBaoTri.SoNgayTre = 0;
                    }
                    else
                    {
                        dataTreBaoTri.SoNgayTre = dateDifference.Days;

                    }
                }
                listData.Add(dataTreBaoTri);
            }

            return listData;
        }

        public async Task<List<CaseTheoThoiGianChoDataDO>> PhanBoCaseTheoThoiGianCho(List<ThongTinCase> data)
        {
            var today = DateTime.Today;
            var result = new List<CaseTheoThoiGianChoDataDO>();

            var groupedCases = data
                .Where(c =>
                {
                    DateTime parsedDate;
                    return DateTime.TryParse(c.ngaynhan, out parsedDate); // Ensure date is valid
                })
                .GroupBy(c =>
                {
                    DateTime parsedDate = DateTime.Parse(c.ngaynhan);
                    var daysDifference = (today - parsedDate).Days;
                    if (daysDifference < 7)
                        return "Ít hơn 1 tuần";
                    else if (daysDifference <= 14)
                        return "Tuần 1";
                    else if (daysDifference <= 21)
                        return "Tuần 2";
                    else if (daysDifference <= 28)
                        return "Tuần 3";
                    else if (daysDifference <= 35)
                        return "Tuần 4";
                    else if (daysDifference <= 42)
                        return "Tuần 5";
                    else if (daysDifference <= 49)
                        return "Tuần 6";
                    else
                        return "Nhiều hơn 6 tuần";
                })
                .Select(g => new CaseTheoThoiGianChoDataDO
                {
                    macase = g.Select(c => c.macase).ToList(),
                    SoCase = g.Count(),
                    tuanSo = g.Key
                })
                .ToList();
            result = groupedCases;
            return await Task.FromResult(result); // Simulating async operation
        }

        public List<XuLyCasedataDO> TinhSoLuongCaseTodayTheoUser(List<XuLyCasedataDO> data, List<ThongTinCase> thongTinCaseToday, string date)
        {
            var groupedData = data.GroupBy(x => x.assignedto)
                                  .ToDictionary(g => g.Key, g => g.First());

            var groupedThongTinCase = thongTinCaseToday.GroupBy(x => x.assignedto)
                                            .ToDictionary(g => g.Key, g => new
                                            {
                                                Count = g.Count(),
                                                TodayTimeCounter = g.Sum(item => CalculateTodayTimeCounter(item, date))
                                            });

            foreach (var kvp in groupedData)
            {
                string assignedTo = kvp.Key;
                XuLyCasedataDO item = kvp.Value;

                if (groupedThongTinCase.TryGetValue(assignedTo, out var caseInfo))
                {
                    item.SoCaseTrongNgay = caseInfo.Count;
                    item.LuongGioTrongNgay = caseInfo.TodayTimeCounter;


                }
                else
                {
                    item.SoCaseTrongNgay = 0;
                    item.LuongGioTrongNgay = 0;

                }
            }

            return groupedData.Values.ToList();
        }


        public List<XuLyCasedataDO> tinhdataXuLyCase(List<dataTreBaoTri> data)
        {
            var result = new List<XuLyCasedataDO>();

            int xuLyTre = 0;
            int treBaoTri = 0;
            int soLuongCaseToday = 0;
            int TgCanXyLy = 0;
            int LuongGioTrongNgay = 0;

            // Group the data by assignedto
            var groupedData = data.GroupBy(x => x.assignedto);

            foreach (var group in groupedData)
            {

                xuLyTre = group.Count(x => x.SoNgayTre > 0);
                treBaoTri = group.Count(x => x.SoNgayTreTrienKhai > 0);
                float tgCanXyLy = group.Sum(x => x.estimatetime);

                var xuLyCase = new XuLyCasedataDO
                {
                    assignedto = group.Key,
                    canXuLy = group.Count(x => x.SoNgayTre >= 0),  // Count cases where SoNgayTre > 0
                    XuLyTre = xuLyTre + treBaoTri,// For now, XuLyTre is the same as canXuLy
                    TgCanXyLy = tgCanXyLy,
                };

                result.Add(xuLyCase);
            }

            return result;
        }
        private bool IsLoaiHopDong3Hoac4(string data)
        {
            return data != "03 - HĐ bảo trì" || data != "01 - HĐ Triển khai";
        }
        private float CalculateTodayTimeCounter(ThongTinCase thongTinCase, string Date)
        {
            DateTime today = DateTime.Parse(Date);


            float todayTimeCounter = 0;

            var detailDates = new DateTime?[]
            {
        thongTinCase.detaildate1,
        thongTinCase.detaildate2,
        thongTinCase.detaildate3,
        thongTinCase.detaildate4,
        thongTinCase.detaildate5,
        thongTinCase.detaildate6,
        thongTinCase.detaildate7,
        thongTinCase.detaildate8,
        thongTinCase.detaildate9,
        thongTinCase.detaildate10
            };

            var detailActualTimes = new float?[]
            {
        thongTinCase.detailactualtime1,
        thongTinCase.detailactualtime2,
        thongTinCase.detailactualtime3,
        thongTinCase.detailactualtime4,
        thongTinCase.detailactualtime5,
        thongTinCase.detailactualtime6,
        thongTinCase.detailactualtime7,
        thongTinCase.detailactualtime8,
        thongTinCase.detailactualtime9,
        thongTinCase.detailactualtime10
            };

            for (int i = 0; i < detailDates.Length; i++)
            {
                if (detailDates[i].HasValue && detailDates[i].Value.Date == today)
                {
                    if (detailActualTimes[i].HasValue)
                    {
                        todayTimeCounter += detailActualTimes[i].Value;
                    }
                }
            }

            return todayTimeCounter;
        }
        public int GetTotalSoCasePhanBoSoCaseTheoThoiGianCho(List<CaseTheoThoiGianChoDataDO> result)
        {
            if (result == null)
            {
                // Return 0 if result or result.data is null
                return 0;
            }
            // Sum the SoCase values from the data list
            return result.Sum(d => d.SoCase);
        }
    }
}
