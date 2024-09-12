using System;
using System.Collections.Generic;

namespace educlient.Models
{
    public class csCase
    {
        public class CSCaseThongKeResult : ApiResultBaseDO
        {
            public CSCaseThongKeDataDO data { get; set; }
        }
        public class CSCaseThongKeDataDO
        {
            public Boolean is_tfs { get; set; }
            public List<ThongTinCase> data_case { get; set; }
            //public List<DataTruong> data_truong { get; set; }
            //public object newcase { get; set; }
            //public object onecase { get; set; }
            //public object dinhkem { get; set; }
            //public object linkfile { get; set; }

        }
        public class CSCaseThongKeSupDataDO
        {
            public Boolean is_tfs { get; set; }
            public List<XuLyCaseSupdataDO> data_case { get; set; }
            //public List<DataTruong> data_truong { get; set; }
            //public object newcase { get; set; }
            //public object onecase { get; set; }
            //public object dinhkem { get; set; }
            //public object linkfile { get; set; }

        }
        public class ThongTinCase
        {
            public string macase { get; set; }          //  System.Id
            public string matruong { get; set; }        //  AQ.Customer
            public string ngaynhan { get; set; }        //  System.CreatedDate
            public string trangthai { get; set; }       //  System.State
            public string ngaydukien { get; set; }      //  AQ.TargetDate
            public string loaihopdong { get; set; }      //  AQ.ContractType
            public string mucdo { get; set; }       //  AQ.PriorityType
            public string version { get; set; }      // AQ.UserGuideRequested
            public string hieuluc { get; set; }         //  AQ.ReleaseDate
            public string resolvedate { get; set; }       //  AQ.MailSentDate
            public string mailto { get; set; }          //  AQ.MailTo
            public string loaicase { get; set; }        //  AQ.CaseType
            public string comment { get; set; }         // Comment
            public string assignedto { get; set; }
            public float? estimatetime { get; set; }
            public float? actualtime { get; set; }
            public DateTime? detaildate1 { get; set; }
            public DateTime? detaildate2 { get; set; }
            public DateTime? detaildate3 { get; set; }
            public DateTime? detaildate4 { get; set; }
            public DateTime? detaildate5 { get; set; }
            public DateTime? detaildate6 { get; set; }
            public DateTime? detaildate7 { get; set; }
            public DateTime? detaildate8 { get; set; }
            public DateTime? detaildate9 { get; set; }
            public DateTime? detaildate10 { get; set; }
            public float? detailactualtime1 { get; set; }
            public float? detailactualtime2 { get; set; }
            public float? detailactualtime3 { get; set; }
            public float? detailactualtime4 { get; set; }
            public float? detailactualtime5 { get; set; }
            public float? detailactualtime6 { get; set; }
            public float? detailactualtime7 { get; set; }
            public float? detailactualtime8 { get; set; }
            public float? detailactualtime9 { get; set; }
            public float? detailactualtime10 { get; set; }
            public string? RequiredAttendee1 { get; set; }
            public string? RequiredAttendee2 { get; set; }
            public string? RequiredAttendee3 { get; set; }
            public string? RequiredAttendee4 { get; set; }
            public string? RequiredAttendee5 { get; set; }
            public string? RequiredAttendee6 { get; set; }
            public string? RequiredAttendee7 { get; set; }
            public string? RequiredAttendee8 { get; set; }
            public string? RequiredAttendee9 { get; set; }
            public string? RequiredAttendee10 { get; set; }
            public int? MinuteTakerTime { get; set; }
            public string? MeetingStart { get; set; }
        }



        public class ThongTinCaseSup
        {
            public string macase { get; set; }          //  System.Id
            public string matruong { get; set; }        //  AQ.Customer
            public string ngaynhan { get; set; }        //  System.CreatedDate
            public string trangthai { get; set; }       //  System.State
            public string yeucautrangthai { get; set; }       //  System.ReqState
            public string ngaydukien { get; set; }      //  AQ.TargetDate
            public string loaihopdong { get; set; }      //  AQ.ContractType
            public string mucdo { get; set; }       //  AQ.PriorityType
            public string version { get; set; }      // AQ.UserGuideRequested
            public string hieuluc { get; set; }         //  AQ.ReleaseDate
            public string ngayemail { get; set; }       //  AQ.MailSentDate
            public string mailto { get; set; }          //  AQ.MailTo
            public string loaicase { get; set; }        //  AQ.CaseType
            public string comment { get; set; }         // Comment
            public string caseanalyst { get; set; }
            public string casetester { get; set; }
            public string assignedto { get; set; }
            public string datekqphantich { get; set; }
            public string datekqtest { get; set; }
            public string resolveddate { get; set; }
        }
        public class XuLyCasedataDO
        {
            public string assignedto { get; set; }
            public int canXuLy { get; set; }
            public int XuLyTre { get; set; }
            public int SoCaseTrongNgay { get; set; }
            public float TgCanXyLy { get; set; }
            public float LuongGioTrongNgay { get; set; }
        }
        public class XuLyCaseDataResult : ApiResultBaseDO
        {
            public int count { get; set; }
            public List<XuLyCasedataDO> data { get; set; }
        }
        public class dataTreBaoTri
        {
            public string macase { get; set; }
            public string assignedto { get; set; }
            public int SoNgayTre { get; set; }
            public int canXuLy { get; set; }
            public int SoNgayTreTrienKhai { get; set; }
            public int SoCaseTrongNgay { get; set; }
            public float estimatetime { get; set; }
        }
        public class XuLyCaseSupDataResult : ApiResultBaseDO
        {
            public int count { get; set; }
            public List<XuLyCaseSupdataDO> data { get; set; }
        }
        public class XuLyCaseSupdataDO
        {
            public string assignedto { get; set; }
            public int canXuLy { get; set; }
            public int XuLyTre { get; set; }
            public int PhanTichTre { get; set; }
            public int TestTre { get; set; }
            public int CaseLamTrongNgay { get; set; }
            public List<string> CaseList { get; set; }
        }

        public class CanXuLyDataDO
        {
            public string assignedto { get; set; }
            public List<string> caseList { get; set; }
            public int CanTest { get; set; }
            public int CanPhanTich { get; set; }
            public int DuocGan { get; set; }
        }
        public class CaseSupLamTrongNgayDataDO
        {
            public string assignedto { get; set; }
            public List<string> caseList { get; set; }
            public int TongSo { get; set; }
        }
        public class AQReportDataDO
        {
            public string Name { get; set; }
            public int TongCase { get; set; }
            public int ConHan { get; set; }
            public int TreHan { get; set; }
            public int NhanSu { get; set; }
        }
        public class AQReportResult : ApiResultBaseDO
        {
            public List<AQReportDataDO> data { get; set; }

        }

        public class CaseTheoThoiGianChoDataDO
        {
            public List<string> macase { get; set; }
            public int SoCase { get; set; }
            public string tuanSo { get; set; }
        }
        public class CaseTheoThoiGianChoResult : ApiResultBaseDO
        {
            public int count { get; set; }
            public List<CaseTheoThoiGianChoDataDO> data { get; set; }
        }
        public class DateInput
        {
            public string data { get; set; }
            public string tmp { get; set; }
        }
    }
}
