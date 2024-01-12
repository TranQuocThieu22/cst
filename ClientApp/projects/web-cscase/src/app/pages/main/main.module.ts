import { CUSTOM_ELEMENTS_SCHEMA, NgModule, NO_ERRORS_SCHEMA } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MainRoutingModule } from './main-routing.module';
import { MainComponent } from './main.component';
import { NgxSpinnerModule } from 'ngx-spinner';
import { CommonModule, HashLocationStrategy, LocationStrategy } from '@angular/common';
import { SharedModule } from '../../shared/shared.module';
import { NgSelectModule } from '@ng-select/ng-select';
import { UploadComponent } from '../upload/upload.component';
import { ModalModule } from 'ngx-bootstrap/modal';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { NgxPrinterModule } from 'ngx-printer';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';
import { JwtInterceptor, LibsModule } from '@mylibs';
import { NgApexchartsModule } from 'ng-apexcharts';
import { CsCaseComponent } from '../cs-case/cs-case.component';
import { DataServices } from '../../service/dataservices.service';
import { TransTextService } from '../../service/trans-text.service';
import { UploadFilesService } from '../../service/upload-files.service';
import { Error404Component } from '../error404/error404.component';
import { ReleaseComponent } from '../release/release.component';
import { ThongkeComponent } from '../thongke/thongke.component';
import { TooltipModule } from 'ngx-bootstrap/tooltip';
import { ReleaseListComponent } from '../release-list/release-list.component';
import { EditRlcaseComponent } from '../release-list/edit-rlcase/edit-rlcase.component';
import { TraodoiComponent } from '../traodoi/traodoi.component';
import { ChitietcaseComponent } from '../chitietcase/chitietcase.component';

@NgModule({
  declarations: [MainComponent, CsCaseComponent, ThongkeComponent,
    UploadComponent, ReleaseComponent, ReleaseListComponent, ChitietcaseComponent,
    EditRlcaseComponent, TraodoiComponent, Error404Component],
  imports: [
    FormsModule,
    CommonModule,
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
    ModalModule.forRoot(),
    PaginationModule.forRoot(),
    TooltipModule.forRoot(),
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
export class MainModule { }
