import { Component, OnInit } from '@angular/core';
import { HttpClient } from "@angular/common/http";

@Component({
  selector: 'app-setting',
  templateUrl: './setting.component.html',
  styleUrls: ['./setting.component.scss']
})
export class SettingComponent implements OnInit {
  public linkKhaoSat: string
  public noiDung: string
  public ngayBatDau = new Date();
  public ngayKetThuc = new Date();

  constructor(private http: HttpClient) {

  }

  ngOnInit(): void {
    this.http.post<any>("/api/main/view_khao_sat", "").subscribe({
      next: (res: any) => {
        this.linkKhaoSat = res.value
        this.ngayBatDau = new Date(res.ngayBatDau)
        this.ngayKetThuc = new Date(res.ngayKetThuc)
        // Your logic for handling the response
        localStorage.setItem("linkhaosat", res.value)
        this.linkKhaoSat = res.linkKhaoSat;
        this.noiDung = res.noidung;
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
  onNgayBatDauChange(event: Date): void {
    this.ngayBatDau = event;
  }

  onNgayKetThucChange(event: Date): void {
    this.ngayKetThuc = event;
  }
  public updateKhaoSat() {
    this.http.post<any>("/api/main/update_khao_sat", { "id": 1, "linkKhaoSat": this.linkKhaoSat, "noidung": this.noiDung, "ngayBatDau": this.ngayBatDau, "ngayKetThuc": this.ngayKetThuc }).subscribe(
      (res: any) => {
        if (res && res.code === 200) {
          localStorage.setItem("linkhaosat", res.value)
        } else {

        }
      }
    )
  }
}
