export interface CSCaseDataDO {
  total_items: number;
  total_pages: number;
  is_tfs: boolean;
  data_case: EduCase[];
  data_truong: DataTruong[];
}

export interface EduCase {
  macase: string;
  matruong: string;
  tentruong: string;
  ngaynhan: string;
  chitietyc: string;
  trangthai: string;
  ngaydukien: string;
  loaihopdong: string;
  mucdo: string;
  hieuluc: string;

  dabangiao: string;
  ngayemail: string;
  mailto: string;
  loaicase: string;
  phanhe: string;
  whatnew: string;
  teststate: string;
  thongtinkh: string;
  dapungcongty: string;
  comment: string;

  reviewcase: string;
}

export interface EduCase_Excel {
  macase: string;
  matruong: string;
  ngaynhan: string;
  chitietyc: string;
  trangthai: string;
  // ngaydukien: string;
  loaihopdong: string;
  mucdo: string;
  hieuluc: string;
  dabangiao: string;
  ngayemail: string;
  mailto: string;
  loaicase: string;
  phanhe: string;
  thongtinkh: string;
  discussion: string;
}

export interface DataTruong {
  matruong: string;
  tentruong: string;
}

export interface DataState {
  trangthai: string;
}

export interface DataLoaiCase {
  loaicase: string;
}
export interface DataPhanHe {
  phanhe: string;
}
export interface CaseMetrics {
  weekNumber: number
  SoGioLamViecTrongNgay: number[];
  SoGioLamThieu: number[];
  SoLuongCaseThucHienTrongTuan: number[];
  SoLuotCaseBiMoLai: number[];
  SoGioUocLuongCase: number[];
  SoGioThucTeLamCase: number[];
  SoGioThamGiaMeeting: number[];
  PhanTramTiLeMoCase: number[];
  PhanTramTiLeChenhLechUocLuongVaThucTe: number[];
}