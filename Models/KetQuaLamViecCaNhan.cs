using System.Collections.Generic;

namespace educlient.Models
{
    public class KetQuaLamViecCaNhanResult : ApiResultBaseDO
    {
        public KetQuaLamViecCaNhanDataDO data { get; set; }
    }
    public class KetQuaLamViecCaNhanDataDO
    {
        public List<int> SoGioLamViecTrongNgay { get; set; }
        public List<int> SoLuongCaseThucHienTrongTuan { get; set; }
        public List<int> SoLuotCaseBiMoLai { get; set; }
        public List<float> SoGioUocLuongCase { get; set; }
        public List<float> SoGioThucTeLamCase { get; set; }
        public List<float> SoGioThamGiaMeeting { get; set; }
        public List<float> PhanTramTiLeMoCase { get; set; }
        public List<float> PhanTramTiLeChenhLechUocLuongVaThucTe { get; set; }
        public List<float> SoGioLamThieu { get; set; }
    }
    public class KetQuaLamViecCaNhanInput
    {
        public string user { get; set; }
        public int year { get; set; }
    }
}
