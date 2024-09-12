using AQFramework.Utilities;
using educlient.Data;
using educlient.Models;
using System;
using System.Collections.Generic;

namespace educlient.Models
{
    public class DsNgayPhepChungResult : ApiResultBaseDO
    {
        public DsNgayPhepChungDO[] data { get; set; }
    }
    public class DsNgayPhepChungDO
    {
        public Guid id { get; set; }
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
        public float sumDay { get; set; }
        public string reason { get; set; }
        public string note { get; set; }
    }
    public class DsNgayPhepChungInput
    {
        public DateTime dateFrom { get; set; }
        public DateTime dateTo { get; set; }
        public float sumDay { get; set; }
        public string reason { get; set; }
        public string note { get; set; }
    }
    public class DsThongTinCaNhanResult : ApiResultBaseDO
    {
        public List<DsThongTinCaNhanDataDO> data { get; set; }
    }
    public class DsThongTinCaNhanDataDO
    {
        public Guid id { get; set; }
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
        public detailAbsenceQuota detailAbsenceQuota { get; set; }
        public bool isActive { get; set; }
    }
}
public class DsNgayPhepCaNhanResult : ApiResultBaseDO
{
    public DsNgayPhepCaNhanDO[] data { get; set; }
}
public class DsNgayPhepCaNhanDO
{
    public Guid id { get; set; }
    public DateTime Ngay { get; set; }
    public int SoLuongBuoi { get; set; }
    public string LyDoNghi { get; set; }
    public string UserNhap { get; set; }
    public string AqUser { get; set; }
    public bool TrangThai { get; set; }
    public Guid ThongTinCaNhanId { get; set; }
    public DsThongTinCaNhanDataDO ThongTinCaNhan { get; set; }


}

public class DsNgayPhepCaNhanInput
{
    public int SoLuongBuoi { get; set; }
    public string LyDoNghi { get; set; }
    public string UserNhap { get; set; }
    public string AqUser { get; set; }
    public bool TrangThai { get; set; }
    public Guid ThongTinCaNhanId { get; set; }

}
public class DsNgayCongTacResult : ApiResultBaseDO
{
    public DsNgayCongTacDO[] data { get; set; }
}
public class DsNgayCongTacDO
{
    public Guid id { get; set; }
    public DateTime dateFrom { get; set; }
    public DateTime dateTo { get; set; }
    public int sumDay { get; set; }
    public string comissionContent { get; set; }
    public string transportation { get; set; }
    public Number commissionExpenses { get; set; }
    public string note { get; set; }
}
public class DsNgayCongTacInput
{
    public int SoLuongBuoi { get; set; }
    public string NoiDungCongTac { get; set; }
    public string TruongCongTac { get; set; }
    public string UserNhap { get; set; }
    public string AqUser { get; set; }
}

