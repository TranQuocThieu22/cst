using educlient.Models;
using System;
using System.Collections.Generic;

namespace educlient.Models
{
    public class DsNgayPhepChungResult : ApiResultBaseDO
    {
        public DsNgayPhepChungDataDO[] data { get; set; }
    }
    public class DsNgayPhepChungDataDO
    {
        public Guid id { get; set; }
        public DateTime Ngay { get; set; }
        public DateTime DenNgay { get; set; }
        public int SoLuongBuoi { get; set; }
        public string LyDoNghi { get; set; }
        public string UserNhap { get; set; }
        public string GhiChu { get; set; }
    }
    public class DsNgayPhepChungInput
    {
        public int SoLuongBuoi { get; set; }
        public string LyDoNghi { get; set; }
        public string UserNhap { get; set; }
        public string GhiChu { get; set; }
        public DateTime DenNgay { get; set; }
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
        public int absenceQuota { get; set; }
        public bool isActive { get; set; }
    }
    public class DsThongTinCaNhanInput
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
        public int absenceQuota { get; set; }
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
    public DsNgayCongTacDataDO[] data { get; set; }
}
public class DsNgayCongTacDataDO
{
    public Guid id { get; set; }
    public DateTime Ngay { get; set; }
    public DateTime DenNgay { get; set; }
    public int SoLuongBuoi { get; set; }
    public string NoiDungCongTac { get; set; }
    public string PhuongTienDiChuyen { get; set; }
    public string TruongCongTac { get; set; }
    public string UserNhap { get; set; }
    public string AqUser { get; set; }
}
public class DsNgayCongTacInput
{
    public int SoLuongBuoi { get; set; }
    public string NoiDungCongTac { get; set; }
    public string TruongCongTac { get; set; }
    public string UserNhap { get; set; }
    public string AqUser { get; set; }
}

