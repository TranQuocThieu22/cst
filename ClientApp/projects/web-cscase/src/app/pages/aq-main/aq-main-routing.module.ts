import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AqMainComponent } from './aq-main.component';
import { AuthGuard } from 'projects/libs/src/login/auth.guard';
import { ReportCaNhanComponent } from '../report-ca-nhan/report-ca-nhan.component';
import { NhanSuAqComponent } from '../nhan-su-aq/nhan-su-aq.component';
import { NgayNghiChungComponent } from '../ngay-nghi-chung/ngay-nghi-chung.component';

const routes: Routes = [
  { path: 'aq-main', pathMatch: 'full', redirectTo: 'aq' },
  {
    path: 'aq-main', component: AqMainComponent,
    children: [
      { path: 'aq', component: ReportCaNhanComponent, canActivate: [AuthGuard] },
      { path: 'nhansuaq', component: NhanSuAqComponent, canActivate: [AuthGuard] },
      { path: 'ngaynghichungaq', component: NgayNghiChungComponent, canActivate: [AuthGuard] },
      { path: '', redirectTo: 'aq', pathMatch: 'full' }
    ]
  },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class AqMainRoutingModule { }
