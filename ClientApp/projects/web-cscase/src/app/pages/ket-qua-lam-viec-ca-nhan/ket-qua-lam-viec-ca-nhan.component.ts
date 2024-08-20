import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ConfirmationService, MessageService, PrimeNGConfig } from 'primeng/api';
import { NgxSpinnerService } from "ngx-spinner";
import { CaseMetrics } from 'projects/libs/src/lib/edu.models';
import { Table } from 'primeng/table';
@Component({
  selector: 'app-ket-qua-lam-viec-ca-nhan',
  templateUrl: './ket-qua-lam-viec-ca-nhan.component.html',
  styleUrls: ['./ket-qua-lam-viec-ca-nhan.component.scss']
})
export class KetQuaLamViecCaNhanComponent implements OnInit {
  caseMetricsList: CaseMetrics[] = [];

  public SoGioLamViecTrongNgay: [];
  public SoGioLamThieu;
  public SoLuongCaseThucHienTrongTuan
  public SoLuotCaseBiMoLai
  public dateValue = new Date()
  public SoGioUocLuongCase

  public SoGioThucTeLamCase

  public SoGioThamGiaMeeting
  public PhanTramTiLeMoCase
  public PhanTramTiLeChenhLechUocLuongVaThucTe
  public products
  constructor(
    private https: HttpClient,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private primengConfig: PrimeNGConfig,
    private spinner: NgxSpinnerService
  ) { }

  ngOnInit(): void {
    this.products = {
      code: 1,
      name: "lam",
      category: "asd",
      quantity: 1
    }
    this.primengConfig.ripple = true;
    this.FetchKetQua(this.dateValue.getFullYear())
  }


  FetchKetQua(data) {
    this.spinner.show("spinner-ketqualamvieccanhan");

    const body = {
      user: "tin <AQ\\tin>",
      year: this.dateValue.getFullYear()
    }
    this.https.post<any>("/api/KetQuaLamViecCaNhan/KetQuaLamViecCaNhan", body).subscribe({
      next: (res: any) => {
        if (res && res.code === 200) {
          if (res.data) {
            // const caseMetrics: CaseMetrics = {
            //   SoGioLamViecTrongNgay: res.data.soGioLamViecTrongNgay,
            //   SoGioLamThieu: res.data.soGioLamThieu,
            //   SoLuongCaseThucHienTrongTuan: res.data.soLuongCaseThucHienTrongTuan,
            //   SoLuotCaseBiMoLai: res.data.soLuotCaseBiMoLai,
            //   SoGioUocLuongCase: res.data.soGioUocLuongCase,
            //   SoGioThucTeLamCase: res.data.soGioThucTeLamCase,
            //   SoGioThamGiaMeeting: res.data.soGioThamGiaMeeting,
            //   PhanTramTiLeMoCase: res.data.phanTramTiLeMoCase,
            //   PhanTramTiLeChenhLechUocLuongVaThucTe: res.data.phanTramTiLeChenhLechUocLuongVaThucTe,
            // };
            const dataLength = res.data.soLuongCaseThucHienTrongTuan.length;

            this.caseMetricsList = Array.from({ length: dataLength }, (_, index) => ({
              weekNumber: index + 1,
              SoGioLamThieu: res.data.soGioLamThieu[index] ?? 0,
              SoGioLamViecTrongNgay: res.data.soGioLamViecTrongNgay[index] ?? 0,
              // SoGioLamViecTrongNgay: 0,
              // SoGioLamThieu: 0,
              SoLuongCaseThucHienTrongTuan: res.data.soLuongCaseThucHienTrongTuan[index],
              SoLuotCaseBiMoLai: res.data.soLuotCaseBiMoLai[index],
              SoGioUocLuongCase: res.data.soGioUocLuongCase[index],
              SoGioThucTeLamCase: res.data.soGioThucTeLamCase[index],
              SoGioThamGiaMeeting: res.data.soGioThamGiaMeeting[index],
              PhanTramTiLeMoCase: res.data.phanTramTiLeMoCase[index],
              PhanTramTiLeChenhLechUocLuongVaThucTe: res.data.phanTramTiLeChenhLechUocLuongVaThucTe[index],
            }));
            this.caseMetricsList.sort((a, b) => b.weekNumber - a.weekNumber);

            console.log(dataLength);
            console.log(this.caseMetricsList);

            this.spinner.hide("spinner-ketqualamvieccanhan");

          } else {

          }
        } else {
          this.spinner.hide("spinner-ketqualamvieccanhan");


        }

      },
    })

  }
  clear(table: Table) {
    table.clear();
  }
}
