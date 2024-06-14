import { Component, OnInit } from '@angular/core';
import { HttpClient } from "@angular/common/http";
import { Editor } from 'ngx-editor';
import { ToastrService } from 'ngx-toastr';
@Component({
  selector: 'app-setting',
  templateUrl: './setting.component.html',
  styleUrls: ['./setting.component.scss']
})
export class SettingComponent implements OnInit {
  public linkKhaoSat: string
  public ngayBatDau = new Date();
  public ngayKetThuc = new Date();
  public editor: Editor;
  public html = '';

  constructor(private http: HttpClient, private toastr: ToastrService) {

  }
  ngOnDestroy(): void {
    this.editor.destroy();
  }
  ngOnInit(): void {
    this.editor = new Editor();
    this.http.post<any>("/api/main/view_khao_sat", "").subscribe({
      next: (res: any) => {
        this.linkKhaoSat = res.value
        this.ngayBatDau = new Date(res.ngayBatDau)
        this.ngayKetThuc = new Date(res.ngayKetThuc)
        // Your logic for handling the response
        localStorage.setItem("linkhaosat", res.value)
        this.linkKhaoSat = res.linkKhaoSat;
        this.html = res.noidung
        console.log(res.noidung);

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
    this.http.post<any>("/api/main/update_khao_sat", { "id": 1, "linkKhaoSat": this.linkKhaoSat, "noidung": this.html, "ngayBatDau": this.ngayBatDau, "ngayKetThuc": this.ngayKetThuc }).subscribe(
      (res: any) => {
        localStorage.setItem("linkhaosat", res.value)
        this.toastr.success("Lưu thành công")
      }
    )
  }
}
