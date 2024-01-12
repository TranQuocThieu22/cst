using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

using System.Data.SqlClient;
using System.Data.Common;

namespace educlient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MainController : ControllerBase
    {
        string m_connStr;
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
        public object Login(LoginModel log)
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "clients.dat");
            var clients = JsonConvert.DeserializeObject<List<EduClient>>(System.IO.File.ReadAllText(dir));
            var user = clients.FirstOrDefault(r => r.MaTruong?.ToLower() == log?.username?.ToLower() && r.Pass == log.password);

            if (user != null)
            {
                _logger.LogInformation("Login Success: " + log.username);
                Session.SetString("current-user", JsonConvert.SerializeObject(user));
            }
            else
            {
                user = new EduClient { TenTruong = "Username or password is incorrect!" };
                _logger.LogInformation("Login Fail: " + log.username);
            }
            return user;
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

        public MainController(ILogger<MainController> logger)
        {
            _logger = logger;
            // connStr = Configuration.GetConnectionString("ProdConnection");
            m_connStr = @"Server=192.168.1.204;Database=AQTech.KB;User Id=dev;Pwd=dev@edusoft;Connect Timeout=1800";
        }

        // AQ\tfsuser:2lvmoc5arhu2pshlcmh6kywgzgf3evpxgydjmefyjbf5jtvcw6za
        // Expired: 3/10/2022 3:16:51 PM
        const string TFS_HOST = "http://dev.aqtech.vn:8080"; //"https://dev.aqtech.vn:1443";
        const string TFS_TOKEN_BASE64 = "QVFcdGZzdXNlcjpyY3phdmVsczJ6ZGw2bDZqdDZ6cXRxdGp0YW1wMzQ1NDQyYm9ycXk3cGNyd2doem1icHFx";
        async Task<string> DoTfsQueryData(string pTfsHost, string pbaseUrl, string pQueryAppend, string pPOSTBody, string pBasicToken)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri(pTfsHost);
                    client.DefaultRequestHeaders.Add("User-Agent", "AQTech TFS program");
                    client.DefaultRequestHeaders.Add("Authorization", "Basic " + TFS_TOKEN_BASE64);
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
                return "";
            }
        }

        async Task<List<workItem0>> DoTfsFetchData(string pProject, string pProjectTeam, string pWIType, string pAQCustomer, string pFieldList, string pStateList, string pAppendWHERE)
        {
            string sWIQL = "{\"query\": \"SELECT [System.Id] FROM WorkItems"
                                        + " WHERE ([System.TeamProject]='" + pProject + "')"
                                            + (pWIType.Length > 0 ? " AND ([System.WorkItemType] IN (" + pWIType + "))" : "")
                                            + (pAQCustomer.Length > 0 ? " AND ([AQ.Customer] IN (" + pAQCustomer + "))" : "")
                                            + " AND ([System.State] NOT IN ('Removed', 'Rejected','Hủy'))" //,'Done','Đóng case','Đã xử lý','Đã gửi mail'
                                            + (pStateList.Length > 0 ? " AND ([System.State] IN (" + pStateList + "))" : "")
                                            + (pAppendWHERE.Length > 0 ? " AND (" + pAppendWHERE + ")" : "");
            sWIQL += " ORDER BY [System.CreatedDate] " + "\"}";

            var jsonCasesId = await DoTfsQueryData(TFS_HOST
                                            , "tfs/aq/" + pProject + "/" + pProjectTeam + "/_apis/wit/wiql?api-version=4.1"
                                            , ""
                                            , sWIQL, TFS_TOKEN_BASE64);

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


                string sTfsFieldList = (pFieldList?.Length == 0 ? "System.AssignedTo,System.Id,AQ.Customer,System.CreatedDate,System.Title,System.State,AQ.TargetDate,AQ.ReleaseDate,AQ.MailSentDate,AQ.MailTo,AQ.Priority" : pFieldList);
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

        [HttpGet, Route("test")]
        public string test()
        {
            string ngaybatdau = "'01/07/2020 00:00'";   // chỉ lấy từ ngày này; trên server tfs (203..) format dd/MM/yyyy
            string list_trangthai = ""; // "'Mở case', 'Đang xử lý', 'Đã xử lý', 'Đã gửi mail', 'Đóng case'";
            var lstAll0 = DoTfsFetchData("Edusoft.Net-CS"
                                             , "Edusoft.Net-CS%20Team"
                                             , "'CS Case', 'CS CASE'"
                                             , ""// matruong
                                             , ""
                                             , list_trangthai
                                             , " ([System.CreatedDate] >=" + ngaybatdau + ")"
                                               + " AND ([AQ.Customer] NOT IN ('RELEASE', 'CST', 'AQ', 'SEMINAR'))"
                                               + " AND ( ([System.State] NOT IN ('Đóng case')) OR ([AQ.MailSentDate] <> '') )"
                                               + " AND ([AQ.Priority] NOT IN ('5 - Cần theo dõi'))"
                                             );

            List<workItem0> lstAll = lstAll0.Result;

            return "";
        }

        [HttpPost, Route("cscase")]
        public CSCaseResult ViewCsCase(CSCaseBindingModel model)
        {
            if (currentUser == null)
            {
                return new CSCaseResult { code = 500, result = false, message = "Not Login" };
            }

            //get data thon tin ma truong ten truong trong file .dat
            var dir = Path.Combine(AppContext.BaseDirectory, "clients.dat");
            var clients = JsonConvert.DeserializeObject<List<EduClient>>(System.IO.File.ReadAllText(dir));

            string list_matruong = "";

            if (currentUser?.Roles?.ToLower().Equals("admin", StringComparison.OrdinalIgnoreCase) == true)
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

            ///1 Get File => neu ko co file hoac file trống thì get TFS/////
            var data = System.IO.File.ReadAllText(dataFile);
            var list = JsonConvert.DeserializeObject<List<EduCase>>(data);

            ///1 Get File/////
            if (System.IO.File.Exists(dataFile) && data.Length > 0
            && (list?.Count > 1 && list?[0].macase != "" && list?[0].matruong != ""))
            {

                var listTruong = new List<DataTruong>();

                // get list truong trong TFS (1)
                var truongtemp = list.Select(o => new
                {
                    matruong = o.matruong,
                    tentruong = "",
                }).Distinct().OrderBy(s => s.matruong).ToList();


                // list truong trong TFS (1) JOIN file data.dat logon (2) => get ten truong
                listTruong = clients.Join(truongtemp, a => a.MaTruong, b => b.matruong,
                  (a, b) => new DataTruong
                  {
                      matruong = a.MaTruong,
                      tentruong = a.TenTruong
                  }).Distinct().OrderBy(s => s.matruong).ToList();

                var datatemp = new CSCaseDataDO
                {
                    // total_items = list.Count(),
                    // total_pages = (list.Count() + model.additional.paging.limit - 1) / model.additional.paging.limit,
                    // data_case = list.OrderBy(o => o.macase).Skip(
                    //        (model.additional.paging.page - 1) * model.additional.paging.limit)
                    //        .Take(model.additional.paging.limit).ToList(),
                    is_tfs = false,
                    data_case = list.ToList(),
                    data_truong = listTruong.Distinct().ToList(),
                };
                return new CSCaseResult { data = datatemp, code = 200, result = true };
            }
            else /// 2 Get TSF
            {

                string ngaybatdau = "'01/07/2020 00:00'";   // chỉ lấy từ ngày này; trên server tfs (203..) format dd/MM/yyyy
                string list_trangthai = ""; // "'Mở case', 'Đang xử lý', 'Đã xử lý', 'Đã gửi mail', 'Đóng case'";
                var lstAll0 = DoTfsFetchData("Edusoft.Net-CS"
                                                 , "Edusoft.Net-CS%20Team"
                                                 , "'CS Case', 'CS CASE'"
                                                 , list_matruong// matruong
                                                 , ""
                                                 , list_trangthai
                                                 , " ([System.CreatedDate] >=" + ngaybatdau + ")"
                                                   + " AND ([AQ.Customer] NOT IN ('RELEASE', 'CST', 'AQ', 'SEMINAR'))"
                                                   + " AND ( ([System.State] NOT IN ('Đóng case')) OR ([AQ.MailSentDate] <> '') )"
                                                   + " AND ([AQ.Priority] NOT IN ('5 - Cần theo dõi'))"
                                                 );

                List<workItem0> lstAll = lstAll0.Result;

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
                    dt.Columns.Add("hieuluc", typeof(string));
                    dt.Columns.Add("dabangiao", typeof(string));
                    dt.Columns.Add("ngayemail", typeof(DateTime));
                    dt.Columns.Add("mailto", typeof(string));

                    foreach (var r in lstAll)
                    {
                        DataRow dr = dt.NewRow();

                        bool isMoCase = false;
                        bool fixTrangThai = false;
                        bool fixTrangThai2 = false;
                        bool fixDaBanGiao = false;

                        dr["trangthai"] = "";

                        foreach (KeyValuePair<string, string> kvp in r.fields)
                        {
                            if (kvp.Key.ToLower().Equals("system.id"))
                                dr["macase"] = kvp.Value;
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
                            else if (kvp.Key.ToLower().Equals("aq.releasedate"))
                                dr["hieuluc"] = kvp.Value;
                            else if (kvp.Key.ToLower().Equals("aq.mailsentdate"))
                                dr["ngayemail"] = kvp.Value;
                            else if (kvp.Key.ToLower().Equals("aq.mailto"))
                                dr["mailto"] = kvp.Value;
                            else if (kvp.Key.ToLower().Equals("system.assignedto"))
                            {
                                if (kvp.Value.ToLower().Contains("tiepnhan"))
                                    fixTrangThai = true;
                                else if (kvp.Value.ToLower().Contains("root"))
                                    fixTrangThai2 = true;
                            }

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
                        if (fixDaBanGiao)
                            dr["dabangiao"] = "X";

                        // FIX 3 : RL tuần thứ 1 tháng 9/2020
                        if (dr["ngaydukien"] != null)
                        {
                            DateTime d = (DateTime)dr["ngaydukien"];
                            dr["hieuluc"] = calcTuanRelease(d);
                        }

                        // FIX 4
                        if (dr["trangthai"].ToString().ToLower().Contains("đang phân tích")
                           || dr["trangthai"].ToString().ToLower().Contains("đang chờ phân tích"))
                        {
                            dr["dabangiao"] = "";
                            dr["ngaydukien"] = DBNull.Value;
                            dr["hieuluc"] = "";
                        }

                        dt.Rows.Add(dr);
                    } // each record

                    sRet = JsonConvert.SerializeObject(dt);
                    var list1 = JsonConvert.DeserializeObject<List<EduCase>>(sRet);

                    var listTruong = new List<DataTruong>();
                    // get list truong trong TFS (1)
                    var truongtemp = list1.Select(o => new
                    {
                        matruong = o.matruong,
                        tentruong = "",
                    }).Distinct().OrderBy(s => s.matruong).ToList();

                    // list truong trong TFS (1) JOIN file data.dat logon (2) => get ten truong
                    listTruong = clients.Join(truongtemp, a => a.MaTruong, b => b.matruong,
                      (a, b) => new DataTruong
                      {
                          matruong = a.MaTruong,
                          tentruong = a.TenTruong
                      }).Distinct().OrderBy(s => s.matruong).ToList();


                    var datatemp = new CSCaseDataDO
                    {
                        // total_items = list1.Count(),
                        // total_pages = (list1.Count() + model.additional.paging.limit - 1) / model.additional.paging.limit,
                        // data_case = list1.OrderBy(o => o.macase).Skip(
                        //         (model.additional.paging.page - 1) * model.additional.paging.limit)
                        //         .Take(model.additional.paging.limit).ToList(),

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

        public async Task<DataTable?> MSSQL_GetData(string pSQL)
        {
            string connStr = m_connStr;

            string commandText = pSQL;

            using (var connection = new SqlConnection(connStr))
            {
                await connection.OpenAsync();   //vs  connection.Open();
                using (var tran = connection.BeginTransaction())
                {
                    using (var cmd = new SqlCommand(commandText, connection, tran))
                    {
                        try
                        {
                            // vs also alternatives, command.ExecuteReader();  or await command.ExecuteNonQueryAsync();
                            SqlDataReader rdr = await cmd.ExecuteReaderAsync();
                            // await command.ExecuteNonQueryAsync();
                            var dt = new DataTable();
                            dt.Load(rdr);

                            // while (rdr.Read()) {}

                            await rdr.CloseAsync();

                            return dt;
                        }
                        catch (Exception Ex)
                        {
                            await connection.CloseAsync();
                            string msg = Ex.Message.ToString();
                            tran.Rollback();

                            return null;
                            // throw;
                        }
                    }
                }
            }

            return null;
        } //

        [HttpGet, Route("kb-loadtree-faq")]
        public async Task<object> KB_LoadTree_FAQ()
        {
            return await KB_LoadTree("'FAQ'");
        }

        [HttpGet, Route("kb-loadtree-guide")]
        public async Task<object> KB_LoadTree_Guide()
        {
            return await KB_LoadTree("'USER-GUIDE', 'QUICK-GUIDE'");
        }

        [HttpPost, Route("kb-loadtree")]
        public async Task<object> KB_LoadTree(params string[] p)
        {
            string s_list = "";
            foreach (string s in p)
            {
                s_list = s_list + (!string.IsNullOrEmpty(s_list) ? ", " : "") + "'" + s + "'";
            }

            return await KB_LoadTree(s_list);
        }

        async Task<object> KB_LoadTree(string pList_MaLoaiTaiLieu)
        {
            string sSQL = @"SELECT t1.IDSanPham, sp.MaSanPham, sp.TenSanPham,
	                           t1.IDPhanHe, ph.MaPhanHe, ph.TenPhanHe,
	                           t1.IDChucNang, cn.MaChucNang, cn.TenChucNang,
	                           loai_tl.MaLoaiTaiLieu, loai_tl.TenLoaiTaiLieu,
	                           loai_nd.MaLoaiNoiDung, loai_nd.TenLoaiNoiDung,
	                           t1.IDTaiLieu, t1.MaTaiLieu, t1.TenTaiLieu, t1.TtTaiLieu
                            FROM [KB-TaiLieu] t1 
	                            LEFT JOIN [KB-DMSanPham] sp ON t1.IDSanPham=sp.IDSanPham
	                            LEFT JOIN [KB-DMPhanHe] ph ON t1.IDPhanHe=ph.IDPhanHe
	                            LEFT JOIN [KB-DMChucNang] cn ON t1.IDChucNang=cn.IDChucNang
	                            LEFT JOIN [KB-DMLoaiTaiLieu] loai_tl ON t1.IDLoaiTaiLieu=loai_tl.IDLoaiTaiLieu
	                            LEFT JOIN [KB-DMLoaiNoiDung] loai_nd ON t1.IDLoaiNoiDung=loai_nd.IDLoaiNoiDung"
                        + (!string.IsNullOrEmpty(pList_MaLoaiTaiLieu) ? " WHERE (loai_tl.MaLoaiTaiLieu IN (" + pList_MaLoaiTaiLieu + "))" : "");

            var dt = await MSSQL_GetData(sSQL);
            // return JsonConvert.SerializeObject(dt );

            var lstTree = from l1 in dt.AsEnumerable()
                          group l1 by new { MaSP = l1.Field<string>("MaSanPham"), TenSP = l1.Field<string>("TenSanPham") } into group1
                          select new
                          {
                              Level = 1,
                              Ma = group1.Key.MaSP,
                              Ten = group1.Key.TenSP,
                              childs = from l2 in group1
                                       group l2 by new { MaPH = l2.Field<string>("MaPhanHe"), TenPH = l2.Field<string>("TenPhanHe") } into group2
                                       select new
                                       {
                                           Level = 2,
                                           Ma = group2.Key.MaPH,
                                           Ten = group2.Key.TenPH,
                                           childs = from l3 in group2
                                                    group l3 by new { MaCN = l3.Field<string>("MaChucNang"), TenCN = l3.Field<string>("TenChucNang") } into group3
                                                    select new
                                                    {
                                                        Level = 3,
                                                        Ma = group3.Key.MaCN,
                                                        Ten = group3.Key.TenCN,
                                                        childs = from l4 in group3
                                                                 select new
                                                                 {
                                                                     Level = 4,
                                                                     LoaiTL = l4.Field<string>("MaLoaiTaiLieu"),
                                                                     MaTL = l4.Field<string>("MaTaiLieu"),
                                                                     TenTL = l4.Field<string>("TenTaiLieu"),
                                                                     //NoiDungTL = l4.Field<string>("NoiDungTaiLieu")
                                                                 }
                                                    }
                                       }
                          };

            return JsonConvert.SerializeObject(lstTree); // "Test OK";

        } // Test3()


        [HttpPost, Route("kb-loadtailieu")]
        public async Task<KB_LoadTaiLieuResult> KB_LoadTaiLieu(params string[] pIDTaiLieu)
        {
            string slist_MaTaiLieu = "";
            foreach (string s in pIDTaiLieu)
            {
                slist_MaTaiLieu = slist_MaTaiLieu + (!string.IsNullOrEmpty(slist_MaTaiLieu) ? ", " : "") + "'" + s + "'";
            }

            string sSQL = @"SELECT t1.IDSanPham, sp.MaSanPham, sp.TenSanPham,
	                           t1.IDPhanHe, ph.MaPhanHe, ph.TenPhanHe,
	                           t1.IDChucNang, cn.MaChucNang, cn.TenChucNang,
	                           loai_tl.IDLoaiTaiLieu, loai_tl.MaLoaiTaiLieu, loai_tl.TenLoaiTaiLieu,
	                           loai_nd.IDLoaiNoiDung, loai_nd.MaLoaiNoiDung, loai_nd.TenLoaiNoiDung,
	                           t1.IDTaiLieu, t1.MaTaiLieu, t1.TenTaiLieu, t1.TtTaiLieu, t1.TomTatTaiLieu, t1.NoiDungTaiLieu, t1.NoiDungTaiLieu1, t1.NoiDungTaiLieu2
                            FROM [KB-TaiLieu] t1 
	                            LEFT JOIN [KB-DMSanPham] sp ON t1.IDSanPham=sp.IDSanPham
	                            LEFT JOIN [KB-DMPhanHe] ph ON t1.IDPhanHe=ph.IDPhanHe
	                            LEFT JOIN [KB-DMChucNang] cn ON t1.IDChucNang=cn.IDChucNang
	                            LEFT JOIN [KB-DMLoaiTaiLieu] loai_tl ON t1.IDLoaiTaiLieu=loai_tl.IDLoaiTaiLieu
	                            LEFT JOIN [KB-DMLoaiNoiDung] loai_nd ON t1.IDLoaiNoiDung=loai_nd.IDLoaiNoiDung"
                            + (!string.IsNullOrEmpty(slist_MaTaiLieu) ? " WHERE (t1.IDTaiLieu IN (" + slist_MaTaiLieu + "))" : "");
            
            var dt = await MSSQL_GetData(sSQL); 

            var lst_ret = from lst in dt.AsEnumerable()
                          //where lst.Field<string>("id_qr_code").Trim() == qr.id_qr_code.Trim()
                          select new KB_LoadTaiLieuDO()
                          {
                              id_san_pham = lst.Field<BigID>("IDSanPham"),
                              ma_san_pham = lst.Field<string>("MaSanPham"),
                              ten_san_pham = lst.Field<string>("TenSanPham"),
                              
                              id_phan_he = lst.Field<BigID>("IDPhanHe"),
                              ma_phan_he = lst.Field<string>("MaPhanHe"),
                              ten_phan_he = lst.Field<string>("TenPhanHe"),

                              id_chuc_nang = lst.Field<BigID>("IDChucNang"),
                              ma_chuc_nang = lst.Field<string>("MaChucNang"),
                              ten_chuc_nang = lst.Field<string>("TenChucNang"),

                              id_loai_tai_lieu = lst.Field<BigID>("IDLoaiTaiLieu"),
                              ma_loai_tai_lieu = lst.Field<string>("MaLoaiTaiLieu"),
                              ten_loai_tai_lieu = lst.Field<string>("TenLoaiTaiLieu"),

                              id_loai_noi_dung = lst.Field<BigID>("IDLoaiNoiDung"),
                              ma_loai_noi_dung = lst.Field<string>("MaLoaiNoiDung"),
                              ten_loai_noi_dung = lst.Field<string>("TenLoaiNoiDung"),
                              
                              id_tai_lieu = lst.Field<BigID>("IDTaiLieu"),
                              ma_tai_lieu = lst.Field<string>("MaTaiLieu"),
                              ten_tai_lieu = lst.Field<string>("TenTaiLieu"),
                              
                              tt_tai_lieu = lst.Field<int>("TtTaiLieu"),
                              tom_tat_tai_lieu = lst.Field<string>("TomTatTaiLieu"),
                              noi_dung_tai_lieu = lst.Field<string>("NoiDungTaiLieu"),
                              noi_dung_tai_lieu1 = lst.Field<string>("NoiDungTaiLieu1"),
                              noi_dung_tai_lieu2 = lst.Field<string>("NoiDungTaiLieu2"), 
                          };

            return new KB_LoadTaiLieuResult
            {
                result = true,
                code = 200,
                data = lst_ret.ToList()
            };

            //return JsonConvert.SerializeObject(lst_ret);
        }

    } // MainController class //
}

class TfsCaseDetailModel
{
    public int count { get; set; }
    public List<workItem0> value { get; set; }

}

public class workItem0
{
    public int id { get; set; }
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
    public string newpass { get; set; }
}

public class ResultModel
{
    public string error { get; set; }
    public object data { get; set; }
}

class EduClient
{
    public string MaTruong { get; set; }
    public string TenTruong { get; set; }
    public string Pass { get; set; }
    public string Roles { get; set; }
}

public class EduCase
{
    public string macase { get; set; }          //  System.Id
    public string matruong { get; set; }        //  AQ.Customer
    public string ngaynhan { get; set; }        //  System.CreatedDate
    public string chitietyc { get; set; }       //  System.Title
    public string trangthai { get; set; }       //  System.State
    public string ngaydukien { get; set; }      //  AQ.TargetDate
    public string hieuluc { get; set; }         //  AQ.ReleaseDate
    public string dabangiao { get; set; }       //
    public string ngayemail { get; set; }       //  AQ.MailSentDate
    public string mailto { get; set; }          //  AQ.MailTo
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
    // public int total_items { get; set; }
    // public int total_pages { get; set; }

    public Boolean is_tfs { get; set; }
    public List<EduCase> data_case { get; set; }
    public List<DataTruong> data_truong { get; set; }

}

public class CSCaseBindingModel
{
    public CSCaseFilterDO filter { get; set; }

    // public AdditionalDO additional { get; set; }
}

public class CSCaseFilterDO
{
    public string matruong { get; set; }
}

public class AdditionalDO
{
    public PagingDO paging { get; set; }

    public List<OrderingDO> ordering { get; set; }
}

public class PagingDO
{
    public int limit { get; set; }

    public int page { get; set; }
}

public class OrderingDO
{
    public string name { get; set; }

    /// <summary>
    /// 0 asc; 1 desc
    /// </summary>
    public int order_type { get; set; }
}

internal class NewClass
{
    public string Matruong { get; }
    public string Tentruong { get; }

    public NewClass(string matruong, string tentruong)
    {
        Matruong = matruong;
        Tentruong = tentruong;
    }

    public override bool Equals(object obj)
    {
        return obj is NewClass other &&
                Matruong == other.Matruong &&
                Tentruong == other.Tentruong;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Matruong, Tentruong);
    }
}

public class KB_LoadTreeResult : ApiResultBaseDO
{
    public int total_items { get; set; }

    public int total_pages { get; set; }

    public List<KB_LoadTaiLieuDO> data { get; set; }
}

public class KB_LoadTaiLieuResult : ApiResultBaseDO
{
    public int total_items { get; set; }

    public int total_pages { get; set; }

    public List<KB_LoadTaiLieuDO> data { get; set; }
}

public class KB_LoadTaiLieuDO
{
    public BigID id_san_pham { get; set; }
    public string ma_san_pham { get; set; }
    public string ten_san_pham { get; set; }

    public BigID id_phan_he { get; set; }
    public string ma_phan_he { get; set; }
    public string ten_phan_he { get; set; }

    public BigID id_chuc_nang { get; set; }
    public string ma_chuc_nang { get; set; }
    public string ten_chuc_nang { get; set; }

    public BigID id_loai_tai_lieu { get; set; }
    public string ma_loai_tai_lieu { get; set; }
    public string ten_loai_tai_lieu { get; set; }


    public BigID id_loai_noi_dung { get; set; }
    public string ma_loai_noi_dung { get; set; }
    public string ten_loai_noi_dung { get; set; }

    public BigID id_tai_lieu { get; set; }
    public string ma_tai_lieu { get; set; }
    public string ten_tai_lieu { get; set; }
    public int tt_tai_lieu { get; set; }
    public string tom_tat_tai_lieu { get; set; }
    public string noi_dung_tai_lieu { get; set; }
    public string noi_dung_tai_lieu1 { get; set; }
    public string noi_dung_tai_lieu2 { get; set; }
}

