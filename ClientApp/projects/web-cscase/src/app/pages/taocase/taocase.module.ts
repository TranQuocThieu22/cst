import { NgModule } from '@angular/core';
import { CommonModule, HashLocationStrategy, LocationStrategy } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgxSpinnerModule } from 'ngx-spinner';
import { NgSelectModule } from '@ng-select/ng-select';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { JwtInterceptor } from '@mylibs';
import { CKEditorModule } from '@ckeditor/ckeditor5-angular';
import { TaocaseRoutingModule } from './taocase-routing.module';
import { TaocaseComponent } from './taocase.component';
import { SharedModule } from '../../shared/shared.module';

@NgModule({
  declarations: [TaocaseComponent],
  imports: [
    FormsModule,
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    CKEditorModule,
    NgxSpinnerModule,
    NgSelectModule,
    TaocaseRoutingModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: LocationStrategy, useClass: HashLocationStrategy },
  ]
})
export class TaocaseModule { }
