using LiteDB;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

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

    public class AQMember
    {
        [BsonId]
        public int id { get; set; }
        public string TFSName { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string avatar { get; set; }
        public DateTime birthDate { get; set; }
        public DateTime startDate { get; set; }
        public string nickName { get; set; }
        public string role { get; set; }
        public bool isLeader { get; set; }
        public bool isLunch { get; set; }
        public int WFHQuota { get; set; }
        public int absenceQuota { get; set; }
        public bool isActive { get; set; }
    }

    public class DayOff
    {
        [BsonId]
        public int id { get; set; }
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
        public float sumDay { get; set; }
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
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
        public float sumDay { get; set; }
        public int memberId { get; set; }
        public string reason { get; set; }
        public bool isAnnual { get; set; }
        public bool isWithoutPay { get; set; }
        public string approvalStatus { get; set; }
        public string note { get; set; }
    }

    public class WorkingOnlineDataDO
    {
        [BsonId]
        public int id { get; set; }
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
        public float sumDay { get; set; }
        public int memberId { get; set; }
        public string reason { get; set; }
        public string approvalStatus { get; set; }
        public string note { get; set; }
    }

}
