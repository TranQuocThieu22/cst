using LiteDB;
using Microsoft.Extensions.Configuration;
using System;
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
}
