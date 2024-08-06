import { Component, OnInit } from '@angular/core';
import { Car } from './report-ca-nhan.interface';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { User } from '@mylibs';

@Component({
  selector: 'app-report-ca-nhan',
  templateUrl: './report-ca-nhan.component.html',
  styleUrls: ['./report-ca-nhan.component.scss']
})
export class ReportCaNhanComponent implements OnInit {
  public user: string;
  public hoVoTen: string;
  public phongBan: string;
  public NgayBatDau: string;
  public tongSoNgayLamViecTrongTuan: number;
  public tongSoGioPhaiLamViecTrongNgay: number;
  public tongSoNgayNghiPhep: number;
  public ThongTinNghiPhep: object[];
  public Cars: Car[];
  public currentSession: any;
  public currentUser: any;
  constructor(private router: Router) {
    this.currentSession = new BehaviorSubject<User>(JSON.parse(sessionStorage.getItem('current-user')));
    this.currentUser = this.currentSession._value
    console.log(this.currentUser);

    this.user = this.currentUser.user;
    // this.user = 'minhlam';
    this.phongBan = this.currentUser.group
      .filter(item => item !== 'users')
      .join(', ');

    console.log(this.phongBan);

    this.NgayBatDau = '1/7/2023';
    this.tongSoGioPhaiLamViecTrongNgay = 7
    this.tongSoNgayLamViecTrongTuan = 5
    this.tongSoNgayNghiPhep = 3
    this.ThongTinNghiPhep = [
      {
        id: 1,
        ngay: '31/7/2024',
        soLuongNgay: 2,
        lyDo: 'Nghỉ do việc cá nhân',
        loaiNgay: 'Phép cá nhân',
        user: this.user,
        trangThai: 'Đã duyệt',
        ghiChu: 'Đây là ghi chú'
      },
      {
        id: 2,
        ngay: '20/7/2024',
        soLuongNgay: 1,
        lyDo: 'Công ty đi team building',
        loaiNgay: 'Phép chung',
        user: this.user,
        trangThai: 'Chờ duyệt',
        ghiChu: 'Đây là ghi chú'
      }
      ,
      {
        id: 3,
        ngay: '1/7/2024',
        soLuongNgay: 3,
        lyDo: 'Công ty đi team building',
        loaiNgay: 'Phép chung',
        user: this.user,
        trangThai: 'Từ chối',
        ghiChu: 'Đây là ghi chú'
      }
    ]
  }

  ngOnInit(): void {
  }
  public NavigateNhanSuAq() {
    this.router.navigate(['main/nhansuaq']);
  }

}
