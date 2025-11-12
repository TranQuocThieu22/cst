export interface DanhSachAddinDTO {
    idAddin: string;
    idAddinParent: string;
    maAddin: string;
    tenAddin: string;
    ghiChuAddin: string;
    isDaMua: boolean;
    ghiChuSale: string;
    ghiChuDev: string;
    ghiChuSupport: string;
    donGia: number;
}

export interface DanhSachAddinResultDTO {
    data: DanhSachAddinDTO[];
    result: boolean;
    code: number;
    message: string;
}