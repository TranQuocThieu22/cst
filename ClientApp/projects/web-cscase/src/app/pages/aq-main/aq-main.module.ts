import { CUSTOM_ELEMENTS_SCHEMA, NgModule, NO_ERRORS_SCHEMA } from '@angular/core';
import { CommonModule, HashLocationStrategy, LocationStrategy } from '@angular/common';

import { AqMainRoutingModule } from './aq-main-routing.module';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { LibsModule, JwtInterceptor } from '@mylibs';
import { NgSelectModule } from '@ng-select/ng-select';
import { NgApexchartsModule } from 'ng-apexcharts';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { NgxPrinterModule } from 'ngx-printer';
import { NgxSpinnerModule } from 'ngx-spinner';
import { SharedModule } from 'primeng/api';
import { DataServices } from '../../service/dataservices.service';
import { TransTextService } from '../../service/trans-text.service';
import { UploadFilesService } from '../../service/upload-files.service';
import { ChitietcaseComponent } from '../chitietcase/chitietcase.component';
import { CsCaseComponent } from '../cs-case/cs-case.component';
import { Error404Component } from '../error404/error404.component';
import { MainRoutingModule } from '../main/main-routing.module';
import { MainComponent } from '../main/main.component';
import { EditRlcaseComponent } from '../release-list/edit-rlcase/edit-rlcase.component';
import { ReleaseListComponent } from '../release-list/release-list.component';
import { ReleaseComponent } from '../release/release.component';
import { ThongkeComponent } from '../thongke/thongke.component';
import { TraodoiComponent } from '../traodoi/traodoi.component';
import { UploadComponent } from '../upload/upload.component';
import { AqMainComponent } from './aq-main.component';
import { ReportCaNhanComponent } from '../report-ca-nhan/report-ca-nhan.component';
import { NhanSuAqComponent } from '../nhan-su-aq/nhan-su-aq.component';


@NgModule({
  declarations: [AqMainComponent, ReportCaNhanComponent, NhanSuAqComponent],
  imports: [
    FormsModule,
    ReactiveFormsModule,
    MainRoutingModule,
    SharedModule,
    HttpClientModule,
    LibsModule,
    NgxSpinnerModule,
    NgApexchartsModule,
    NgSelectModule,
    BsDatepickerModule.forRoot(),
    NgxPrinterModule.forRoot({ timeToWaitRender: 500 }),
    CommonModule,
    AqMainRoutingModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: LocationStrategy, useClass: HashLocationStrategy },
    DataServices, TransTextService, UploadFilesService
  ],
  entryComponents: [MainComponent, CsCaseComponent, ThongkeComponent,
    UploadComponent, ReleaseComponent, ReleaseListComponent,
    EditRlcaseComponent, TraodoiComponent, Error404Component]
})
export class AqMainModule { }
