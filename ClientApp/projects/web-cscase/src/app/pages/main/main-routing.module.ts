import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from '@mylibs';
import { MainComponent } from './main.component';
import { CsCaseComponent } from '../../pages/cs-case/cs-case.component';
import { ThongkeComponent } from '../thongke/thongke.component';
import { UploadComponent } from '../../pages/upload/upload.component';
import { Error404Component } from '../../pages/error404/error404.component';
import { ReleaseComponent } from '../release/release.component';
import { ReleaseListComponent } from '../release-list/release-list.component';
import { SettingComponent } from '../setting/setting.component';

const routes: Routes = [
  { path: 'main', pathMatch: 'full', redirectTo: 'cscase' },
  {
    path: 'main', component: MainComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'cscase' },
      { path: 'cscase', component: CsCaseComponent, canActivate: [AuthGuard] },
      { path: 'thongke', component: ThongkeComponent, canActivate: [AuthGuard] },
      { path: 'upload', component: UploadComponent, canActivate: [AuthGuard], data: { roles: 'admin' } },
      { path: 'release', component: ReleaseComponent, canActivate: [AuthGuard], data: { roles: 'admin' } },
      { path: 'releasedlist', component: ReleaseListComponent, canActivate: [AuthGuard] },
      { path: 'setting', component: SettingComponent, canActivate: [AuthGuard] },
      {
        path: 'taocase', loadChildren: () => import('../taocase/taocase.module')
          .then(m => m.TaocaseModule), canActivate: [AuthGuard],
      },
      { path: '**', component: Error404Component },
    ]
  }
]

@NgModule({
  imports: [RouterModule.forChild(routes), FormsModule],
  exports: [RouterModule]
})

export class MainRoutingModule { }
