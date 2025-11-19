export interface SchoolDataApiDTO {
    idTruong: string;
    maTruong: string;
    tenTruong: string;
    ngayHetHan: string;
    thoiDiemTrienKhai: string;
}

export interface SchoolDataApiResult {
    data: SchoolDataApiDTO[];
    result: boolean;
    code: number;
    message: string;
}

export interface AddinSchoolDataApiDTO {
    idAddin: string;
    idAddinParent: string;
    maAddin: string;
    tenAddin: string;
    ghiChuAddin: string;
    isDaMua: boolean;
    thongTinDaMua: {
        ngayMua: Date;
        userCapAddin: string;
        ghiChuCapAddin: string;
        danhSachMaDVPC: string;
    }
}

export interface AddinSchoolDataApiResult {
    data: AddinSchoolDataApiDTO[];
    result: boolean;
    code: number;
    message: string;
}

export interface AddinSchoolInput {
    idTruong: string;
}

export interface ContactPerson {
    hoTen: string;
    dienThoai: string;
    email: string;
}

export interface AddinModule {
    ghiChuSale: string;
    ghiChuSupport: string;
    ghiChuDev: string;
}

export interface LuuYDacThu {
    supportGhiChuMoHinh: string;
    supportGhiChuCachHoTro: string;
    devGhiChu: string;
    saleGhiChu: string;
}

export interface ThongTinServer {
    nguoiQuanLy: string;
    thongTinChung: string;
    ghiChu: string;
}

export interface SchoolProfileDTO {
    idTruong: string;
    maTruong: string;
    tenTruong: string;
    diaChiTruong: string;
    hieuTruong: ContactPerson;
    hieuPho: ContactPerson;
    truongPhongDaoTao: ContactPerson;
    truongPhongKhaoThi: ContactPerson;
    truongPhongTaiVu: ContactPerson;
    admin: ContactPerson;
    ghiChuKinhDoanh: string;
    ghiChuKyThuat: string;
    ghiChuChamSoc: string;
    danhSachAddin: AddinModule;
    luuYXuLyDacThu: LuuYDacThu;
    serverInfo: ThongTinServer;
}

export interface SchoolProfileInsertDTO {
    idTruong: string;
    maTruong: string;
    tenTruong: string;
    diaChiTruong: string;
    hieuTruong: ContactPerson;
    hieuPho: ContactPerson;
    truongPhongDaoTao: ContactPerson;
    truongPhongKhaoThi: ContactPerson;
    truongPhongTaiVu: ContactPerson;
    admin: ContactPerson;
    ghiChuKinhDoanh: string;
    ghiChuKyThuat: string;
    ghiChuChamSoc: string;
    danhSachAddin: AddinModule;
    luuYXuLyDacThu: LuuYDacThu;
    serverInfo: ThongTinServer;
}

export interface SchoolProfileUpdateDTO {
    diaChiTruong: string;
    hieuTruong: ContactPerson;
    hieuPho: ContactPerson;
    truongPhongDaoTao: ContactPerson;
    truongPhongKhaoThi: ContactPerson;
    truongPhongTaiVu: ContactPerson;
    admin: ContactPerson;
    ghiChuKinhDoanh: string;
    ghiChuKyThuat: string;
    ghiChuChamSoc: string;
    danhSachAddin: AddinModule;
    luuYXuLyDacThu: LuuYDacThu;
    serverInfo: ThongTinServer;
}

export interface ApiResultBaseDO {
    message: string;
    code: number;
    result: boolean;
}

export interface SchoolProfileInsertResultDTO extends ApiResultBaseDO {
    data: SchoolProfileInsertDTO[];
    numberOfNewRecord: number;
}

export interface SchoolProfileResultDTO extends ApiResultBaseDO {
    data: SchoolProfileDTO[];
}

export interface SchoolProfileUpdateResultDTO extends ApiResultBaseDO {
    data: SchoolProfileUpdateDTO;
}
