using educlient.Data;
using educlient.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static educlient.Models.csCase;

namespace educlient.Services
{
    public interface IKetQuaLamViecCaNhan
    {


        Task<List<int>> SoLuongCaseThucHienTrongTuan(string user, int year);
        Task<KetQuaLamViecCaNhanResult> KetQuaLamViecCaNhanReturn(string user, int year);
    }

    public class KetQuaLamViecCaNhan : IKetQuaLamViecCaNhan
    {
        private readonly IDbLiteContext database;
        private readonly IConfiguration config;
        const string TFS_TOKEN_BASE64 = "QVFcdGZzdXNlcjpyY3phdmVsczJ6ZGw2bDZqdDZ6cXRxdGp0YW1wMzQ1NDQyYm9ycXk3cGNyd2doem1icHFx";
        public static string TFS_HOST = Startup.tfsUrl;
        public KetQuaLamViecCaNhan(IConfiguration cf, IDbLiteContext dataContext)
        {
            config = cf;
            database = dataContext;
        }
        public async Task<KetQuaLamViecCaNhanResult> KetQuaLamViecCaNhanReturn(string user, int year)
        {
            List<int> SoGioLamViecTrongNgayData = await SoGioLamViecTrongNgay(user, year);
            List<int> SoLuongCaseThucHienData = await SoLuongCaseThucHienTrongTuan(user, year);
            List<int> soLuotCaseBiMoLaiData = await SoLuotCaseBiMoLai(user, year);
            List<float> SoGioUocLuongCaseData = await SoGioUocLuongCase(user, year);
            List<float> SoGioThucTeLamCaseData = await SoGioThucTeLamCase(user, year);
            List<float> meetingCaseData = await MeetingCase(user, year);
            List<float> SoGioLamThieuData = SoGioLamThieu(SoGioLamViecTrongNgayData, SoGioThucTeLamCaseData, meetingCaseData);
            List<float> phanTramTiLeMoCaseData = PhanTramTiLeMoCase(soLuotCaseBiMoLaiData, SoLuongCaseThucHienData);
            List<float> PhanTramTiLeChenhLechUocTinhVaThucTeData = PhanTramTiLeChenhLechUocTinhVaThucTe(SoGioUocLuongCaseData, SoGioThucTeLamCaseData);
            //List<float> SoGioLamThieuData = SoGioLamThieu(24, SoGioThucTeLamCaseData, meetingCaseData);


            SoGioUocLuongCaseData = SoGioUocLuongCaseData.Select(x => float.IsInfinity(x) || float.IsNaN(x) ? 0f : (float)Math.Round(x, 2)).ToList();
            SoGioThucTeLamCaseData = SoGioThucTeLamCaseData.Select(x => float.IsInfinity(x) || float.IsNaN(x) ? 0f : (float)Math.Round(x, 2)).ToList();
            meetingCaseData = meetingCaseData.Select(x => float.IsInfinity(x) || float.IsNaN(x) ? 0f : (float)Math.Round(x, 2)).ToList();
            phanTramTiLeMoCaseData = phanTramTiLeMoCaseData.Select(x => float.IsInfinity(x) || float.IsNaN(x) ? 0f : (float)Math.Round(x, 2)).ToList();
            PhanTramTiLeChenhLechUocTinhVaThucTeData = PhanTramTiLeChenhLechUocTinhVaThucTeData.Select(x => float.IsInfinity(x) || float.IsNaN(x) ? 0f : (float)Math.Round(x, 2)).ToList();


            return new KetQuaLamViecCaNhanResult
            {
                result = true,
                code = 200,
                message = "",
                data = new KetQuaLamViecCaNhanDataDO()
                {
                    //SoGioLamViecTrongNgay = new List<int>() { 0 },
                    //SoGioLamThieu = new List<int>() { 0 },
                    SoGioLamViecTrongNgay = SoGioLamViecTrongNgayData,
                    SoGioLamThieu = SoGioLamThieuData,
                    SoLuongCaseThucHienTrongTuan = SoLuongCaseThucHienData,
                    SoLuotCaseBiMoLai = soLuotCaseBiMoLaiData,
                    SoGioUocLuongCase = SoGioUocLuongCaseData,
                    SoGioThucTeLamCase = SoGioThucTeLamCaseData,
                    SoGioThamGiaMeeting = meetingCaseData,
                    PhanTramTiLeMoCase = phanTramTiLeMoCaseData,
                    PhanTramTiLeChenhLechUocLuongVaThucTe = PhanTramTiLeChenhLechUocTinhVaThucTeData
                }
            };
        }


        public async Task<List<float>> SoGioLamThieu(List<int> SoGioLamViecTrongNam, List<int> SoGioThucTeLamCase, List<int> SoGioThamGiaMeeting)
        {
            // Initialize the result list
            var soGioLamThieu = new List<float>();

            // Ensure the lists have the same length
            int length = Math.Min(SoGioLamViecTrongNam.Count, Math.Min(SoGioThucTeLamCase.Count, SoGioThamGiaMeeting.Count));

            // Calculate SoGioLamThieu for each week
            for (int i = 0; i < length; i++)
            {
                float gioLamThieu = SoGioLamViecTrongNam[i] - SoGioThucTeLamCase[i] - SoGioThamGiaMeeting[i];
                soGioLamThieu.Add(gioLamThieu);
            }

            return soGioLamThieu;
        }

        public async Task<List<int>> SoGioLamViecTrongNgay(string user, int year)
        {
            int currentWeek = GetCurrentWeekNumber(year);
            int totalWeeks = (year == DateTime.Now.Year) ? currentWeek : GetWeeksInYear(year);
            int numberOfDaysInWeek = 5;

            var dayOffTable = database.Table<DayOff>();
            var NhanSuTable = database.Table<AQMember>();
            var NghiCaNhanTable = database.Table<IndividualDayOff>();
            var ngayCongTacTable = database.Table<Commission>();

            var NgayCongTacData = ngayCongTacTable?.Query().Select(x => new Commission
            {
                id = x.id,
                dateFrom = x.dateFrom,
                dateTo = x.dateTo,
                memberList = x.memberList,
            }).ToList() ?? new List<Commission>();

            var NghiCaNhanData = NghiCaNhanTable?.Query().Select(x => new IndividualDayOff
            {
                id = x.id,
                dateFrom = x.dateFrom,
                dateTo = x.dateTo,
                memberId = x.memberId,
                reason = x.reason,
                isAnnual = x.isAnnual,
                isWithoutPay = x.isWithoutPay,
                note = x.note
            }).ToList() ?? new List<IndividualDayOff>();

            var NhanSuData = NhanSuTable?.Query().Select(x => new AQMember
            {
                id = x.id,
                TFSName = x.TFSName
            }).ToList() ?? new List<AQMember>();

            var dayOffData = dayOffTable?.Query().Select(x => new DayOff
            {
                id = x.id,
                dateFrom = x.dateFrom,
                dateTo = x.dateTo,
                sumDay = x.sumDay,
                reason = x.reason,
                note = x.note
            }).ToList() ?? new List<DayOff>();

            var IdNhanSuHienTai = NhanSuData.FirstOrDefault(n => n.TFSName == user)?.id;

            var matchingNghiCaNhanData = IdNhanSuHienTai != null
                ? NghiCaNhanData.Where(n => n.memberId == IdNhanSuHienTai).ToList()
                : new List<IndividualDayOff>();

            var matchingNgayCongTacData = IdNhanSuHienTai != null
                ? NgayCongTacData.Where(commission => commission.memberList.Any(member => member.id == IdNhanSuHienTai)).ToList()
                : new List<Commission>();

            var soNgayNghiChung = new List<int>();

            for (int week = 1; week <= totalWeeks; week++)
            {
                int workingDaysInWeek = numberOfDaysInWeek;

                foreach (var dayOff in dayOffData)
                {
                    if (IsInWeek(dayOff.dateFrom, year, week) || IsInWeek(dayOff.dateTo, year, week))
                    {
                        DateTime firstDayOfWeek = GetFirstDayOfWeek(year, week);
                        DateTime lastDayOfWeek = firstDayOfWeek.AddDays(4);

                        DateTime actualDateFrom = (dayOff.dateFrom < firstDayOfWeek) ? firstDayOfWeek : dayOff.dateFrom;
                        DateTime actualDateTo = (dayOff.dateTo > lastDayOfWeek) ? lastDayOfWeek : dayOff.dateTo;

                        int daysOffInWeek = (actualDateTo - actualDateFrom).Days + 1;
                        workingDaysInWeek -= daysOffInWeek;
                    }
                }

                foreach (var individualDayOff in matchingNghiCaNhanData)
                {
                    if (IsInWeek(individualDayOff.dateFrom, year, week) || IsInWeek(individualDayOff.dateTo, year, week))
                    {
                        DateTime firstDayOfWeek = GetFirstDayOfWeek(year, week);
                        DateTime lastDayOfWeek = firstDayOfWeek.AddDays(4);

                        DateTime actualDateFrom = (individualDayOff.dateFrom < firstDayOfWeek) ? firstDayOfWeek : individualDayOff.dateFrom;
                        DateTime actualDateTo = (individualDayOff.dateTo > lastDayOfWeek) ? lastDayOfWeek : individualDayOff.dateTo;

                        int daysOffInWeek = (actualDateTo - actualDateFrom).Days + 1;
                        workingDaysInWeek -= daysOffInWeek;
                    }
                }

                foreach (var commission in matchingNgayCongTacData)
                {
                    if (IsInWeek(commission.dateFrom, year, week) || IsInWeek(commission.dateTo, year, week))
                    {
                        DateTime firstDayOfWeek = GetFirstDayOfWeek(year, week);
                        DateTime lastDayOfWeek = firstDayOfWeek.AddDays(4);

                        DateTime actualDateFrom = (commission.dateFrom < firstDayOfWeek) ? firstDayOfWeek : commission.dateFrom;
                        DateTime actualDateTo = (commission.dateTo > lastDayOfWeek) ? lastDayOfWeek : commission.dateTo;

                        int daysOffInWeek = (actualDateTo - actualDateFrom).Days + 1;
                        workingDaysInWeek -= daysOffInWeek;
                    }
                }
                workingDaysInWeek *= 8;
                soNgayNghiChung.Add(workingDaysInWeek);
            }

            return soNgayNghiChung;
        }
        public async Task<List<int>> SoLuongCaseThucHienTrongTuan(string user, int year)
        {
            var tmpCaseCountList = new List<int>();
            int tmpCount = 0;
            string escapedUser = user.Replace("\\", "\\\\");
            var query = SoLuongCaseThucHienTrongTuanQuery2(escapedUser);
            var idsCaseThucHien = await GetCaseIds(query);
            var DetailCaseThucHien = await GetCaseDetails(idsCaseThucHien);
            List<ThongTinCase> thongTinCases = processSoLuongCaseThucHienTrongTuan(DetailCaseThucHien);
            List<int> returnData = CalSoLuongCaseTrongTuan(thongTinCases, year);
            return returnData;
            ////var soLuongCaseThucHienWiqlQuery = SoLuongCaseThucHienTrongTuanWiqlQuery(user);
            ////var idsCaseThucHien = await GetCaseIds(soLuongCaseThucHienWiqlQuery);

            ////if (idsCaseThucHien == null)
            ////{
            ////    return 0;
            ////}

            //var queries = GetWeeklyQueries(year, user);

            //// Output all queries
            //foreach (var query in queries)
            //{
            //    //var soLuongCaseThucHienWiqlQuery = SoLuongCaseThucHienTrongTuanWiqlQuery(user);
            //    var idsCaseThucHien = await GetCaseIds(query);
            //    if (idsCaseThucHien == null)
            //    {
            //        tmpCount = 0;
            //    }
            //    else
            //    {
            //        tmpCount = idsCaseThucHien.Count;
            //    }
            //    tmpCaseCountList.Add(tmpCount);
            //}
            //Debug.WriteLine(tmpCaseCountList);
            //return tmpCaseCountList;
        }

        public List<int> CalSoLuongCaseTrongTuan(List<ThongTinCase> thongTinCases, int year)
        {
            List<int> weeklyCaseCounts = new List<int>();
            int currentWeek = GetCurrentWeekNumber(year);
            int totalWeeks = (year == DateTime.Now.Year) ? currentWeek : GetWeeksInYear(year);
            for (int week = 1; week <= totalWeeks; week++)
            {
                int caseCount = 0;
                DateTime startDate = GetFirstDayOfWeek(year, week);
                DateTime endDate = startDate.AddDays(6);

                foreach (var caseInfo in thongTinCases)
                {
                    bool caseCountedForWeek = false;

                    // Check detaildate1 to detaildate10
                    for (int i = 1; i <= 10; i++)
                    {
                        var detailDate = GetDetailDate(caseInfo, i);
                        if (detailDate.HasValue && IsInWeek(detailDate.Value, year, week))
                        {
                            caseCount++;
                            caseCountedForWeek = true;
                            break;
                        }
                    }

                    // If case not counted based on detaildate, check other conditions
                    if (!caseCountedForWeek)
                    {
                        DateTime? targetDate = ParseDate(caseInfo.ngaydukien);
                        if (IsClosedOrProcessedCase(caseInfo.trangthai) &&
                            targetDate.HasValue &&
                            IsInWeek(targetDate.Value, year, week))
                        {
                            caseCount++;
                        }
                    }
                }

                weeklyCaseCounts.Add(caseCount);
            }

            return weeklyCaseCounts;
        }
        public Tuple<string, string> GetCurrentWeekDates()
        {
            DateTime today = DateTime.Now;
            int currentDayOfWeek = (int)today.DayOfWeek;
            DateTime startOfWeek = today.AddDays(-currentDayOfWeek).Date; // Start of the week
            DateTime endOfWeek = startOfWeek.AddDays(6).Date; // End of the week

            return Tuple.Create(startOfWeek.ToString("dd/MM/yyyy"), endOfWeek.ToString("dd/MM/yyyy"));
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
        public async Task<TfsCaseDetailModel> GetCaseDetails(List<workItemBase> caseIds)
        {
            const int batchSize = 200; // Adjust this value based on your needs
            string sTfsFieldList = "System.AssignedTo,System.Id,AQ.Customer,System.CreatedDate,System.Title,System.State,AQ.TargetDate,AQ.ReleaseDate,Microsoft.VSTS.Common.StateChangeDate,AQ.MailTo,AQ.Priority,AQ.CaseType,AQ.Module,AQ.Comment,AQ.ContractType,AQ.PriorityType,AQ.UserGuideRequested,AQ.ReviewCase,System.State,AQ.EstimateTime,AQ.DetailDate1,AQ.DetailDate2,AQ.DetailDate3,AQ.DetailDate4,AQ.DetailDate5,AQ.DetailDate6,AQ.DetailDate7,AQ.DetailDate8,AQ.DetailDate9,AQ.DetailDate10,AQ.DetailActualTime1,AQ.DetailActualTime2,AQ.DetailActualTime3,AQ.DetailActualTime4,AQ.DetailActualTime5,AQ.DetailActualTime6,AQ.DetailActualTime7,AQ.DetailActualTime8,AQ.DetailActualTime9,AQ.DetailActualTime10,AQ.ActualTime";

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


            //string sTfsFieldList = "System.AssignedTo,System.Id,AQ.Customer,System.CreatedDate,System.Title,System.State,AQ.TargetDate,AQ.ReleaseDate,Microsoft.VSTS.Common.StateChangeDate,AQ.MailTo,AQ.Priority,AQ.CaseType,AQ.Module,AQ.Comment,AQ.ContractType,AQ.PriorityType,AQ.UserGuideRequested,AQ.ReviewCase,System.State,AQ.EstimateTime,AQ.DetailDate1,AQ.DetailDate2,AQ.DetailDate3,AQ.DetailDate4,AQ.DetailDate5,AQ.DetailDate6,AQ.DetailDate7,AQ.DetailDate8,AQ.DetailDate9,AQ.DetailDate10,AQ.DetailActualTime1,AQ.DetailActualTime2,AQ.DetailActualTime3,AQ.DetailActualTime4,AQ.DetailActualTime5,AQ.DetailActualTime6,AQ.DetailActualTime7,AQ.DetailActualTime8,AQ.DetailActualTime9,AQ.DetailActualTime10,AQ.ActualTime";
            //string sCaseList = string.Join(",", caseIds.Select(s => s.id));

            //var tmpjson = await DoTfsQueryData(TFS_HOST,
            //    $"tfs/aq/Edusoft.Net-CS/_apis/wit/workitems?ids={sCaseList}&fields={sTfsFieldList}",
            //    "", "", TFS_TOKEN_BASE64);

            //return JsonConvert.DeserializeObject<TfsCaseDetailModel>(tmpjson);
        }
        public async Task<TfsCaseDetailModel> GetMeetingCaseDetails(List<workItemBase> caseIds)
        {
            string sTfsFieldList = "Microsoft.VSTS.CMMI.RequiredAttendee1,Microsoft.VSTS.CMMI.RequiredAttendee2,Microsoft.VSTS.CMMI.RequiredAttendee3,Microsoft.VSTS.CMMI.RequiredAttendee4,Microsoft.VSTS.CMMI.RequiredAttendee5,Microsoft.VSTS.CMMI.RequiredAttendee6,Microsoft.VSTS.CMMI.RequiredAttendee7,Microsoft.VSTS.CMMI.RequiredAttendee8,Microsoft.VSTS.CMMI.RequiredAttendee9,Microsoft.VSTS.CMMI.RequiredAttendee10,AQ.MinuteTakerTime,AQ.MeetingStart";
            string sCaseList = string.Join(",", caseIds.Select(s => s.id));

            var tmpjson = await DoTfsQueryData(TFS_HOST,
                $"tfs/aq/Edusoft.Net-CS/_apis/wit/workitems?ids={sCaseList}&fields={sTfsFieldList}",
                "", "", TFS_TOKEN_BASE64);

            return JsonConvert.DeserializeObject<TfsCaseDetailModel>(tmpjson);
        }
        public static (DateTime, DateTime) GetWeekStartAndEnd(DateTime referenceDate, int weekNumber, DayOfWeek firstDayOfWeek)
        {
            Calendar calendar = CultureInfo.CurrentCulture.Calendar;

            // Get the first day of the year
            DateTime firstDayOfYear = new DateTime(referenceDate.Year, 1, 1);

            // Adjust to the first specified day of the week
            DateTime firstWeekStart = firstDayOfYear;
            while (firstWeekStart.DayOfWeek != firstDayOfWeek)
            {
                firstWeekStart = firstWeekStart.AddDays(1);
            }

            // Calculate the start date of the desired week
            DateTime startOfWeek = firstWeekStart.AddDays((weekNumber - 1) * 7);

            // Calculate the end date of the desired week
            DateTime endOfWeek = startOfWeek.AddDays(6);

            return (startOfWeek, endOfWeek);
        }

        public List<string> GetWeeklyQueries(int year, string user)
        {
            var queries = new List<string>();
            string escapedUser = user.Replace("\\", "\\\\");

            int currentYear = DateTime.Now.Year;
            int maxWeekNumber = (year == currentYear) ? GetCurrentWeekNumber(year) : GetLastWeekNumber(year);

            for (int weekNumber = 1; weekNumber <= maxWeekNumber; weekNumber++)
            {
                (DateTime startOfWeek, DateTime endOfWeek) = GetWeekStartAndEnd(new DateTime(year, 1, 1), weekNumber, DayOfWeek.Monday);

                // Format the dates for the query
                string startDateFormatted = startOfWeek.ToString("yyyy-MM-ddTHH:mm:ss.fffffff");
                string endDateFormatted = endOfWeek.ToString("yyyy-MM-ddTHH:mm:ss.fffffff");

                // Build the query for this week
                string query = SoLuongCaseThucHienTrongTuanQuery(escapedUser, startDateFormatted, endDateFormatted);

                queries.Add(query);
            }

            return queries;
        }
        public string SoLuongCaseThucHienTrongTuanQuery(string user, string startDateFormatted, string endDateFormatted)
        {
            return $@"
{{
    ""query"": ""
        SELECT [System.Id]
        FROM WorkItems
        WHERE [System.TeamProject] = 'Edusoft.Net-CS'
        AND [System.AssignedTo] = '{user}'
        AND [System.WorkItemType] <> ''
        AND (
            (
                [System.State] NOT IN ('Đóng case', 'Đã xử lý')
                AND (
                    ([AQ.DetailDate1] >= '{startDateFormatted}' AND [AQ.DetailDate1] <= '{endDateFormatted}')
                    OR ([AQ.DetailDate2] >= '{startDateFormatted}' AND [AQ.DetailDate2] <= '{endDateFormatted}')
                    OR ([AQ.DetailDate3] >= '{startDateFormatted}' AND [AQ.DetailDate3] <= '{endDateFormatted}')
                    OR ([AQ.DetailDate4] >= '{startDateFormatted}' AND [AQ.DetailDate4] <= '{endDateFormatted}')
                    OR ([AQ.DetailDate5] >= '{startDateFormatted}' AND [AQ.DetailDate5] <= '{endDateFormatted}')
                    OR ([AQ.DetailDate6] >= '{startDateFormatted}' AND [AQ.DetailDate6] <= '{endDateFormatted}')
                    OR ([AQ.DetailDate7] >= '{startDateFormatted}' AND [AQ.DetailDate7] <= '{endDateFormatted}')
                    OR ([AQ.DetailDate8] >= '{startDateFormatted}' AND [AQ.DetailDate8] <= '{endDateFormatted}')
                    OR ([AQ.DetailDate9] >= '{startDateFormatted}' AND [AQ.DetailDate9] <= '{endDateFormatted}')
                    OR ([AQ.DetailDate10] >= '{startDateFormatted}' AND [AQ.DetailDate10] <= '{endDateFormatted}')
                )
            )
            OR 
            (
                ([System.State] IN ('Đóng case', 'Đã xử lý'))
                AND ([AQ.TargetDate] >= '{startDateFormatted}' AND [AQ.TargetDate] <= '{endDateFormatted}')
            )
        )
    ""
}}";
        }
        public string SoLuongCaseThucHienTrongTuanQuery2(string user)
        {
            return $@"
{{
    ""query"": ""
        SELECT [System.Id]
        FROM WorkItems
        WHERE [System.TeamProject] = 'Edusoft.Net-CS'
        AND [System.AssignedTo] = '{user}'
        AND [System.WorkItemType] <> ''
        
    ""
}}";
        }
        public string reOpenCasesQuery(string user)
        {
            return $@"
            {{
                ""query"": ""
                    SELECT [System.Id]
                    FROM WorkItems
                    WHERE [System.TeamProject] = 'Edusoft.Net-CS'
                    AND [System.AssignedTo] = '{user}'
                    AND [System.WorkItemType] <> ''
                    AND ([System.State] = 'Đóng case' OR [System.State] = 'Đã xử lý')
                    AND [AQ.ReopenCount] >= 1
                ""
            }}";
        }
        public string MeetingCaseQuery(string user)
        {
            return $@"
                {{
                    ""query"": ""
                        SELECT [System.Id]
                        FROM WorkItems
                        WHERE 
                            ([Microsoft.VSTS.CMMI.RequiredAttendee1] = '{user}' 
                            OR [Microsoft.VSTS.CMMI.RequiredAttendee2] = '{user}' 
                            OR [Microsoft.VSTS.CMMI.RequiredAttendee3] = '{user}' 
                            OR [Microsoft.VSTS.CMMI.RequiredAttendee4] = '{user}' 
                            OR [Microsoft.VSTS.CMMI.RequiredAttendee5] = '{user}' 
                            OR [Microsoft.VSTS.CMMI.RequiredAttendee6] = '{user}' 
                            OR [Microsoft.VSTS.CMMI.RequiredAttendee7] = '{user}' 
                            OR [Microsoft.VSTS.CMMI.RequiredAttendee8] = '{user}' 
                            OR [Microsoft.VSTS.CMMI.RequiredAttendee9] = '{user}' 
                            OR [Microsoft.VSTS.CMMI.RequiredAttendee10] = '{user}')
                        ORDER BY [AQ.MeetingStart]
                    ""
                }}";
        }
        public string SoGioUocLuongCaseQuery(string user)
        {
            return $@"
            {{
                ""query"": ""
                    SELECT [System.Id]
                    FROM WorkItems
                    WHERE [System.TeamProject] = 'Edusoft.Net-CS'
                    AND [System.AssignedTo] = '{user}'
                    AND [System.WorkItemType] <> ''
                ""
            }}";
        }
        public async Task<List<float>> MeetingCase(string user, int year)
        {
            string escapedUser = user.Replace("\\", "\\\\");
            var query = MeetingCaseQuery(escapedUser);
            var idsCaseThucHien = await GetCaseIds(query);
            var DetailCaseMoLai = await GetMeetingCaseDetails(idsCaseThucHien);
            List<ThongTinCase> thongTinCases = processSoSoGioUocLuongCase(DetailCaseMoLai);
            List<float> returnData = SumGioMeetingCase(thongTinCases, year);
            return returnData;
        }
        public async Task<List<float>> SoGioUocLuongCase(string user, int year)
        {
            string escapedUser = user.Replace("\\", "\\\\");
            var query = SoGioUocLuongCaseQuery(escapedUser);
            var idsCaseThucHien = await GetCaseIds(query);
            var DetailCaseMoLai = await GetCaseDetails(idsCaseThucHien);
            List<ThongTinCase> thongTinCases = processSoSoGioUocLuongCase(DetailCaseMoLai);
            List<float> returnData = SumGioUocLuongCase(thongTinCases, year);
            return returnData;
        }
        public async Task<List<float>> SoGioThucTeLamCase(string user, int year)
        {
            string escapedUser = user.Replace("\\", "\\\\");
            var query = SoGioUocLuongCaseQuery(escapedUser);
            var idsCaseThucHien = await GetCaseIds(query);
            var DetailCaseMoLai = await GetCaseDetails(idsCaseThucHien);
            List<ThongTinCase> thongTinCases = processSoSoGioUocLuongCase(DetailCaseMoLai);
            List<float> returnData = SumGioThucTeCase(thongTinCases, year);
            return returnData;
        }
        public async Task<List<int>> SoLuotCaseBiMoLai(string user, int year)
        {
            string escapedUser = user.Replace("\\", "\\\\");
            var query = reOpenCasesQuery(escapedUser);
            var idsCaseThucHien = await GetCaseIds(query);
            var DetailCaseMoLai = await GetCaseDetails(idsCaseThucHien);
            List<ThongTinCase> thongTinCases = processSoCaseMoLai(DetailCaseMoLai);

            List<int> returnData = GroupCasesByWeek(thongTinCases, year);
            return returnData;
        }
        public List<ThongTinCase> processSoCaseMoLai(TfsCaseDetailModel caseDetails)
        {
            var dt = CreateDataTable();
            if (caseDetails == null)
            {
                return null;
            }
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
        public List<ThongTinCase> processSoSoGioUocLuongCase(TfsCaseDetailModel caseDetails)
        {
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
        public List<ThongTinCase> processSoLuongCaseThucHienTrongTuan(TfsCaseDetailModel caseDetails)
        {
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
                case "aq.actualtime":
                    dr["actualtime"] = float.Parse(kvp.Value);
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
                case "microsoft.vsts.cmmi.requiredattendee1":
                    dr["RequiredAttendee1"] = kvp.Value;
                    break;
                case "microsoft.vsts.cmmi.requiredattendee2":
                    dr["requiredattendee2"] = kvp.Value;
                    break;
                case "microsoft.vsts.cmmi.requiredattendee3":
                    dr["requiredattendee3"] = kvp.Value;
                    break;
                case "microsoft.vsts.cmmi.requiredattendee4":
                    dr["requiredattendee4"] = kvp.Value;
                    break;
                case "microsoft.vsts.cmmi.requiredattendee5":
                    dr["requiredattendee5"] = kvp.Value;
                    break;
                case "microsoft.vsts.cmmi.requiredattendee6":
                    dr["requiredattendee6"] = kvp.Value;
                    break;
                case "microsoft.vsts.cmmi.requiredattendee7":
                    dr["requiredattendee7"] = kvp.Value;
                    break;
                case "microsoft.vsts.cmmi.requiredattendee8":
                    dr["requiredattendee8"] = kvp.Value;
                    break;
                case "microsoft.vsts.cmmi.requiredattendee9":
                    dr["requiredattendee9"] = kvp.Value;
                    break;
                case "microsoft.vsts.cmmi.requiredatendee10":
                    dr["requiredatendee10"] = kvp.Value;
                    break;
                case "aq.minutetakertime":
                    dr["minutetakertime"] = float.Parse(kvp.Value);
                    break;
                case "aq.meetingstart":
                    dr["meetingstart"] = kvp.Value;
                    break;
            }
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
            dt.Columns.Add("requiredAttendee1", typeof(string));
            dt.Columns.Add("requiredAttendee2", typeof(string));
            dt.Columns.Add("requiredAttendee3", typeof(string));
            dt.Columns.Add("requiredAttendee4", typeof(string));
            dt.Columns.Add("requiredAttendee5", typeof(string));
            dt.Columns.Add("requiredAttendee6", typeof(string));
            dt.Columns.Add("requiredAttendee7", typeof(string));
            dt.Columns.Add("requiredAttendee8", typeof(string));
            dt.Columns.Add("requiredAttendee9", typeof(string));
            dt.Columns.Add("requiredAttendee10", typeof(string));
            dt.Columns.Add("minutetakertime", typeof(int));
            dt.Columns.Add("meetingstart", typeof(string));
            dt.Columns.Add("actualtime", typeof(float));

            return dt;
        }
        public int GetCurrentWeekNumber(int year)
        {
            DateTime currentDate = DateTime.Now;
            Calendar calendar = CultureInfo.CurrentCulture.Calendar;

            // Get the week number for the current date
            return calendar.GetWeekOfYear(currentDate, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public int GetLastWeekNumber(int year)
        {
            DateTime lastDayOfYear = new DateTime(year, 12, 31);
            Calendar calendar = CultureInfo.CurrentCulture.Calendar;

            // Get the week number for the last day of the year
            return calendar.GetWeekOfYear(lastDayOfYear, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        private List<int> GroupCasesByWeek(List<ThongTinCase> cases, int year)
        {
            var result = new List<int>();
            int currentWeek = GetCurrentWeekNumber(year);
            int totalWeeks = (year == DateTime.Now.Year) ? currentWeek : GetWeeksInYear(year);

            for (int week = 1; week <= totalWeeks; week++)
            {
                int casesInWeek = 0;
                if (cases != null)
                {
                    casesInWeek = cases.Count(c => IsInWeek(DateTime.Parse(c.ngaydukien), year, week));
                }
                result.Add(casesInWeek);
            }

            return result;
        }
        private List<float> SumGioUocLuongCase(List<ThongTinCase> cases, int year)
        {
            var result = new List<float>();
            var hours = 0;
            int currentWeek = GetCurrentWeekNumber(year);
            int totalWeeks = (year == DateTime.Now.Year) ? currentWeek : GetWeeksInYear(year);

            for (int week = 1; week <= totalWeeks; week++)
            {
                List<ThongTinCase> casesInWeek = cases
        .Where(c => DateTime.TryParse(c.ngaydukien, out var targetDate) && IsInWeek(targetDate, year, week))
        .ToList();

                float sumEstimateTimeInWeek = casesInWeek
        .Where(c => IsInWeek(DateTime.Parse(c.ngaydukien), year, week))
        .Sum(c => c.estimatetime.HasValue ? c.estimatetime.Value : 0);

                result.Add(sumEstimateTimeInWeek);
            }

            return result;
        }
        private List<float> SumGioMeetingCase(List<ThongTinCase> cases, int year)
        {
            var result = new List<float>();
            var hours = 0;
            int currentWeek = GetCurrentWeekNumber(year);
            int totalWeeks = (year == DateTime.Now.Year) ? currentWeek : GetWeeksInYear(year);

            for (int week = 1; week <= totalWeeks; week++)
            {

                int sumEstimateTimeInWeek = cases
        .Where(c => IsInWeek(DateTime.Parse(c.MeetingStart), year, week))
        .Sum(c => c.MinuteTakerTime.HasValue ? c.MinuteTakerTime.Value : 0);

                result.Add(sumEstimateTimeInWeek / 60);
            }

            return result;
        }
        //private List<float> SumGioThucTeCase(List<ThongTinCase> cases, int year)
        //{
        //    var result = new List<float>();
        //    var hours = 0;
        //    int currentWeek = GetCurrentWeekNumber(year);
        //    int totalWeeks = (year == DateTime.Now.Year) ? currentWeek : GetWeeksInYear(year);

        //    for (int week = 1; week <= totalWeeks; week++)
        //    {

        //        List<ThongTinCase> casesInWeek = cases
        //.Where(c => DateTime.TryParse(c.ngaydukien, out var targetDate) && IsInWeek(targetDate, year, week))
        //.ToList();

        //        float sumActualTimeInWeek = 0;

        //        foreach (var c in casesInWeek)
        //        {
        //            bool hasAnyDetailDate = Enumerable.Range(1, 10)
        //                .Any(i => (DateTime?)c.GetType().GetProperty($"detaildate{i}")?.GetValue(c) != null);

        //            if (!hasAnyDetailDate)
        //            {
        //                // If no detaildate is set, distribute actualtime evenly across weeks
        //                if (c.actualtime.HasValue)
        //                {
        //                    sumActualTimeInWeek += c.actualtime.Value;
        //                    Debug.WriteLine($"Case with actual time: {c.macase}, {sumActualTimeInWeek}, {c.actualtime}");
        //                }
        //            }
        //            else
        //            {
        //                // If at least one detaildate is set, use the original logic
        //                for (int i = 1; i <= 10; i++)
        //                {

        //                    var dateProperty = c.GetType().GetProperty($"detaildate{i}");
        //                    var timeProperty = c.GetType().GetProperty($"detailactualtime{i}");

        //                    if (dateProperty != null && timeProperty != null)
        //                    {
        //                        DateTime? date = (DateTime?)dateProperty.GetValue(c);
        //                        float? time = (float?)timeProperty.GetValue(c);

        //                        if (date.HasValue && IsInWeek(date.Value, year, week))
        //                        {
        //                            sumActualTimeInWeek += time ?? 0;
        //                            Debug.WriteLine($"{c.macase}, {sumActualTimeInWeek}, {c.actualtime}");
        //                        }
        //                    }
        //                }
        //            }
        //        }


        //        //            float sumActualTimeInWeek = casesInWeek.Sum(c =>

        //        //    (c.detaildate1.HasValue && IsInWeek(c.detaildate1.Value, year, week) ? c.detailactualtime1 ?? 0 : 0) +
        //        //    (c.detaildate2.HasValue && IsInWeek(c.detaildate2.Value, year, week) ? c.detailactualtime2 ?? 0 : 0) +
        //        //    (c.detaildate3.HasValue && IsInWeek(c.detaildate3.Value, year, week) ? c.detailactualtime3 ?? 0 : 0) +
        //        //    (c.detaildate4.HasValue && IsInWeek(c.detaildate4.Value, year, week) ? c.detailactualtime4 ?? 0 : 0) +
        //        //    (c.detaildate5.HasValue && IsInWeek(c.detaildate5.Value, year, week) ? c.detailactualtime5 ?? 0 : 0) +
        //        //    (c.detaildate6.HasValue && IsInWeek(c.detaildate6.Value, year, week) ? c.detailactualtime6 ?? 0 : 0) +
        //        //    (c.detaildate7.HasValue && IsInWeek(c.detaildate7.Value, year, week) ? c.detailactualtime7 ?? 0 : 0) +
        //        //    (c.detaildate8.HasValue && IsInWeek(c.detaildate8.Value, year, week) ? c.detailactualtime8 ?? 0 : 0) +
        //        //    (c.detaildate9.HasValue && IsInWeek(c.detaildate9.Value, year, week) ? c.detailactualtime9 ?? 0 : 0) +
        //        //    (c.detaildate10.HasValue && IsInWeek(c.detaildate10.Value, year, week) ? c.detailactualtime10 ?? 0 : 0)
        //        //);



        //        result.Add(sumActualTimeInWeek);
        //    }

        //    return result;
        //}
        private List<float> SumGioThucTeCase(List<ThongTinCase> cases, int year)
        {
            var result = new List<float>();
            int currentWeek = GetCurrentWeekNumber(year);
            int totalWeeks = (year == DateTime.Now.Year) ? currentWeek : GetWeeksInYear(year);

            for (int week = 1; week <= totalWeeks; week++)
            {
                float sumActualTimeInWeek = 0;

                foreach (var c in cases)
                {
                    bool hasAnyDetailDate = Enumerable.Range(1, 10)
                        .Any(i => (DateTime?)c.GetType().GetProperty($"detaildate{i}")?.GetValue(c) != null);

                    if (!hasAnyDetailDate)
                    {
                        // If no detaildate is set, distribute actualtime evenly across weeks
                        if (c.actualtime.HasValue)
                        {
                            DateTime? targetDate;
                            if (DateTime.TryParse(c.ngaydukien, out var parsedDate))
                            {
                                targetDate = parsedDate;
                            }
                            else
                            {
                                continue; // Skip this case if ngaydukien is invalid
                            }

                            if (IsInWeek(targetDate.Value, year, week))
                            {
                                sumActualTimeInWeek += c.actualtime.Value;
                                Debug.WriteLine($"Case with actual time: {c.macase}, {sumActualTimeInWeek}, {c.actualtime}");
                            }
                        }
                    }
                    else
                    {
                        // If at least one detaildate is set, use the detailed logic
                        for (int i = 1; i <= 10; i++)
                        {
                            var dateProperty = c.GetType().GetProperty($"detaildate{i}");
                            var timeProperty = c.GetType().GetProperty($"detailactualtime{i}");

                            if (dateProperty != null && timeProperty != null)
                            {
                                DateTime? date = (DateTime?)dateProperty.GetValue(c);
                                float? time = (float?)timeProperty.GetValue(c);

                                if (date.HasValue && IsInWeek(date.Value, year, week))
                                {
                                    sumActualTimeInWeek += time ?? 0;
                                    Debug.WriteLine($"{c.macase}, {sumActualTimeInWeek}, {time}");
                                }
                            }
                        }
                    }
                }

                result.Add(sumActualTimeInWeek);
            }

            return result;
        }
        private bool IsInWeek(DateTime date, int year, int weekNumber)
        {
            Calendar calendar = CultureInfo.CurrentCulture.Calendar;
            var dateInYear = date.Year == year;
            var weekOfYear = calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            if (dateInYear && weekOfYear == weekNumber)
            {
                DateTime firstDayOfYear = new DateTime(year, 1, 1);
                int daysOffset = (weekNumber - 1) * 7;

                // Get the first day of the week
                DateTime firstDayOfWeek = firstDayOfYear.AddDays(daysOffset - (int)firstDayOfYear.DayOfWeek + (int)DayOfWeek.Monday);

                // Get the last day of the week
                DateTime lastDayOfWeek = firstDayOfWeek.AddDays(6);

                // Log the start and end date of the week
                Debug.WriteLine($"Week {weekNumber} of {year}: {firstDayOfWeek.ToShortDateString()} to {lastDayOfWeek.ToShortDateString()}");
            }
            return dateInYear && weekOfYear == weekNumber;
        }

        private List<float> PhanTramTiLeMoCase(List<int> SoLuotCaseBiMoLai, List<int> SoLuongCaseThucHienTrongTuan)
        {
            // Initialize a list to store the results
            List<float> phanTramTiLe = new List<float>();

            // Loop through each element in the lists
            for (int i = 0; i < SoLuotCaseBiMoLai.Count; i++)
            {
                // Calculate the percentage
                float percentage = (float)SoLuotCaseBiMoLai[i] * 100 / SoLuongCaseThucHienTrongTuan[i];

                // Add the result to the list
                phanTramTiLe.Add(percentage);
            }

            // Return the list of percentages
            return phanTramTiLe;
        }
        private List<float> PhanTramTiLeChenhLechUocTinhVaThucTe(List<float> UocTinh, List<float> ThucTe)
        {
            // Initialize a list to store the results
            List<float> chenhLechTiLe = new List<float>();

            // Check if the lists have the same length
            if (UocTinh.Count != ThucTe.Count)
            {
                throw new ArgumentException("The lists UocTinh and ThucTe must have the same length.");
            }

            // Loop through each element in the lists
            for (int i = 0; i < UocTinh.Count; i++)
            {
                float percentageDifference = 0;
                // Ensure UocTinh[i] is not zero to avoid division by zero
                if (UocTinh[i] == 0)
                {

                    percentageDifference = 0;
                    //throw new DivideByZeroException("UocTinh cannot have zero values.");
                }

                // Calculate the percentage difference
                percentageDifference = (ThucTe[i] * 100 / UocTinh[i]) - 100;

                // Add the result to the list
                chenhLechTiLe.Add(percentageDifference);
            }

            // Return the list of percentage differences
            return chenhLechTiLe;
        }
        private List<float> SoGioLamThieu(List<int> SoGioLamTrongTuan, List<float> SoGioThucTeLamCase, List<float> SoGioThamGiaMeeting)
        {
            // Initialize a list to store the results
            List<float> soGioLamThieu = new List<float>();

            // Check if all lists have the same length
            if (SoGioLamTrongTuan.Count != SoGioThucTeLamCase.Count || SoGioLamTrongTuan.Count != SoGioThamGiaMeeting.Count)
            {
                throw new ArgumentException("All input lists must have the same length.");
            }

            // Loop through each element in the lists
            for (int i = 0; i < SoGioLamTrongTuan.Count; i++)
            {
                // Calculate the number of hours lacking
                float hoursLacking = SoGioLamTrongTuan[i] - SoGioThucTeLamCase[i] - SoGioThamGiaMeeting[i];

                // Add the result to the list
                soGioLamThieu.Add(hoursLacking);
            }

            // Return the list of hours lacking
            return soGioLamThieu;
        }

        private DateTime? GetDetailDate(ThongTinCase caseInfo, int index)
        {
            return index switch
            {
                1 => caseInfo.detaildate1,
                2 => caseInfo.detaildate2,
                3 => caseInfo.detaildate3,
                4 => caseInfo.detaildate4,
                5 => caseInfo.detaildate5,
                6 => caseInfo.detaildate6,
                7 => caseInfo.detaildate7,
                8 => caseInfo.detaildate8,
                9 => caseInfo.detaildate9,
                10 => caseInfo.detaildate10,
                _ => null
            };
        }
        private bool IsClosedOrProcessedCase(string state)
        {
            return state == "Đóng case" || state == "Đã xử lý";
        }

        private int GetWeeksInYear(int year)
        {
            // This method calculates the number of weeks in a year
            DateTime lastDay = new DateTime(year, 12, 31);
            Calendar calendar = CultureInfo.CurrentCulture.Calendar;
            return calendar.GetWeekOfYear(lastDay, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }
        private DateTime GetFirstDayOfWeek(int year, int weekNumber)
        {
            DateTime firstDayOfYear = new DateTime(year, 1, 1);
            int daysOffset = (weekNumber - 1) * 7;
            return firstDayOfYear.AddDays(daysOffset - (int)firstDayOfYear.DayOfWeek + (int)DayOfWeek.Monday);
        }
        private DateTime? ParseDate(string dateString)
        {
            return DateTime.TryParse(dateString, out DateTime result) ? result : (DateTime?)null;
        }

    }
}
