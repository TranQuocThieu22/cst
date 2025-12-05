import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AqMainComponent } from './aq-main.component';
import { AuthGuard } from 'projects/libs/src/login/auth.guard';
import { ReportCaNhanComponent } from '../report-ca-nhan/report-ca-nhan.component';
import { NhanSuAqComponent } from '../nhan-su-aq/nhan-su-aq.component';
import { NgayNghiChungComponent } from '../ngay-nghi-chung/ngay-nghi-chung.component';
import { NgayCongTacComponent } from '../ngay-cong-tac/ngay-cong-tac.component';
import { LamViecOnlineComponent } from '../lam-viec-online/lam-viec-online.component';
import { AqReportComponent } from '../aq-report/aq-report.component';
import { LamViecNgoaiGioComponent } from '../lam-viec-ngoai-gio/lam-viec-ngoai-gio.component';
import { KetQuaLamViecCaNhanComponent } from '../ket-qua-lam-viec-ca-nhan/ket-qua-lam-viec-ca-nhan.component';
import { NgayPhepCaNhanComponent } from '../ngay-phep-ca-nhan/ngay-phep-ca-nhan.component';
import { BaoBieuThongKeComponent } from '../bao-bieu-thong-ke/bao-bieu-thong-ke.component';
import { BaoCaoTheoChuKyComponent } from '../bao-cao-theo-chu-ky/bao-cao-theo-chu-ky.component';
import { DanhSachTruongComponent } from '../danh-sach-truong/danh-sach-truong.component';
import { DanhSachAddinComponent } from '../danh-sach-addin/danh-sach-addin.component';
import { TinhNangBaoGiaComponent } from '../tinh-nang-bao-gia/tinh-nang-bao-gia.component';

const routes: Routes = [
  { path: 'aq-main', pathMatch: 'full', redirectTo: 'aq' },
  {
    path: 'aq-main', component: AqMainComponent,
    children: [
      { path: 'aq', component: ReportCaNhanComponent, canActivate: [AuthGuard] },
      { path: 'nhansuaq', component: NhanSuAqComponent, canActivate: [AuthGuard] },
      { path: 'ngaynghichungaq', component: NgayNghiChungComponent, canActivate: [AuthGuard] },
      { path: 'ngaycongtacpaq', component: NgayCongTacComponent, canActivate: [AuthGuard] },
      { path: 'ngayphepcanhan', component: NgayPhepCaNhanComponent, canActivate: [AuthGuard] },
      { path: 'lamvieconline', component: LamViecOnlineComponent, canActivate: [AuthGuard] },
      // { path: 'report', component: AqReportComponent, canActivate: [AuthGuard] },
      { path: 'lamviecngoaigio', component: LamViecNgoaiGioComponent, canActivate: [AuthGuard] },
      { path: 'baobieuthongke', component: BaoBieuThongKeComponent, canActivate: [AuthGuard] },
      { path: 'ketqualamvieccanhan', component: KetQuaLamViecCaNhanComponent, canActivate: [AuthGuard] },
      { path: 'baocaotheochuky', component: BaoCaoTheoChuKyComponent, canActivate: [AuthGuard] },
      { path: 'danhsachtruong', component: DanhSachTruongComponent, canActivate: [AuthGuard] },
      { path: 'danhsachaddin', component: DanhSachAddinComponent, canActivate: [AuthGuard] },
      { path: 'tinhnangbaogia', component: TinhNangBaoGiaComponent, canActivate: [AuthGuard] },
      { path: '', redirectTo: 'aq', pathMatch: 'full' }
    ]
  },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AqMainRoutingModule { }
