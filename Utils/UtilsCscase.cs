using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace educlient.Utils
{
    public class UtilsCscase
    {
        public static DataTable GetTFSTable()
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
            return dt;
        }

        public static bool IsNgayDuKienCoTruocReleaseCST(DateTime ngayDuKien)
        {
            DateTime releaseCSTtime = new DateTime(2023, 10, 16);
            return DateTime.Compare(ngayDuKien, releaseCSTtime) > 0;
        }
    }
}
