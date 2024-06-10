import { Component, OnInit } from '@angular/core';
import { EduCase } from '@mylibs';
import { HttpClient } from '@angular/common/http';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-thongbaokhaosat',
  templateUrl: './thongbaokhaosat.component.html',
  styleUrls: ['./thongbaokhaosat.component.scss']
})
export class ThongbaokhaosatComponent implements OnInit {
  public linkKhaoSat: string = '';
  public noiDung: string = ''
  public isChecked: boolean = false;
  public data: EduCase[]; public type: number;
  public datacase: Partial<EduCase> = {};
  constructor(public bsModalRefEditor: BsModalRef, private https: HttpClient) { }

  ngOnInit(): void {
    if (this.data && this.data.length > 0) {
      this.data[0].thongtinkh = this.data[0].thongtinkh ?? '';
      this.data[0].dapungcongty = this.data[0].dapungcongty ?? ''
      this.datacase = this.data[0];
    }
    this.https.post<any>("/api/main/view_khao_sat", "").subscribe({
      next: (res: any) => {
        console.log(res);

        this.linkKhaoSat = res.linkKhaoSat
        this.noiDung = res.noidung
        // Your logic for handling the response
        localStorage.setItem("linkhaosat", res.linkKhaoSat)
        localStorage.setItem("noidung", res.noidung)
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
      }
    });
  }

  onCheckboxChange(event: any) {
    this.isChecked = event.target.checked;
    console.log(this.isChecked);
  }
  dongThongBao() {
    if (this.isChecked) {
      this.https.post<any>("/api/main/them_truong_da_khao_sat", { "tenTruong": JSON.parse(sessionStorage.getItem("current-user")).maTruong }).subscribe({
        next: (res: any) => {
          console.log(res);
        },
      });
    }
    this.bsModalRefEditor.hide()
  }
}
