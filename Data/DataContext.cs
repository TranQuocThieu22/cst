using LiteDB;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace educlient.Data
{
    public interface IDbLiteContext : IDisposable
    {
        ILiteCollection<T> Table<T>();
    }



    public class DataContext : IDbLiteContext
    {
        readonly LiteDatabase MainDB;
        readonly IConfiguration configuration;
        readonly string dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");

        public DataContext(IConfiguration configuration)
        {
            this.configuration = configuration;
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
            MainDB = new LiteDatabase("data/main.db");
        }

        public void Dispose()
        {
            MainDB?.Dispose();
        }

        public ILiteCollection<T> Table<T>()
        {
            return MainDB.GetCollection<T>();
        }

    }

    /// Dinh nghia table can luu trong database
    public class DataRelease
    {
        [BsonId]
        public int id { get; set; }
        public string ngaychot { get; set; }
        public string macase { get; set; }
        public string version { get; set; }
        public string vesion { get; set; }
        public string version_rl { get; set; }
        public string loaicase { get; set; }
        public string matruong { get; set; }
        public string phanhe { get; set; }
        public string chitietyc { get; set; }
        public string ngaydukien { get; set; }
        public string whatnew { get; set; }
        public string reviewcase { get; set; }
    }

    public class AnnualAQDataStatus
    {
        [BsonId]
        public int id { get; set; }
        public int year { get; set; }
        public bool isSetup { get; set; }
        public int numberOfSetup { get; set; }
    }

    public class AQMember
    {
        [BsonId]
        public int id { get; set; }
        public string TFSName { get; set; }
        public string password { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public byte[] avatar { get; set; }
        public DateTime birthDate { get; set; }
        public DateTime startDate { get; set; }
        public string nickName { get; set; }
        public string role { get; set; }
        public bool isLeader { get; set; }
        public bool isLunchStatus { get; set; }
        public int workingYear { get; set; } = 0;
        public int minWFHQuota { get; set; }
        public int additionalWFHQuota { get; set; } = 0;
        public int minAbsenceQuota { get; set; }
        public int additionalAbsenceQuota { get; set; } = 0;
        public bool isActive { get; set; }
        public string MaSoCCCD { get; set; }
        public string address { get; set; }
        public DateTime? contractStartDate { get; set; } = null;
        public DateTime? contractExpireDate { get; set; } = null;
        public string contractType { get; set; }
        public int employeeType { get; set; }
        public string note { get; set; }
    }

    public class DayOff
    {
        [BsonId]
        public int id { get; set; }
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
        public float sumDay { get; set; }
        public float sumDayWithWeekend { get; set; }
        public string reason { get; set; }
        public string note { get; set; }
    }

    public class Commission
    {
        [BsonId]
        public int id { get; set; }
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
        public float sumDay { get; set; }
        public string comissionContent { get; set; }
        public string transportation { get; set; }
        public List<CommissionMember> memberList { get; set; }
        public int commissionExpenses { get; set; }
        public string note { get; set; }
    }

    public class CommissionMember
    {
        public int id { get; set; }
        public int memberExpenses { get; set; }
    }

    public class ApprovalInput
    {
        public int id { get; set; }
        public string approvalStatus { get; set; }
    }

    public class IndividualDayOff
    {
        [BsonId]
        public int id { get; set; }
        public DateTime date { get; set; }
        public int memberId { get; set; }
        public string reason { get; set; }
        public int periodType { get; set; }
        public int dayOffType { get; set; }
        public bool isDayOffWithPayment { get; set; }
        public string approvalStatus { get; set; }
        public string note { get; set; }
    }

    public class WorkingOnlineDay
    {
        [BsonId]
        public int id { get; set; }
        public DateTime date { get; set; }
        public int memberId { get; set; }
        public string reason { get; set; }
        public int periodType { get; set; }
        public int wfhType { get; set; }
        public string approvalStatus { get; set; }
        public string note { get; set; }
    }

    public class WorkingOTDataDO
    {
        [BsonId]
        public int id { get; set; }
        public DateTime date { get; set; }
        public float time { get; set; }
        public int memberId { get; set; }
        public string note { get; set; }
    }

    public class SchoolProfile
    {
        [BsonId]
        public string IdTruong { get; set; }
        public string MaTruong { get; set; }
        public string TenTruong { get; set; }
        public DateTime? ThoiDiemTrienKhai { get; set; }
        public int? SoNamDungEdusoft { get; set; }
        public DateTime? NgayHetHanNangCap { get; set; }
        public string DiaChiTruong { get; set; }
        public ContactPerson HieuTruong { get; set; }
        public ContactPerson HieuPho { get; set; }
        public ContactPerson TruongPhongDaoTao { get; set; }
        public ContactPerson TruongPhongKhaoThi { get; set; }
        public ContactPerson TruongPhongTaiVu { get; set; }
        public ContactPerson Admin { get; set; }
        public string GhiChuKinhDoanh { get; set; }
        public string GhiChuKyThuat { get; set; }
        public string GhiChuChamSoc { get; set; }
        public AddinModule DanhSachAddin { get; set; }
        public LuuYDacThu LuuYXuLyDacThu { get; set; }
        public ThongTinServer ServerInfo { get; set; }

    }

    public class AddinModule
    {
        //public string IDAddin { get; set; }
        //public string TenAddin { get; set; }
        public string GhiChuSale { get; set; }
        public string GhiChuDev { get; set; }
        public string GhiChuSupport { get; set; }
    }

    public class ContactPerson
    {
        public string HoTen { get; set; }
        public string DienThoai { get; set; }
        public string Email { get; set; }
    }

    public class LuuYDacThu
    {
        public string SupportGhiChuMoHinh { get; set; }
        public string SupportGhiChuCachHoTro { get; set; }
        public string DevGhiChu { get; set; }
        public string SaleGhiChu { get; set; }
    }

    public class ThongTinServer
    {
        public string ThongTinChung { get; set; }
        public string GhiChu { get; set; }
        public string NguoiQuanLy { get; set; }
    }
}
