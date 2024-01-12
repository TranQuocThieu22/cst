import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TaocaseComponent } from './taocase.component';

const routes: Routes = [
  { path: '', pathMatch: 'full', component: TaocaseComponent },
  { path: ':id', component: TaocaseComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes), FormsModule],
  exports: [RouterModule]
})
export class TaocaseRoutingModule { }
