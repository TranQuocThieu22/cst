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
        public List<detailLunch> detailLunch { get; set; } = new List<detailLunch>
        {
            new detailLunch()
        };
        public int workingYear { get; set; } = 0;
        public detailWFHQuota detailWFHQuota { get; set; } = new detailWFHQuota();
        public detailAbsenceQuota detailAbsenceQuota { get; set; } = new detailAbsenceQuota();
        public bool isActive { get; set; }
        public string MaSoCCCD { get; set; } = "";
        public string address { get; set; } = "";
        public detailContract detailContract { get; set; } = new detailContract();
    }
    public class detailContract
    {
        public DateTime contractStartDate { get; set; } = DateTime.Now.Date;
        public DateTime contractExpireDate { get; set; } = DateTime.Now.Date;
        public int contractDuration { get; set; } = 0;
        public string contractType { get; set; } = "";
    }
    public class detailAbsenceQuota
    {
        public int minAbsenceQuota { get; set; } = 0;
        public List<actualAbsenceQuotaByYear> actualAbsenceQuotaByYear { get; set; } = new List<actualAbsenceQuotaByYear>
        {
            new actualAbsenceQuotaByYear()
        };
    }

    public class actualAbsenceQuotaByYear
    {
        public int year { get; set; } = DateTime.Now.Year;
        public int absenceQuota { get; set; } = 0;
    }

    public class detailWFHQuota
    {
        public int minWFHQuota { get; set; } = 0;
        public List<actualWFHQuotaByYear> actualWFHQuotaByYear { get; set; } = new List<actualWFHQuotaByYear> {
            new actualWFHQuotaByYear()
        };
    }

    public class actualWFHQuotaByYear
    {
        public int year { get; set; } = DateTime.Now.Year;
        public int WFHQuota { get; set; } = 0;
    }

    public class detailLunch
    {
        public int year { get; set; } = DateTime.Now.Year;
        public List<lunchByMonth> lunchByMonth { get; set; } = Enumerable.Range(DateTime.Now.Month, 12 - DateTime.Now.Month + 1)
                            .Select(month => new lunchByMonth
                            {
                                month = month,
                                isLunch = false,
                            })
                            .ToList();
    }

    public class lunchByMonth
    {
        public int month { get; set; }
        public bool isLunch { get; set; }
        public int lunchFee { get; set; } = 0;
        public string note { get; set; } = "";
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

    public class WorkingOTDataDO
    {
        [BsonId]
        public int id { get; set; }
        public DateTime date { get; set; }
        public float time { get; set; }
        public int memberId { get; set; }
        public string note { get; set; }
    }
}
