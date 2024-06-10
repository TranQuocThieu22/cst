import { CUSTOM_ELEMENTS_SCHEMA, NgModule, NO_ERRORS_SCHEMA } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { AppRoutingModule } from './app-routing.module';
import { CommonModule, HashLocationStrategy, LocationStrategy } from '@angular/common';
import { NgxSpinnerModule } from 'ngx-spinner';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { ToastrModule } from 'ngx-toastr';
import { ModalModule } from 'ngx-bootstrap/modal';
import { SharedModule } from './shared/shared.module';
import { ErrorInterceptor, JwtInterceptor, LibsModule } from '@mylibs';
import { MainModule } from './pages/main/main.module';
import { CKEditorModule } from '@ckeditor/ckeditor5-angular';
import { NgApexchartsModule } from 'ng-apexcharts';

import { LoginComponent } from './pages/login/login.component';
import { AppComponent } from './app.component';
import { NoidungcscaseComponent } from './pages/noidungcscase/noidungcscase.component';
import { MessBoxComponent } from './service/mess-box/mess-box.component';
import { SettingComponent } from './pages/setting/setting.component';
import { NoidungcasenewComponent } from './pages/noidungcasenew/noidungcasenew.component';
import { ThongbaokhaosatComponent } from './pages/thongbaokhaosat/thongbaokhaosat.component';

@NgModule({
  declarations: [AppComponent, LoginComponent, NoidungcscaseComponent, MessBoxComponent, SettingComponent, NoidungcasenewComponent, ThongbaokhaosatComponent],
  imports: [
    BrowserAnimationsModule,
    BrowserModule,
    FormsModule,
    CommonModule,
    ReactiveFormsModule,
    HttpClientModule,
    AppRoutingModule,
    CKEditorModule,
    SharedModule,
    MainModule,
    LibsModule,
    NgxSpinnerModule,
    BsDatepickerModule.forRoot(),
    ModalModule.forRoot(),
    NgApexchartsModule,
    ToastrModule.forRoot({
      timeOut: 10000,
      positionClass: 'toast-bottom-right',
      closeButton: true,
      tapToDismiss: true,
    }),
  ],

  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    // { provide: LOCALE_ID, useValue: 'vi-VN' },
    { provide: LocationStrategy, useClass: HashLocationStrategy },
  ],

  bootstrap: [AppComponent],
  entryComponents: [AppComponent, LoginComponent, NoidungcscaseComponent, MessBoxComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA]
})
export class AppModule { }
