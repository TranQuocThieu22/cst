const PROXY_CONFIG = [
  {
    context: ["/"],
    // target: "https://cst.aqtech.vn",
    target: "http://localhost:54383/", // code MainController.cs
    // target: "https://cst.aqtech.vn:4244/#/",
    secure: false,
    changeOrigin: true,
    logLevel: "debug",
    headers: { host: "localhost" },
    timeout: 60000,
    cookieDomainRewrite: {
      localhost: "localhost",
    },
  },
];
module.exports = PROXY_CONFIG;

('[{"macase":40598,"matruong":"DHLN","ngaynhan":"2024-01-02T08:26:00.537","chitietyc":"Lỗi xuất HDDT","trangthai":"Đóng case","ngaydukien":"2024-01-05T00:00:00","loaihopdong":"03 - HĐ bảo trì","mucdo":"3 - Độ ưu tiên trung bình","hieuluc":"RL tuần thứ 2 tháng 1/2024","dabangiao":"X","ngayemail":"2024-01-02T15:11:54.78","mailto":"ntphu@vnuf2.edu.vn","loaicase":"CV - Trao đổi hoặc chưa phân loại","phanhe":"CSHP","comment":null,"tinhnangmoi":null},{"macase":40600,"matruong":"TDDM","ngaynhan":"2024-01-02T09:55:19.987","chitietyc":"Bổ sung hướng dẫn sử dụng trên phân hệ NHÂN SỰ.","trangthai":"Đóng case","ngaydukien":"2024-02-14T09:54:00","loaihopdong":"03 - HĐ bảo trì","mucdo":"3 - Độ ưu tiên trung bình","hieuluc":"RL tuần thứ 4 tháng 2/2024","dabangiao":"X","ngayemail":"2024-06-21T17:59:31.393","mailto":"admin-edu@tdmu.edu.vn","loaicase":"NF - Yêu cầu mới cần sửa code","phanhe":"QLNS","comment":null,"tinhnangmoi":null},{"macase":40601,"matruong":"DHVB","ngaynhan":"2024-01-02T10:26:35.663","chitietyc":"Xếp loại ...');
