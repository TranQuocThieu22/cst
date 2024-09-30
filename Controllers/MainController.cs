using educlient.Data;
using educlient.Models;
using educlient.Services;
using educlient.Utils;
using HtmlAgilityPack;
using LiteDB;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BigID = System.Int64;
using JsonConvert = Newtonsoft.Json.JsonConvert;


namespace educlient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MainController : ControllerBase
    {
        private readonly IDbLiteContext database;
        private readonly ITFSAccountService _tFSAccountService;
        ISession Session
        {
            get { return HttpContext.Session; }
        }

        EduClient currentUser
        {
            get
            {
                var user = Session.GetString("current-user");
                if (!string.IsNullOrEmpty(user))
                    return JsonConvert.DeserializeObject<EduClient>(user);
                return null;
            }
        }

        [HttpPost, Route("login")]
        public async Task<object> Login(LoginModel log)
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "clients.dat");
            var clients = JsonConvert.DeserializeObject<List<EduClient>>(System.IO.File.ReadAllText(dir));
            EduClient user = clients.FirstOrDefault(r => r.MaTruong?.ToLower() == log?.username?.ToLower() && r.Pass == log.password);
            EduClient TFSUser;
            try
            {
                TFSUser = await _tFSAccountService.LoginAsync(log);
            }
            catch (Exception e)
            {
                return "Loi roi" + e;
            }
            if (user != null)
            {
                user.User = null;
                user.Group = null;
                user.userData = null;
                _logger.LogInformation("Login Success: " + log.username);
                Session.SetString("current-user", JsonConvert.SerializeObject(user));
                return user;
            }
            else if (TFSUser != null)
            {
                var tb = database.Table<DsThongTinCaNhanDataDO>();
                TFSUser.userData = tb.Query()
                .Where(x => x.TFSName == TFSUser.User.ToLower())
                .FirstOrDefault();

                _logger.LogInformation("Login Success: " + log.username);
                Session.SetString("current-user", JsonConvert.SerializeObject(TFSUser));
                return TFSUser;
            }
            else
            {
                user = new EduClient { TenTruong = "Username or password is incorrect!" };
                _logger.LogInformation("Login Fail: " + log.username);
                return user;
            }

        }

        [HttpGet, Route("logout")]
        public object LogOut()
        {
            var u = currentUser;
            if (u != null)
                _logger.LogInformation("LogOut: " + currentUser.MaTruong);
            Session.Clear();
            return "Success!";
        }

        string dataFile
        {
            get { return Path.Combine(AppContext.BaseDirectory, "infos.dat"); }
        }

        [HttpPost, Route("upload")]
        public ResultModel Upload([FromBody] IEnumerable<EduCase> list)
        {
            if (list?.Count() > 0)
            {
                System.IO.File.WriteAllText(dataFile, JsonConvert.SerializeObject(list));
                _logger.LogInformation("Admin Upload");
                return new ResultModel { data = list.Count() };
            }
            return new ResultModel { error = "List is empty" };
        }

        private readonly ILogger<MainController> _logger;
        private readonly IConfiguration config;

        public MainController(ILogger<MainController> logger, IConfiguration cf, IDbLiteContext dataContext, ITFSAccountService tFSAccountService)
        {
            _logger = logger;
            config = cf;
            database = dataContext;
            _tFSAccountService = tFSAccountService;
            // connStr = Configuration.GetConnectionString("ProdConnection");
            // m_connStr = @"Server=192.168.1.204;Database=AQTech.KB;User Id=dev;Pwd=dev@edusoft;Connect Timeout=1800";
        }

        // AQ\tfsuser:2lvmoc5arhu2pshlcmh6kywgzgf3evpxgydjmefyjbf5jtvcw6za
        // Expired: 3/10/2022 3:16:51 PM

        public static string TFS_HOST = Startup.tfsUrl;

        const string TFS_TOKEN_BASE64 = "QVFcdGZzdXNlcjpyY3phdmVsczJ6ZGw2bDZqdDZ6cXRxdGp0YW1wMzQ1NDQyYm9ycXk3cGNyd2doem1icHFx";

        async Task<string> DoTfsQueryData(string pTfsHost, string pbaseUrl, string pQueryAppend, string pPOSTBody, string pBasicToken)
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

        async Task<string> DoTfsQueryData_CreateCase(string pTfsHost, string pbaseUrl, dynamic pPOSTBody, string pBasicToken)
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
                    HttpResponseMessage res;
                    HttpContent content_ = new StringContent(pPOSTBody.ToString(), Encoding.UTF8, "application/json-patch+json");
                    res = await client.PatchAsync(pbaseUrl, content_);
                    try
                    {
                        res.EnsureSuccessStatusCode();
                        using (HttpContent content = res.Content)
                        {
                            string data = await content.ReadAsStringAsync();
                            return data;
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
                return "";
            }
        }

        async Task<string> DoTfsQueryData_GetOneCase(string pTfsHost, string pbaseUrl, string pBasicToken)
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
                    HttpResponseMessage res;
                    HttpContent content_ = new StringContent("", Encoding.UTF8, "application/json");
                    res = await client.GetAsync(pbaseUrl);
                    try
                    {
                        res.EnsureSuccessStatusCode();
                        using (HttpContent content = res.Content)
                        {
                            string data = await content.ReadAsStringAsync();
                            return data;
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
                return "";
            }
        }

        public async Task<List<workItem0>> DoTfsFetchData(string pProject, string pProjectTeam, string pWIType, string pAQCustomer, string pFieldList, string pStateList, string pAppendWHERE, string macase)
        {
            //SELECT [System.Id] FROM WorkItems
            //macase = "36811";
            string sWIQL = "{\"query\": \"SELECT [System.Id] FROM WorkItems"
                                        + " WHERE ([System.TeamProject]='" + pProject + "')"
                                            + (pWIType.Length > 0 ? " AND ([System.WorkItemType] IN (" + pWIType + "))" : "")
                                            + (pAQCustomer.Length > 0 ? " AND ([AQ.Customer] IN (" + pAQCustomer + "))" : "")
                                            + " AND ([System.State] NOT IN ('Removed', 'Rejected','Hủy'))" //,'Done','Đóng case','Đã xử lý','Đã gửi mail'
                                            + (pStateList.Length > 0 ? " AND ([System.State] IN (" + pStateList + "))" : "")
                                            + (pAppendWHERE.Length > 0 ? " AND (" + pAppendWHERE + ")" : "")
                                            + (macase.Length > 0 ? " AND ([System.Id] IN (" + macase + "))" : "")
                                            ;

            sWIQL += " ORDER BY [System.CreatedDate] " + "\"}";

            var jsonCasesId = await DoTfsQueryData(TFS_HOST
                                            , "tfs/aq/" + pProject + "/" + pProjectTeam + "/_apis/wit/wiql?api-version=4.1"
                                            , "", sWIQL, TFS_TOKEN_BASE64);

            if (string.IsNullOrEmpty(jsonCasesId))
                return new List<workItem0>();

            var lstCasesId = JsonConvert.DeserializeObject<TfsCaseListModel>(jsonCasesId);
            if (lstCasesId.workItems.Count == 0)
                return new List<workItem0>();

            int MAX_CASES = 100;
            int i = 0;

            List<workItem0> lstReturn = new List<workItem0>();

            var lst = lstCasesId.workItems;
            while (lst.Any())
            {
                i++;
                string sCaseList = "";
                foreach (var s in lst.Take(MAX_CASES).ToList())
                    sCaseList += (sCaseList.Length > 0 ? "," : "") + s.id;

                string sTfsFieldList = "System.AssignedTo,System.Id,AQ.Customer,System.CreatedDate,System.Title,System.State,AQ.TargetDate,AQ.ReleaseDate,Microsoft.VSTS.Common.StateChangeDate,AQ.MailTo,AQ.Priority,AQ.CaseType,AQ.Module,AQ.Comment,AQ.ContractType,AQ.PriorityType,AQ.UserGuideRequested,AQ.ReviewCase";

                if (macase.Length > 0)
                {
                    sTfsFieldList = (pFieldList?.Length == 0 ? sTfsFieldList + ",System.Description,Microsoft.VSTS.Common.DescriptionHtml" : pFieldList);
                }
                else
                {
                    sTfsFieldList = (pFieldList?.Length == 0 ? sTfsFieldList : pFieldList);
                }

                var tmpjson = await DoTfsQueryData(TFS_HOST
                                            , "tfs/aq/" + pProject + "/_apis/wit/workitems?ids=" + sCaseList
                                            , "&fields=" + sTfsFieldList
                                            , ""
                                            , TFS_TOKEN_BASE64);

                var tmp = JsonConvert.DeserializeObject<TfsCaseDetailModel>(tmpjson);

                if (tmp != null)
                    lstReturn.AddRange(tmp.value);

                lst = lst.Skip(MAX_CASES).ToList();
            }

            return lstReturn;
        }

        async Task<List<workItem0>> DoTfsFetchData_CreateCase(dynamic body)
        {
            var jsonCases = await DoTfsQueryData_CreateCase(TFS_HOST
                                            , "tfs/aq/Edusoft.Net-CS/_apis/wit/workitems/$CS Case?api-version=4.1"
                                            , body
                                            , TFS_TOKEN_BASE64);

            if (string.IsNullOrEmpty(jsonCases))
                return new List<workItem0>();

            var lstCases = JsonConvert.DeserializeObject<workItem0>(jsonCases);
            return new List<workItem0> { lstCases };
        }

        async Task<List<workItem0>> DoTfsFetchData_UpdateCase(int macase, string body)
        {
            var jsonCases = await DoTfsQueryData_CreateCase(TFS_HOST
                                            , "tfs/aq/Edusoft.Net-CS/_apis/wit/workitems/" + macase + "?api-version=4.1"
                                            , body
                                            , TFS_TOKEN_BASE64);

            if (string.IsNullOrEmpty(jsonCases))
                return new List<workItem0>();

            var lstCases = JsonConvert.DeserializeObject<workItem0>(jsonCases);
            return new List<workItem0> { lstCases };

        }

        async Task<List<workItem0>> DoTfsFetchData_GetOne(int macase)
        {
            var jsonCases = await DoTfsQueryData_GetOneCase(TFS_HOST
                                            , "tfs/aq/Edusoft.Net-CS/_apis/wit/workitems/" + macase + "?api-version=4.1"
                                            , TFS_TOKEN_BASE64);

            if (string.IsNullOrEmpty(jsonCases))
                return new List<workItem0>();

            var lstCases = JsonConvert.DeserializeObject<workItem0>(jsonCases);
            return new List<workItem0> { lstCases };
        }

        [HttpPost, Route("viewonecase")]
        public CSCaseResult ViewOnCase(CSCaseBindingModel model)
        {
            if (currentUser == null)
            {
                return new CSCaseResult { code = 500, result = false, message = "Not Login" };
            }
            List<workItem0> lstAll;
            var lstAll0 = DoTfsFetchData_GetOne(Int32.Parse(model.filter?.macase));
            lstAll = lstAll0.Result;

            if (lstAll != null && lstAll.Count > 0)
            {
                var datatemp = new CSCaseDataDO
                {
                    is_tfs = true,
                    onecase = lstAll
                };
                return new CSCaseResult { data = datatemp, code = 200, result = true };
            }
            else
            {
                return new CSCaseResult { data = null, code = 400, result = false };
            }

        }

        [HttpPost, Route("cscase")]
        public async Task<CSCaseResult> ViewCsCase(CSCaseBindingModel model)
        {
            if (currentUser == null)
            {
                return new CSCaseResult { code = 500, result = false, message = "Not Login" };
            }

            //get data thong tin ma truong ten truong trong file .dat
            var dir = Path.Combine(AppContext.BaseDirectory, "clients.dat");
            var clients = JsonConvert.DeserializeObject<List<EduClient>>(System.IO.File.ReadAllText(dir));

            string list_matruong = "";

            if (currentUser?.Roles?.ToLower().Equals("admin", StringComparison.OrdinalIgnoreCase) == true
            || currentUser?.Roles?.ToLower().Equals("administrator", StringComparison.OrdinalIgnoreCase) == true)
            {
                list_matruong = "";
            }
            else
            {
                var temp = clients.Where(s => s.MaTruong.StartsWith(currentUser.MaTruong)).ToList();
                List<string> empnames = (from e in temp select e.MaTruong).ToList();
                string StringMaTruong = string.Join("','", empnames);
                list_matruong = "'" + StringMaTruong + "'";
            }

            string ngayBatDau = "'01/01/2022'";
            string ngayKetThuc = "'29/12/2024'";
            if (model.dateRange == "22-23")
            {
                ngayBatDau = "'01/01/2022'";
                ngayKetThuc = "'01/01/2023'";
            }
            else if (model.dateRange == "23-24")
            {
                ngayBatDau = "'01/01/2023'";
                ngayKetThuc = "'01/01/2024'";
            }
            else if (model.dateRange == "24-25")
            {
                ngayBatDau = "'01/01/2024'";
                ngayKetThuc = "'01/01/2025'";
            }
            string list_trangthai = ""; // "'Mở case', 'Đang xử lý', 'Đã xử lý', 'Đã gửi mail', 'Đóng case'";

            List<workItem0> lstAll;

            if (currentUser?.Roles?.ToLower() == "administrator")
            {
                var lstAll0 = await DoTfsFetchData("Edusoft.Net-CS"
                                                , "Edusoft.Net-CS%20Team"
                                                , "'CS Case', 'CS CASE'"
                                                , "" // matruong
                                                , ""
                                                , list_trangthai
                                                , " ([System.CreatedDate] >=" + ngayBatDau + ")"
                                                  + " AND ([System.CreatedDate] <= " + ngayKetThuc + ")"
                                                  + " AND ([AQ.Customer] NOT IN ('RELEASE', 'CST', 'AQ', 'SEMINAR'))"
                                                  + " AND ([System.State] NOT IN ('Đóng case', 'Đã xử lý'))"
                                                  + " AND ([AQ.Priority] NOT IN ('5 - Cần theo dõi'))"
                                                  + " AND ([AQ.CaseType] NOT IN ('ST - Chỉnh định cho khách hàng', 'ZF - Task nội bộ AQ'))"
                                                 , "");
                lstAll = lstAll0;
            }
            else
            {

                var lstAll0 = await DoTfsFetchData("Edusoft.Net-CS"
                                               , "Edusoft.Net-CS%20Team"
                                               , "'CS Case', 'CS CASE'"
                                               , list_matruong
                                               , ""
                                               , list_trangthai
                                               , " ([System.CreatedDate] >=" + ngayBatDau + ")"
                                                 + " AND ([System.CreatedDate] <= " + ngayKetThuc + ")"
                                                 + " AND ([AQ.Customer] NOT IN ('RELEASE', 'CST', 'AQ', 'SEMINAR'))"
                                                 + " AND ([AQ.Priority] NOT IN ('5 - Cần theo dõi'))"
                                                 + " AND ([AQ.CaseType] NOT IN ('ST - Chỉnh định cho khách hàng', 'ZF - Task nội bộ AQ'))"
                                                 , !string.IsNullOrEmpty(model.filter?.macase) ? model.filter?.macase : ""
                                               //, "38696"
                                               );
                lstAll = lstAll0;
            }
            var dayTargetCanAdd = double.Parse(config["targetDateAddDay"]);
            string sRet = "";
            if (lstAll != null && lstAll.Count > 0)
            {
                DataTable dt = new DataTable("tblTfsData");
                dt.Clear();
                dt.Columns.Add("macase", typeof(int));
                dt.Columns.Add("matruong", typeof(string));
                dt.Columns.Add("ngaynhan", typeof(DateTime));
                dt.Columns.Add("chitietyc", typeof(string));
                dt.Columns.Add("trangthai", typeof(string));
                dt.Columns.Add("ngaydukien", typeof(DateTime));
                dt.Columns.Add("loaihopdong", typeof(string));
                dt.Columns.Add("mucdo", typeof(string));
                dt.Columns.Add("hieuluc", typeof(string));
                dt.Columns.Add("dabangiao", typeof(string));
                dt.Columns.Add("ngayemail", typeof(DateTime));
                dt.Columns.Add("mailto", typeof(string));
                dt.Columns.Add("loaicase", typeof(string));
                dt.Columns.Add("phanhe", typeof(string));
                dt.Columns.Add("comment", typeof(string));
                dt.Columns.Add("tinhnangmoi", typeof(bool));

                if (!string.IsNullOrEmpty(model.filter?.macase))
                {
                    dt.Columns.Add("thongtinkh", typeof(string));
                    dt.Columns.Add("dapungcongty", typeof(string));
                }

                foreach (var r in lstAll)
                {
                    DataRow dr = dt.NewRow();
                    bool isMoCase = false;
                    bool fixTrangThai = false;
                    bool fixTrangThai2 = false;
                    bool fixDaBanGiao = false;
                    dr["trangthai"] = "";

                    var z = r.fields;
                    foreach (KeyValuePair<string, string> kvp in r.fields)
                    {

                        if (kvp.Key.ToLower().Equals("system.id"))
                            dr["macase"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.reviewcase"))
                        {
                            dr["tinhnangmoi"] = true;
                        }

                        else if (kvp.Key.ToLower().Equals("aq.customer"))
                            dr["matruong"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("system.createddate"))
                            dr["ngaynhan"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("system.title"))
                            dr["chitietyc"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.priority"))
                        {
                            if (kvp.Value.ToLower().Contains("cần phân tích"))
                                fixTrangThai = true;
                        }
                        else if (kvp.Key.ToLower().Equals("system.state"))
                        {
                            if (kvp.Value.ToLower().Contains("mở case") || kvp.Value.ToLower().Contains("đang xử lý"))
                                isMoCase = true;

                            if (kvp.Value.ToLower().Contains("đóng case"))
                                fixDaBanGiao = true;

                            dr["trangthai"] = kvp.Value;
                        }
                        else if (kvp.Key.ToLower().Equals("aq.targetdate"))
                            dr["ngaydukien"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.contracttype"))
                            dr["loaihopdong"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.prioritytype"))
                            dr["mucdo"] = kvp.Value;

                        else if (kvp.Key.ToLower().Equals("aq.releasedate"))
                            dr["hieuluc"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("microsoft.vsts.common.statechangedate") && fixDaBanGiao == true)
                            dr["ngayemail"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.mailto"))
                            dr["mailto"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.casetype"))
                            dr["loaicase"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.module"))
                            dr["phanhe"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.comment"))
                            dr["comment"] = (string.IsNullOrEmpty(kvp.Value) || kvp.Value == "0" || kvp.Value == "1") ? "" : kvp.Value;

                        else if (kvp.Key.ToLower().Equals("system.assignedto"))
                        {
                            if (kvp.Value.ToLower().Contains("tiepnhan"))
                                fixTrangThai = true;
                            else if (kvp.Value.ToLower().Contains("root"))
                                fixTrangThai2 = true;
                        }
                        else if (!string.IsNullOrEmpty(model.filter?.macase) && kvp.Key.ToLower().Equals("system.description"))
                            dr["thongtinkh"] = kvp.Value;
                        else if (!string.IsNullOrEmpty(model.filter?.macase) && kvp.Key.ToLower().Equals("microsoft.vsts.common.descriptionhtml"))
                            dr["dapungcongty"] = kvp.Value;

                    } // for each fields

                    dr["dabangiao"] = ""; dr["hieuluc"] = "";

                    // FIX 1
                    if (isMoCase)
                    {
                        if (fixTrangThai)
                            dr["trangthai"] = "Đang phân tích";
                        else if (fixTrangThai2)
                            dr["trangthai"] = "Đang chờ phân tích nghiệp vụ phức tạp";
                    }

                    // FIX 2
                    if (fixDaBanGiao) dr["dabangiao"] = "X";


                    // Kiểm tra thời gian ngày dữ kiến đôi với releaseCStTime nếu ngày dự kiến trước releaseCST time thì không thêm ngày
                    if (dr["ngaydukien"] != null)
                    {
                        DateTime d = (DateTime)dr["ngaydukien"];
                        dr["hieuluc"] = calcTuanRelease(d);
                    }
                    bool ngayDuKienCoTruoc = UtilsCscase.IsNgayDuKienCoTruocReleaseCST((DateTime)dr["ngaydukien"]);
                    if (ngayDuKienCoTruoc && dayTargetCanAdd >= 0 && dr["ngaydukien"].ToString() != "")
                    {
                        var targetDate = (DateTime)dr["ngaydukien"];
                        if (dr["mucdo"].ToString().ToLower().Contains("1"))
                        {
                            targetDate = targetDate.AddDays(1);
                            while (!IsWeekDay(targetDate))
                            {
                                targetDate = targetDate.AddDays(1);
                            }
                        }
                        else
                        if (dr["mucdo"].ToString().ToLower().Contains("2"))
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                targetDate = targetDate.AddDays(1);
                                while (!IsWeekDay(targetDate))
                                {
                                    targetDate = targetDate.AddDays(1);
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < dayTargetCanAdd; i++)
                            {
                                /*if (dr["macase"].ToString() == "40818")
                             {
                                 await Console.Out.WriteLineAsync("ss");
                              }*/
                                targetDate = targetDate.AddDays(1);
                                while (!IsWeekDay(targetDate))
                                {
                                    targetDate = targetDate.AddDays(1);
                                }
                            }
                        }
                        dr["ngaydukien"] = targetDate.ToString();
                    }
                    // FIX 4
                    if (dr["trangthai"].ToString().ToLower().Contains("đang phân tích")
                       || dr["trangthai"].ToString().ToLower().Contains("đang chờ phân tích"))
                    {
                        dr["dabangiao"] = "";
                        dr["ngaydukien"] = DBNull.Value;
                        dr["hieuluc"] = "";
                    }

                    if (dr["trangthai"].ToString().ToLower().Contains("đã xử lý"))
                    {
                        dr["trangthai"] = "Đang test";
                    }

                    dt.Rows.Add(dr);
                } // each record

                sRet = JsonConvert.SerializeObject(dt);
                var list1 = JsonConvert.DeserializeObject<List<EduCase>>(sRet);

                // get list truong trong TFS (1)
                var truongtemp = list1.Select(o => new
                {
                    matruong = o.matruong,
                    tentruong = "",
                }).Distinct().OrderBy(s => s.matruong).ToList();

                var listTruong = new List<DataTruong>();

                if (string.IsNullOrEmpty(model.filter?.macase))
                {
                    // list truong trong TFS (1) JOIN file data.dat logon (2) => get ten truong
                    listTruong = clients.Join(truongtemp, a => a.MaTruong, b => b.matruong,
                    (a, b) => new DataTruong
                    {
                        matruong = a.MaTruong,
                        tentruong = a.TenTruong
                    }).Distinct().OrderBy(s => s.matruong).ToList();

                }
                list1.Where(x => !string.IsNullOrEmpty(x.thongtinkh)).ToList().ForEach(x => x.thongtinkh = EmbedHtmlContent(x.thongtinkh));
                list1.Where(x => !string.IsNullOrEmpty(x.dapungcongty)).ToList().ForEach(x => x.dapungcongty = EmbedHtmlContent(x.dapungcongty));
                var datatemp = new CSCaseDataDO
                {
                    is_tfs = true,
                    data_case = list1.ToList(),
                    data_truong = listTruong.Distinct().ToList(),
                };

                return new CSCaseResult { data = datatemp, code = 200, result = true };
            }
            else
            {
                var datatemp = new CSCaseDataDO
                {
                    is_tfs = true,
                    data_case = new List<EduCase>(),
                    data_truong = new List<DataTruong>(),
                };
                return new CSCaseResult { data = datatemp, code = 200, result = true, message = list_matruong };
            }
        }


        [HttpPost, Route("viewcscasenew")]
        public async Task<CSCaseResult> ViewCsCaseNew(CSCaseBindingModel model)
        {
            if (currentUser == null)
            {
                return new CSCaseResult { code = 500, result = false, message = "Not Login" };
            }

            //get data thong tin ma truong ten truong trong file .dat
            var dir = Path.Combine(AppContext.BaseDirectory, "clients.dat");
            var clients = JsonConvert.DeserializeObject<List<EduClient>>(System.IO.File.ReadAllText(dir));

            string list_matruong = "";

            if (currentUser?.Roles?.ToLower().Equals("admin", StringComparison.OrdinalIgnoreCase) == true
            || currentUser?.Roles?.ToLower().Equals("administrator", StringComparison.OrdinalIgnoreCase) == true)
            {
                list_matruong = "";
            }
            else
            {
                var temp = clients.Where(s => s.MaTruong.StartsWith(currentUser.MaTruong)).ToList();
                List<string> empnames = (from e in temp select e.MaTruong).ToList();
                string StringMaTruong = string.Join("','", empnames);
                list_matruong = "'" + StringMaTruong + "'";
            }

            string ngayBatDau = "'01/01/2022'";
            string ngayKetThuc = "'29/12/2024'";
            if (model.dateRange == "22-23")
            {
                ngayBatDau = "'01/01/2022'";
                ngayKetThuc = "'01/01/2023'";
            }
            else if (model.dateRange == "23-24")
            {
                ngayBatDau = "'01/01/2023'";
                ngayKetThuc = "'01/01/2024'";
            }
            else if (model.dateRange == "24-25")
            {
                ngayBatDau = "'01/01/2024'";
                ngayKetThuc = "'01/01/2025'";
            }
            string list_trangthai = ""; // "'Mở case', 'Đang xử lý', 'Đã xử lý', 'Đã gửi mail', 'Đóng case'";

            List<workItem0> lstAll;

            var lstAll0 = await DoTfsFetchData("Edusoft.Net-CS"
                                           , "Edusoft.Net-CS%20Team"
                                           , "'CS Case', 'CS CASE'"
                                           , ""
                                           , ""
                                           , list_trangthai
                                           , " ([System.CreatedDate] >=" + ngayBatDau + ")"
                                             + " AND ([System.CreatedDate] <= " + ngayKetThuc + ")"
                                             + " AND ([AQ.Customer] NOT IN ('RELEASE', 'CST', 'AQ', 'SEMINAR'))"
                                             + " AND ([AQ.Priority] NOT IN ('5 - Cần theo dõi'))"
                                             + " AND ([AQ.CaseType] NOT IN ('ST - Chỉnh định cho khách hàng', 'ZF - Task nội bộ AQ'))"
                                             , !string.IsNullOrEmpty(model.filter?.macase) ? model.filter?.macase : ""
                                           //, "38696"
                                           );
            lstAll = lstAll0;
            var dayTargetCanAdd = double.Parse(config["targetDateAddDay"]);
            string sRet = "";
            if (lstAll != null && lstAll.Count > 0)
            {
                DataTable dt = new DataTable("tblTfsData");
                dt.Clear();
                dt.Columns.Add("macase", typeof(int));
                dt.Columns.Add("matruong", typeof(string));
                dt.Columns.Add("ngaynhan", typeof(DateTime));
                dt.Columns.Add("chitietyc", typeof(string));
                dt.Columns.Add("trangthai", typeof(string));
                dt.Columns.Add("ngaydukien", typeof(DateTime));
                dt.Columns.Add("loaihopdong", typeof(string));
                dt.Columns.Add("mucdo", typeof(string));
                dt.Columns.Add("hieuluc", typeof(string));
                dt.Columns.Add("dabangiao", typeof(string));
                dt.Columns.Add("ngayemail", typeof(DateTime));
                dt.Columns.Add("mailto", typeof(string));
                dt.Columns.Add("loaicase", typeof(string));
                dt.Columns.Add("phanhe", typeof(string));
                dt.Columns.Add("comment", typeof(string));
                dt.Columns.Add("tinhnangmoi", typeof(bool));

                if (!string.IsNullOrEmpty(model.filter?.macase))
                {
                    dt.Columns.Add("thongtinkh", typeof(string));
                    dt.Columns.Add("dapungcongty", typeof(string));
                }

                foreach (var r in lstAll)
                {
                    DataRow dr = dt.NewRow();
                    bool isMoCase = false;
                    bool fixTrangThai = false;
                    bool fixTrangThai2 = false;
                    bool fixDaBanGiao = false;
                    dr["trangthai"] = "";

                    var z = r.fields;
                    foreach (KeyValuePair<string, string> kvp in r.fields)
                    {

                        if (kvp.Key.ToLower().Equals("system.id"))
                            dr["macase"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.reviewcase"))
                        {
                            dr["tinhnangmoi"] = true;
                        }

                        else if (kvp.Key.ToLower().Equals("aq.customer"))
                            dr["matruong"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("system.createddate"))
                            dr["ngaynhan"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("system.title"))
                            dr["chitietyc"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.priority"))
                        {
                            if (kvp.Value.ToLower().Contains("cần phân tích"))
                                fixTrangThai = true;
                        }
                        else if (kvp.Key.ToLower().Equals("system.state"))
                        {
                            if (kvp.Value.ToLower().Contains("mở case") || kvp.Value.ToLower().Contains("đang xử lý"))
                                isMoCase = true;

                            if (kvp.Value.ToLower().Contains("đóng case"))
                                fixDaBanGiao = true;

                            dr["trangthai"] = kvp.Value;
                        }
                        else if (kvp.Key.ToLower().Equals("aq.targetdate"))
                            dr["ngaydukien"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.contracttype"))
                            dr["loaihopdong"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.prioritytype"))
                            dr["mucdo"] = kvp.Value;

                        else if (kvp.Key.ToLower().Equals("aq.releasedate"))
                            dr["hieuluc"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("microsoft.vsts.common.statechangedate") && fixDaBanGiao == true)
                            dr["ngayemail"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.mailto"))
                            dr["mailto"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.casetype"))
                            dr["loaicase"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.module"))
                            dr["phanhe"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.comment"))
                            dr["comment"] = (string.IsNullOrEmpty(kvp.Value) || kvp.Value == "0" || kvp.Value == "1") ? "" : kvp.Value;

                        else if (kvp.Key.ToLower().Equals("system.assignedto"))
                        {
                            if (kvp.Value.ToLower().Contains("tiepnhan"))
                                fixTrangThai = true;
                            else if (kvp.Value.ToLower().Contains("root"))
                                fixTrangThai2 = true;
                        }
                        else if (!string.IsNullOrEmpty(model.filter?.macase) && kvp.Key.ToLower().Equals("system.description"))
                            dr["thongtinkh"] = kvp.Value;
                        else if (!string.IsNullOrEmpty(model.filter?.macase) && kvp.Key.ToLower().Equals("microsoft.vsts.common.descriptionhtml"))
                            dr["dapungcongty"] = kvp.Value;

                    } // for each fields

                    dr["dabangiao"] = ""; dr["hieuluc"] = "";

                    // FIX 1
                    if (isMoCase)
                    {
                        if (fixTrangThai)
                            dr["trangthai"] = "Đang phân tích";
                        else if (fixTrangThai2)
                            dr["trangthai"] = "Đang chờ phân tích nghiệp vụ phức tạp";
                    }

                    // FIX 2
                    if (fixDaBanGiao) dr["dabangiao"] = "X";


                    // Kiểm tra thời gian ngày dữ kiến đôi với releaseCStTime nếu ngày dự kiến trước releaseCST time thì không thêm ngày
                    if (dr["ngaydukien"] != null)
                    {
                        DateTime d = (DateTime)dr["ngaydukien"];
                        dr["hieuluc"] = calcTuanRelease(d);
                    }
                    bool ngayDuKienCoTruoc = UtilsCscase.IsNgayDuKienCoTruocReleaseCST((DateTime)dr["ngaydukien"]);
                    if (ngayDuKienCoTruoc && dayTargetCanAdd >= 0 && dr["ngaydukien"].ToString() != "")
                    {
                        var targetDate = (DateTime)dr["ngaydukien"];
                        if (dr["mucdo"].ToString().ToLower().Contains("1"))
                        {
                            targetDate = targetDate.AddDays(1);
                            while (!IsWeekDay(targetDate))
                            {
                                targetDate = targetDate.AddDays(1);
                            }
                        }
                        else
                        if (dr["mucdo"].ToString().ToLower().Contains("2"))
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                targetDate = targetDate.AddDays(1);
                                while (!IsWeekDay(targetDate))
                                {
                                    targetDate = targetDate.AddDays(1);
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < dayTargetCanAdd; i++)
                            {
                                /*if (dr["macase"].ToString() == "40818")
                             {
                                 await Console.Out.WriteLineAsync("ss");
                              }*/
                                targetDate = targetDate.AddDays(1);
                                while (!IsWeekDay(targetDate))
                                {
                                    targetDate = targetDate.AddDays(1);
                                }
                            }
                        }
                        dr["ngaydukien"] = targetDate.ToString();
                    }
                    // FIX 4
                    if (dr["trangthai"].ToString().ToLower().Contains("đang phân tích")
                       || dr["trangthai"].ToString().ToLower().Contains("đang chờ phân tích"))
                    {
                        dr["dabangiao"] = "";
                        dr["ngaydukien"] = DBNull.Value;
                        dr["hieuluc"] = "";
                    }

                    if (dr["trangthai"].ToString().ToLower().Contains("đã xử lý"))
                    {
                        dr["trangthai"] = "Đang test";
                    }

                    dt.Rows.Add(dr);
                } // each record

                sRet = JsonConvert.SerializeObject(dt);
                var list1 = JsonConvert.DeserializeObject<List<EduCase>>(sRet);

                // get list truong trong TFS (1)
                var truongtemp = list1.Select(o => new
                {
                    matruong = o.matruong,
                    tentruong = "",
                }).Distinct().OrderBy(s => s.matruong).ToList();

                var listTruong = new List<DataTruong>();

                if (string.IsNullOrEmpty(model.filter?.macase))
                {
                    // list truong trong TFS (1) JOIN file data.dat logon (2) => get ten truong
                    listTruong = clients.Join(truongtemp, a => a.MaTruong, b => b.matruong,
                    (a, b) => new DataTruong
                    {
                        matruong = a.MaTruong,
                        tentruong = a.TenTruong
                    }).Distinct().OrderBy(s => s.matruong).ToList();

                }
                list1.Where(x => !string.IsNullOrEmpty(x.thongtinkh)).ToList().ForEach(x => x.thongtinkh = EmbedHtmlContent(x.thongtinkh));
                list1.Where(x => !string.IsNullOrEmpty(x.dapungcongty)).ToList().ForEach(x => x.dapungcongty = EmbedHtmlContent(x.dapungcongty));
                var datatemp = new CSCaseDataDO
                {
                    is_tfs = true,
                    data_case = list1.ToList(),
                    data_truong = listTruong.Distinct().ToList(),
                };

                return new CSCaseResult { data = datatemp, code = 200, result = true };
            }
            else
            {
                var datatemp = new CSCaseDataDO
                {
                    is_tfs = true,
                    data_case = new List<EduCase>(),
                    data_truong = new List<DataTruong>(),
                };
                return new CSCaseResult { data = datatemp, code = 200, result = true, message = list_matruong };
            }
        }

        private static bool IsWeekDay(DateTime date)
        {
            DayOfWeek day = date.DayOfWeek;
            return day != DayOfWeek.Saturday && day != DayOfWeek.Sunday;
        }
        string EmbedHtmlContent(string htmlContent)
        {
            HtmlDocument doc = new();
            doc.LoadHtml(htmlContent);

            var images = doc.DocumentNode.SelectNodes("//img");
            if (images != null)
            {
                foreach (var image in images)
                {
                    //need to understand if it is in base 64 or no, if the answer is no, we need to embed image
                    var src = image.GetAttributeValue("src", "");
                    if (!String.IsNullOrEmpty(src))
                    {
                        if (src.Contains("base64")) // data:image/jpeg;base64,
                        {
                            continue;
                        }
                        else
                        {
                            var acc = config["tfsAcc"].Split(":");

                            try
                            {
                                var data = Task.Run(async () => await TfsClient.LoadImage(src, acc[0], acc[1])).Result;
                                var newSrcValue = $"data:image/jpeg;base64,{Convert.ToBase64String(data)}";
                                image.SetAttributeValue("src", newSrcValue);
                            }
                            catch (Exception) { }

                        }
                    }
                }
                return doc.DocumentNode.OuterHtml;
            }
            return htmlContent;
        }

        [HttpPost, Route("createcase")]
        public object CreateCase(dynamic model)
        {
            if (currentUser == null)
            {
                return new CSCaseResult { code = 500, result = false, message = "Not Login" };
            }

            List<workItem0> lstAll;
            var lstAll0 = DoTfsFetchData_CreateCase(string.Join("", model));
            lstAll = lstAll0.Result;

            if (lstAll != null && lstAll.Count > 0)
            {
                var datatemp = new CSCaseDataDO
                {
                    is_tfs = true,
                    newcase = lstAll
                };
                return new CSCaseResult { data = datatemp, code = 200, result = true };
            }
            else
            {
                return new CSCaseResult { data = null, code = 400, result = false };
            }
        }

        [HttpPost, Route("updatecase")] //update link đính kèm vào case
        public object UpdateCase(CSCaseBindingModel model)
        {
            if (currentUser == null)
            {
                return new CSCaseResult { code = 500, result = false, message = "Not Login" };
            }

            var tmp = "";
            if (!string.IsNullOrEmpty(model.filter?.body_update)) // update trao doi
            {
                tmp = model.filter?.body_update;
            }
            else
            {
                var baseUrl = $"{Request.Scheme}://{Request.Host}/";
                if (Request.PathBase != null)
                    baseUrl += $"{Request.PathBase}/";
                foreach (var s in model.filter?.file) // update link file
                {
                    var url = baseUrl + "attachment/" + s?.filename;
                    tmp += "{ \"op\": \"add\", \"path\": \"/relations/-\", \"value\": { \"rel\": \"Hyperlink\",\"url\":\"" + url + "\"}},";
                }
                if (!string.IsNullOrEmpty(tmp))
                {
                    tmp = "[" + tmp + "]";
                }
            }

            List<workItem0> lstAll;
            var lstAll0 = DoTfsFetchData_UpdateCase(Int32.Parse(model.filter?.macase), tmp);
            lstAll = lstAll0.Result;

            if (lstAll != null && lstAll.Count > 0)
            {
                var datatemp = new CSCaseDataDO
                {
                    is_tfs = true,
                    newcase = lstAll
                };
                return new CSCaseResult { data = datatemp, code = 200, result = true };
            }
            else
            {
                return new CSCaseResult { data = null, code = 400, result = false };
            }
        }

        [HttpPost, Route("upload_dinhkemfile")]
        public object Upload_DinhKemFile(CSCaseBindingModel model)
        {
            if (currentUser == null)
            {
                return new CSCaseResult { code = 500, result = false, message = "Not Login" };
            }
            if (string.IsNullOrEmpty(model.filter?.filename))
            {
                return new CSCaseResult { code = 500, result = false, message = "Not File Name" };
            }

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"wwwroot", "attachment");

            bool folderExists = Directory.Exists(path);
            if (!folderExists)
                Directory.CreateDirectory(path);

            var dir1 = Path.Combine(path, model.filter?.filename);

            string im = model.filter?.base64;
            try
            {
                System.IO.File.WriteAllBytes(dir1, Convert.FromBase64String(im.Substring(im.LastIndexOf(',') + 1)));
                return new CSCaseResult { code = 200, result = true };
            }
            catch
            {
                return new CSCaseResult { data = null, code = 400, result = false };
            }
        }

        ////////////////////////////////     RELEASE     //////////////////////////////////////////////////////////////////////
        async Task<List<workItem0>> DoTfsFetchData_RL(string pProject, string pProjectTeam, string pWIType, string pAppendWHERE)
        {
            string sWIQL = "{\"query\": \"SELECT [System.Id] FROM WorkItems"
                                       + " WHERE ([System.TeamProject]='" + pProject + "')"
                                           + (pWIType.Length > 0 ? " AND ([System.WorkItemType] IN (" + pWIType + "))" : "")
                                           //+ " AND [System.State] IN ('Đã xử lý') AND [AQ.TestState] IN ('01 - Đã kiểm tra và kết quả đúng')"
                                           //+ " AND [System.State] IN ('Đã xử lý')"
                                           + (pAppendWHERE.Length > 0 ? " AND (" + pAppendWHERE + ")" : "");

            sWIQL += " ORDER BY [System.CreatedDate] " + "\"}";

            var jsonCasesId = await DoTfsQueryData(TFS_HOST
                                            , "tfs/aq/" + pProject + "/" + pProjectTeam + "/_apis/wit/wiql?api-version=4.1"
                                            , "", sWIQL, TFS_TOKEN_BASE64);

            if (string.IsNullOrEmpty(jsonCasesId))
                return new List<workItem0>();

            var lstCasesId = JsonConvert.DeserializeObject<TfsCaseListModel>(jsonCasesId);
            if (lstCasesId.workItems.Count == 0)
                return new List<workItem0>();

            int MAX_CASES = 32;
            int i = 0;

            List<workItem0> lstReturn = new List<workItem0>();

            var lst = lstCasesId.workItems;

            while (lst.Any())
            {
                i++;
                string sCaseList = "";
                foreach (var s in lst.Take(MAX_CASES).ToList())
                    sCaseList += (sCaseList.Length > 0 ? "," : "") + s.id;

                string sTfsFieldList = "System.Id,AQ.Customer,System.CreatedDate,System.Title,AQ.TargetDate,AQ.CaseType,AQ.Module,AQ.Solution,AQ.TestState,AQ.ReviewCase";
                var tmpjson = await DoTfsQueryData(TFS_HOST
                                            , "tfs/aq/" + pProject + "/_apis/wit/workitems?ids=" + sCaseList
                                            , "&fields=" + sTfsFieldList
                                            , ""
                                            , TFS_TOKEN_BASE64);

                var tmp = JsonConvert.DeserializeObject<TfsCaseDetailModel>(tmpjson);
                if (tmp != null)
                    lstReturn.AddRange(tmp.value);

                lst = lst.Skip(MAX_CASES).ToList();
            }

            return lstReturn;
        }

        [HttpPost, Route("cscase_rl")]
        public CSCaseResult ViewCsCase_rl(CSCaseBindingModel model)
        {
            if (currentUser == null)
            {
                return new CSCaseResult { code = 500, result = false, message = "Not Login" };
            }

            List<workItem0> lstAll;
            var lstAll0 = DoTfsFetchData_RL("Edusoft.Net-CS"
                                               , "Edusoft.Net-CS%20Team"
                                               , "'CS Case', 'CS CASE'"
                                               , " [AQ.UserGuideRequested] IN ('" + model.filter?.version + "') "
                                              //+ " AND ([System.State] IN ('Đã xử lý')) "
                                              //+ " AND ([AQ.TestState] IN ('01 - Đã kiểm tra và kết quả đúng'))"
                                              );

            lstAll = lstAll0.Result;
            string sRet = "";
            if (lstAll != null && lstAll.Count > 0)
            {
                DataTable dt = new DataTable("tblTfsData");
                dt.Clear();
                dt.Columns.Add("macase", typeof(int));
                dt.Columns.Add("matruong", typeof(string));
                dt.Columns.Add("ngaynhan", typeof(DateTime));
                dt.Columns.Add("version", typeof(string));
                dt.Columns.Add("chitietyc", typeof(string));
                dt.Columns.Add("trangthai", typeof(string));
                dt.Columns.Add("ngaydukien", typeof(DateTime));
                dt.Columns.Add("loaicase", typeof(string));
                dt.Columns.Add("phanhe", typeof(string));
                dt.Columns.Add("comment", typeof(string));

                dt.Columns.Add("whatnew", typeof(string));
                dt.Columns.Add("teststate", typeof(string));

                dt.Columns.Add("reviewcase", typeof(string));

                foreach (var r in lstAll)
                {
                    Console.WriteLine(r.fields);
                    DataRow dr = dt.NewRow();
                    foreach (KeyValuePair<string, string> kvp in r.fields)
                    {
                        if (kvp.Key.ToLower().Equals("system.id"))
                            dr["macase"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.customer"))
                            dr["matruong"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("system.createddate"))
                            dr["ngaynhan"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.userguiderequested"))
                            dr["version"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("system.title"))
                            dr["chitietyc"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("system.state"))
                            dr["trangthai"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.targetdate"))
                            dr["ngaydukien"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.casetype"))
                            dr["loaicase"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.module"))
                            dr["phanhe"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.comment"))
                            dr["comment"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.solution"))
                            dr["whatnew"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.teststate"))
                            dr["teststate"] = kvp.Value;
                        else if (kvp.Key.ToLower().Equals("aq.reviewcase"))
                            dr["reviewcase"] = kvp.Value;
                    } // for each fields

                    dt.Rows.Add(dr);
                } // each record

                sRet = JsonConvert.SerializeObject(dt);
                var list1 = JsonConvert.DeserializeObject<List<EduCase>>(sRet);

                var datatemp = new CSCaseDataDO
                {
                    is_tfs = true,
                    data_case = list1.ToList(),
                };

                return new CSCaseResult { data = datatemp, code = 200, result = true };
            }
            else
            {
                var datatemp = new CSCaseDataDO
                {
                    is_tfs = true,
                    data_case = new List<EduCase>(),
                };
                return new CSCaseResult { data = datatemp, code = 200, result = true, message = "" };
            }
        }

        string calcTuanRelease(DateTime dtTargetDate)
        {
            int DAYW_COMMIT = 5; // Thu Nam
            int DAYW_BUILD = 3; // Thu Ba

            DateTime t0 = dtTargetDate; // new DateTime(Convert.ToInt32(txtYear.Text.Trim()), Convert.ToInt32(txtMonth.Text.Trim()), Convert.ToInt32(txtDay.Text.Trim()));
            int i = (int)t0.DayOfWeek + 1; // C# 0 = CN / Excel 1 = CN ;
            DateTime tR = t0.AddDays((i <= DAYW_COMMIT ? (7 + DAYW_BUILD - i) : (7 + DAYW_BUILD - i) + 7));   // =F8 + IF(WEEKDAY(F8,1)<=5, 10-WEEKDAY(F8,1), 10-WEEKDAY(F8,1)+7)

            // = WEEKNUM(I38)-WEEKNUM( DATE(YEAR(I38),MONTH(I38),1) )+1   - IF(WEEKDAY( DATE(YEAR(I38),MONTH(I38),1) )=1, 1, 0)

            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;

            int nw1 = cal.GetWeekOfYear(tR, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);
            DateTime t1 = new DateTime(tR.Year, tR.Month, 1);
            int nw2 = cal.GetWeekOfYear(t1, CalendarWeekRule.FirstDay, DayOfWeek.Sunday);

            int wRL = nw1 - nw2 + (((int)t1.DayOfWeek + 1) == 1 ? 0 : 1);

            return "RL tuần thứ " + wRL.ToString() + " tháng " + tR.Month.ToString() + "/" + tR.Year.ToString();
        }

        /// sqlserver: convert(bigint, convert(varbinary, concat('0x', replace(substring(convert(varchar(36), @pGuid ), 20, 17), '-', '')), 1) )
        public static BigID UIDToBig(Guid id)
        {
            var arr = id.ToByteArray();
            Array.Reverse(arr);
            var ff = BitConverter.ToInt64(arr, 0);
            if (ff != 0) //truong hop guid full
                return ff;
            return BitConverter.ToInt64(arr, 8);
        }

        ////////////////////////////////////// Release notes //////////////////////
        [HttpPost, Route("createRelease")]
        public object GetSampleTable(CSCaseBindingModel data) // data web
        {
            var tb = database.Table<DataRelease>();
            var z = data;
            var version = data.filter.version;
            var row = tb.Find(x => x.version == version);

            if (row != null)
            {
                tb.DeleteMany(x => x.version == version);
            }

            foreach (var r in data.filter.dsrl)
            {
                tb.Insert(new DataRelease
                {
                    macase = r.macase,
                    version = r.version,
                    vesion = r.vesion,
                    version_rl = data.filter.version_rl,
                    loaicase = r.loaicase,
                    matruong = r.matruong,
                    phanhe = r.phanhe,
                    chitietyc = r.chitietyc,
                    //ngaydukien = r.ngaydukien,
                    whatnew = r.whatnew,
                    reviewcase = r.reviewcase
                });
            }
            var ret = tb.FindAll();
            return ret;
        }

        [HttpPost, Route("update_case_rl")]
        public object Update_Case_Rl(CSCaseBindingModel data) // data web
        {
            var tb = database.Table<DataRelease>();
            var version = data.filter.version;
            var row = tb.Find(x => x.version == version);
            foreach (var r in row)
            {
                r.vesion = data.filter.version;
                tb.Update(r);
            }
            var ret1 = tb.FindAll();
            return ret1;
        }

        [HttpPost, Route("delete_case_rl")]
        public object Delete_Case_Rl(CSCaseBindingModel data) // data web
        {
            var tb = database.Table<DataRelease>();
            var version = data.filter.version;
            var vesion = data.filter.vesion;
            var row = tb.Find(x => x.version == version && x.vesion == vesion);

            if (row != null)
            {
                tb.DeleteMany(x => x.version == version && x.vesion == vesion);
            }
            var ret = tb.FindAll();
            return ret;
        }

        [HttpPost, Route("view_khao_sat")]
        public object ViewKhaoSat()
        {
            var tb = database.Table<KhaoSat>();
            var firstItem = tb.FindOne(Query.All());
            if (firstItem == null)
            {
                tb.Insert(new KhaoSat { linkKhaoSat = "https://example.com", noidung = "Link khảo sát", ngayBatDau = DateTime.Now, ngayKetThuc = DateTime.Now.AddDays(1) });
            }
            return firstItem;
        }

        [HttpPost, Route("update_khao_sat")]
        public object UpdateKhaoSat(KhaoSat khaoSatRequest)
        {
            // Lấy bảng SettingModel từ cơ sở dữ liệu
            var tb = database.Table<KhaoSat>();

            // Tìm bản ghi có tên "khaosat"
            var firstItem = tb.FindOne(Query.All());


            firstItem.noidung = khaoSatRequest.noidung;
            firstItem.linkKhaoSat = khaoSatRequest.linkKhaoSat;
            firstItem.ngayBatDau = khaoSatRequest.ngayBatDau;
            firstItem.ngayKetThuc = khaoSatRequest.ngayKetThuc;
            tb.Update(firstItem);

            // Trả về bản ghi đã được cập nhật hoặc thêm mới
            return firstItem;
        }

        [HttpPost, Route("ds_case_rl")]
        public object ViewCsCase_rl()
        {
            var tb = database.Table<DataRelease>();
            var ret = tb.FindAll();
            return ret;
        }
        [HttpPost, Route("danh_sach_truong_khao_sat")]
        public object ViewDanhSachTruongKhaoSat()
        {
            var tb = database.Table<DanhSachTruongKhaoSat>();
            var ret = tb.FindAll();
            return ret;
        }
        [HttpPost, Route("them_truong_da_khao_sat")]
        public object ThemTruongKhaoSat(DanhSachTruongKhaoSat danhSachTruongKhaoSat)
        {
            var tb = database.Table<DanhSachTruongKhaoSat>();
            tb.Insert(new DanhSachTruongKhaoSat
            {
                tenTruong = danhSachTruongKhaoSat.tenTruong
            });
            var ret = tb.FindAll();
            return ret;
        }



        // public async Task<DataTable?> MSSQL_GetData(string pSQL)
        // {
        //     string connStr = m_connStr;

        //     string commandText = pSQL;

        //     using (var connection = new SqlConnection(connStr))
        //     {
        //         await connection.OpenAsync();   //vs  connection.Open();
        //         using (var tran = connection.BeginTransaction())
        //         {
        //             using (var cmd = new SqlCommand(commandText, connection, tran))
        //             {
        //                 try
        //                 {
        //                     // vs also alternatives, command.ExecuteReader();  or await command.ExecuteNonQueryAsync();
        //                     SqlDataReader rdr = await cmd.ExecuteReaderAsync();
        //                     // await command.ExecuteNonQueryAsync();
        //                     var dt = new DataTable();
        //                     dt.Load(rdr);

        //                     // while (rdr.Read()) {}

        //                     await rdr.CloseAsync();

        //                     return dt;
        //                 }
        //                 catch (Exception Ex)
        //                 {
        //                     await connection.CloseAsync();
        //                     string msg = Ex.Message.ToString();
        //                     tran.Rollback();

        //                     return null;
        //                     // throw;
        //                 }
        //             }
        //         }
        //     }

        //     return null;
        // }

        // [HttpGet, Route("kb-loadtree-faq")]
        // public async Task<object> KB_LoadTree_FAQ()
        // {
        //     return await KB_LoadTree("'FAQ'");
        // }

        // [HttpGet, Route("kb-loadtree-guide")]
        // public async Task<object> KB_LoadTree_Guide()
        // {
        //     return await KB_LoadTree("'USER-GUIDE', 'QUICK-GUIDE'");
        // }

        // [HttpPost, Route("kb-loadtree")]
        // public async Task<object> KB_LoadTree(params string[] p)
        // {
        //     string s_list = "";
        //     foreach (string s in p)
        //     {
        //         s_list = s_list + (!string.IsNullOrEmpty(s_list) ? ", " : "") + "'" + s + "'";
        //     }

        //     return await KB_LoadTree(s_list);
        // }

        // async Task<object> KB_LoadTree(string pList_MaLoaiTaiLieu)
        // {
        //     string sSQL = @"SELECT t1.IDSanPham, sp.MaSanPham, sp.TenSanPham,
        //                        t1.IDPhanHe, ph.MaPhanHe, ph.TenPhanHe,
        //                        t1.IDChucNang, cn.MaChucNang, cn.TenChucNang,
        //                        loai_tl.MaLoaiTaiLieu, loai_tl.TenLoaiTaiLieu,
        //                        loai_nd.MaLoaiNoiDung, loai_nd.TenLoaiNoiDung,
        //                        t1.IDTaiLieu, t1.MaTaiLieu, t1.TenTaiLieu, t1.TtTaiLieu
        //                     FROM [KB-TaiLieu] t1
        //                         LEFT JOIN [KB-DMSanPham] sp ON t1.IDSanPham=sp.IDSanPham
        //                         LEFT JOIN [KB-DMPhanHe] ph ON t1.IDPhanHe=ph.IDPhanHe
        //                         LEFT JOIN [KB-DMChucNang] cn ON t1.IDChucNang=cn.IDChucNang
        //                         LEFT JOIN [KB-DMLoaiTaiLieu] loai_tl ON t1.IDLoaiTaiLieu=loai_tl.IDLoaiTaiLieu
        //                         LEFT JOIN [KB-DMLoaiNoiDung] loai_nd ON t1.IDLoaiNoiDung=loai_nd.IDLoaiNoiDung"
        //                 + (!string.IsNullOrEmpty(pList_MaLoaiTaiLieu) ? " WHERE (loai_tl.MaLoaiTaiLieu IN (" + pList_MaLoaiTaiLieu + "))" : "");

        //     var dt = await MSSQL_GetData(sSQL);
        //     // return JsonConvert.SerializeObject(dt );

        //     var lstTree = from l1 in dt.AsEnumerable()
        //                   group l1 by new { MaSP = l1.Field<string>("MaSanPham"), TenSP = l1.Field<string>("TenSanPham") } into group1
        //                   select new
        //                   {
        //                       Level = 1,
        //                       Ma = group1.Key.MaSP,
        //                       Ten = group1.Key.TenSP,
        //                       childs = from l2 in group1
        //                                group l2 by new { MaPH = l2.Field<string>("MaPhanHe"), TenPH = l2.Field<string>("TenPhanHe") } into group2
        //                                select new
        //                                {
        //                                    Level = 2,
        //                                    Ma = group2.Key.MaPH,
        //                                    Ten = group2.Key.TenPH,
        //                                    childs = from l3 in group2
        //                                             group l3 by new { MaCN = l3.Field<string>("MaChucNang"), TenCN = l3.Field<string>("TenChucNang") } into group3
        //                                             select new
        //                                             {
        //                                                 Level = 3,
        //                                                 Ma = group3.Key.MaCN,
        //                                                 Ten = group3.Key.TenCN,
        //                                                 childs = from l4 in group3
        //                                                          select new
        //                                                          {
        //                                                              Level = 4,
        //                                                              LoaiTL = l4.Field<string>("MaLoaiTaiLieu"),
        //                                                              MaTL = l4.Field<string>("MaTaiLieu"),
        //                                                              TenTL = l4.Field<string>("TenTaiLieu"),
        //                                                              //NoiDungTL = l4.Field<string>("NoiDungTaiLieu")
        //                                                          }
        //                                             }
        //                                }
        //                   };

        //     return JsonConvert.SerializeObject(lstTree); // "Test OK";

        // } // Test3()


        // [HttpPost, Route("kb-loadtailieu")]
        // public async Task<KB_LoadTaiLieuResult> KB_LoadTaiLieu(params string[] pIDTaiLieu)
        // {
        //     string slist_MaTaiLieu = "";
        //     foreach (string s in pIDTaiLieu)
        //     {
        //         slist_MaTaiLieu = slist_MaTaiLieu + (!string.IsNullOrEmpty(slist_MaTaiLieu) ? ", " : "") + "'" + s + "'";
        //     }

        //     string sSQL = @"SELECT t1.IDSanPham, sp.MaSanPham, sp.TenSanPham,
        //                        t1.IDPhanHe, ph.MaPhanHe, ph.TenPhanHe,
        //                        t1.IDChucNang, cn.MaChucNang, cn.TenChucNang,
        //                        loai_tl.IDLoaiTaiLieu, loai_tl.MaLoaiTaiLieu, loai_tl.TenLoaiTaiLieu,
        //                        loai_nd.IDLoaiNoiDung, loai_nd.MaLoaiNoiDung, loai_nd.TenLoaiNoiDung,
        //                        t1.IDTaiLieu, t1.MaTaiLieu, t1.TenTaiLieu, t1.TtTaiLieu, t1.TomTatTaiLieu, t1.NoiDungTaiLieu, t1.NoiDungTaiLieu1, t1.NoiDungTaiLieu2
        //                     FROM [KB-TaiLieu] t1
        //                         LEFT JOIN [KB-DMSanPham] sp ON t1.IDSanPham=sp.IDSanPham
        //                         LEFT JOIN [KB-DMPhanHe] ph ON t1.IDPhanHe=ph.IDPhanHe
        //                         LEFT JOIN [KB-DMChucNang] cn ON t1.IDChucNang=cn.IDChucNang
        //                         LEFT JOIN [KB-DMLoaiTaiLieu] loai_tl ON t1.IDLoaiTaiLieu=loai_tl.IDLoaiTaiLieu
        //                         LEFT JOIN [KB-DMLoaiNoiDung] loai_nd ON t1.IDLoaiNoiDung=loai_nd.IDLoaiNoiDung"
        //                     + (!string.IsNullOrEmpty(slist_MaTaiLieu) ? " WHERE (t1.IDTaiLieu IN (" + slist_MaTaiLieu + "))" : "");

        //     var dt = await MSSQL_GetData(sSQL);

        //     var lst_ret = from lst in dt.AsEnumerable()
        //                   //where lst.Field<string>("id_qr_code").Trim() == qr.id_qr_code.Trim()
        //                   select new KB_LoadTaiLieuDO()
        //                   {
        //                       id_san_pham = lst.Field<BigID>("IDSanPham"),
        //                       ma_san_pham = lst.Field<string>("MaSanPham"),
        //                       ten_san_pham = lst.Field<string>("TenSanPham"),

        //                       id_phan_he = lst.Field<BigID>("IDPhanHe"),
        //                       ma_phan_he = lst.Field<string>("MaPhanHe"),
        //                       ten_phan_he = lst.Field<string>("TenPhanHe"),

        //                       id_chuc_nang = lst.Field<BigID>("IDChucNang"),
        //                       ma_chuc_nang = lst.Field<string>("MaChucNang"),
        //                       ten_chuc_nang = lst.Field<string>("TenChucNang"),

        //                       id_loai_tai_lieu = lst.Field<BigID>("IDLoaiTaiLieu"),
        //                       ma_loai_tai_lieu = lst.Field<string>("MaLoaiTaiLieu"),
        //                       ten_loai_tai_lieu = lst.Field<string>("TenLoaiTaiLieu"),

        //                       id_loai_noi_dung = lst.Field<BigID>("IDLoaiNoiDung"),
        //                       ma_loai_noi_dung = lst.Field<string>("MaLoaiNoiDung"),
        //                       ten_loai_noi_dung = lst.Field<string>("TenLoaiNoiDung"),

        //                       id_tai_lieu = lst.Field<BigID>("IDTaiLieu"),
        //                       ma_tai_lieu = lst.Field<string>("MaTaiLieu"),
        //                       ten_tai_lieu = lst.Field<string>("TenTaiLieu"),

        //                       tt_tai_lieu = lst.Field<int>("TtTaiLieu"),
        //                       tom_tat_tai_lieu = lst.Field<string>("TomTatTaiLieu"),
        //                       noi_dung_tai_lieu = lst.Field<string>("NoiDungTaiLieu"),
        //                       noi_dung_tai_lieu1 = lst.Field<string>("NoiDungTaiLieu1"),
        //                       noi_dung_tai_lieu2 = lst.Field<string>("NoiDungTaiLieu2"),
        //                   };

        //     return new KB_LoadTaiLieuResult
        //     {
        //         result = true,
        //         code = 200,
        //         data = lst_ret.ToList()
        //     };

        //     //return JsonConvert.SerializeObject(lst_ret);
        // }

    }

    // MainController class //

}

public class TfsCaseDetailModel
{
    public int count { get; set; }
    public List<workItem0> value { get; set; }

}
public class CSCaseBindingModel
{
    public CSCaseFilterDO filter { get; set; }
    public string dateRange { get; set; }
}

public class SettingRequest
{
    public string name { get; set; }
    public string value { get; set; }
}
class SettingModel
{
    [BsonId]
    public int id { get; set; }
    public string name { get; set; }
    public string value { get; set; }
}

public class KhaoSat
{
    [BsonId]
    public int id { get; set; }
    public string noidung { get; set; }
    public string linkKhaoSat { get; set; }
    public DateTime ngayBatDau { get; set; }
    public DateTime ngayKetThuc { get; set; }
}
public class DanhSachTruongKhaoSat
{
    [BsonId]
    public int id { get; set; }
    public string tenTruong { get; set; }
}

class TfsCaseModel
{
    public int id { get; set; }
    public string url { get; set; }
    public int rev { get; set; }
    public Dictionary<string, string> fields { get; set; }

}

public class workItem0
{
    public int id { get; set; }
    public string url { get; set; }
    public int rev { get; set; }
    public Dictionary<string, string> fields { get; set; }
}

public class workItem01
{
    public string id { get; set; }
    public string url { get; set; }
    public int rev { get; set; }
    public Dictionary<string, string> fields { get; set; }
}

class TfsCaseListModel
{
    public string queryType { get; set; }

    public string queryResultType { get; set; }

    public DateTime asOf { get; set; }

    public List<workItemBase> workItems { get; set; }
}

public class workItemBase
{
    public int id { get; set; }
    public string url { get; set; }
}

public class LoginModel
{
    public string username { get; set; }
    public string password { get; set; }
}

public class ResultModel
{
    public string error { get; set; }
    public object data { get; set; }
}

public class EduClient
{
    public string MaTruong { get; set; }
    public string TenTruong { get; set; }
    public string Pass { get; set; }
    public string Roles { get; set; }
    public List<string> Group { get; set; } = new List<string>();
    public string User { get; set; }
    public object userData { get; set; }
}

public class EduCase
{
    public string macase { get; set; }          //  System.Id
    public string matruong { get; set; }        //  AQ.Customer
    public string ngaynhan { get; set; }        //  System.CreatedDate
    public string chitietyc { get; set; }       //  System.Title
    public string trangthai { get; set; }       //  System.State
    public string ngaydukien { get; set; }      //  AQ.TargetDate
    public string loaihopdong { get; set; }      //  AQ.ContractType
    public string mucdo { get; set; }       //  AQ.PriorityType
    public string version { get; set; }      // AQ.UserGuideRequested

    public string hieuluc { get; set; }         //  AQ.ReleaseDate
    public string dabangiao { get; set; }       //
    public string ngayemail { get; set; }       //  AQ.MailSentDate
    public string mailto { get; set; }          //  AQ.MailTo
    public string loaicase { get; set; }        //  AQ.CaseType
    public string phanhe { get; set; }          //  AQ.Module
    public string whatnew { get; set; }         //  AQ.Solution
    public string teststate { get; set; }        //  AQ.TestState
    public string thongtinkh { get; set; }      //  System.Description
    public string dapungcongty { get; set; }    //  microsoft.vsts.common.descriptionhtml
    public string comment { get; set; }         // Comment
    public string assignedto { get; set; }
    public string reviewcase { get; set; }         // Comment       // Comment
}

public class DataTruong
{
    public string matruong { get; set; }
    public string tentruong { get; set; }
}

public class ApiResultBaseDO
{

    public bool result { get; set; }

    /// <summary>
    /// Theo [HTTP response status codes]
    /// </summary>
    public int code { get; set; }

    /// <summary>
    /// Thông báo trạng thái HOẶC nội dung exception (nếu lỗi)
    /// </summary>
    public string message { get; set; }

    public List<string> validated_message { get; set; }
}

public class CSCaseResult : ApiResultBaseDO
{
    public CSCaseDataDO data { get; set; }
}

public class CSCaseDataDO
{
    public Boolean is_tfs { get; set; }
    public List<EduCase> data_case { get; set; }
    public List<DataTruong> data_truong { get; set; }
    public object newcase { get; set; }
    public object onecase { get; set; }
    public object dinhkem { get; set; }
    public object linkfile { get; set; }

}


public class CSCaseFilterDO
{
    public List<CSCaseDataRelease> dsrl { get; set; }
    public string ngaychot { get; set; }
    public string macase { get; set; }

    public string body_update { get; set; }
    public string version { get; set; }
    public string vesion { get; set; }
    public string version_rl { get; set; }

    public string filename { get; set; }

    public List<FileIf> file { get; set; }

    public string base64 { get; set; }

}

public class FileIf
{
    public string filename { get; set; }
}

public class CSCaseDataRelease
{
    public int id { get; set; }
    public string ngaychot { get; set; }
    public string macase { get; set; }
    public string vesion { get; set; }
    public string version { get; set; }
    public string version_rl { get; set; }
    public string loaicase { get; set; }
    public string matruong { get; set; }
    public string phanhe { get; set; }

    public string chitietyc { get; set; }
    public string ngaydukien { get; set; }
    public string whatnew { get; set; }
    public string reviewcase { get; set; }
}

// public class stringAdditionalDO
// {
//   public PagingDO paging { get; set; }

//   public List<OrderingDO> ordering { get; set; }
// }

// public class PagingDO
// {
//   public int limit { get; set; }

//   public int page { get; set; }
// }

// public class OrderingDO
// {
//   public string name { get; set; }

//   /// <summary>
//   /// 0 asc; 1 desc
//   /// </summary>
//   public int order_type { get; set; }
// }

// internal class NewClass
// {
//   public string Matruong { get; }
//   public string Tentruong { get; }

//   public NewClass(string matruong, string tentruong)
//   {
//     Matruong = matruong;
//     Tentruong = tentruong;
//   }

//   public override bool Equals(object obj)
//   {
//     return obj is NewClass other &&
//             Matruong == other.Matruong &&
//             Tentruong == other.Tentruong;
//   }

//   public override int GetHashCode()
//   {
//     return HashCode.Combine(Matruong, Tentruong);
//   }
// }

// public class KB_LoadTreeResult : ApiResultBaseDO
// {
//   public int total_items { get; set; }

//   public int total_pages { get; set; }

//   public List<KB_LoadTaiLieuDO> data { get; set; }
// }

// public class KB_LoadTaiLieuResult : ApiResultBaseDO
// {
//   public int total_items { get; set; }

//   public int total_pages { get; set; }

//   public List<KB_LoadTaiLieuDO> data { get; set; }
// }

// public class KB_LoadTaiLieuDO
// {
//   public BigID id_san_pham { get; set; }
//   public string ma_san_pham { get; set; }
//   public string ten_san_pham { get; set; }

//   public BigID id_phan_he { get; set; }
//   public string ma_phan_he { get; set; }
//   public string ten_phan_he { get; set; }

//   public BigID id_chuc_nang { get; set; }
//   public string ma_chuc_nang { get; set; }
//   public string ten_chuc_nang { get; set; }

//   public BigID id_loai_tai_lieu { get; set; }
//   public string ma_loai_tai_lieu { get; set; }
//   public string ten_loai_tai_lieu { get; set; }


//   public BigID id_loai_noi_dung { get; set; }
//   public string ma_loai_noi_dung { get; set; }
//   public string ten_loai_noi_dung { get; set; }

//   public BigID id_tai_lieu { get; set; }
//   public string ma_tai_lieu { get; set; }
//   public string ten_tai_lieu { get; set; }
//   public int tt_tai_lieu { get; set; }
//   public string tom_tat_tai_lieu { get; set; }
//   public string noi_dung_tai_lieu { get; set; }
//   public string noi_dung_tai_lieu1 { get; set; }
//   public string noi_dung_tai_lieu2 { get; set; }
// }
