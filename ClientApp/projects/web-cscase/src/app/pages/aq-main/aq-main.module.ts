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
import { NgayNghiChungComponent } from '../ngay-nghi-chung/ngay-nghi-chung.component';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { CalendarModule } from 'primeng/calendar';
import { SliderModule } from 'primeng/slider';
import { MultiSelectModule } from 'primeng/multiselect';
import { ContextMenuModule } from 'primeng/contextmenu';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { ProgressBarModule } from 'primeng/progressbar';
import { InputTextModule } from 'primeng/inputtext';
import { FileUploadModule } from 'primeng/fileupload';
import { ToolbarModule } from 'primeng/toolbar';
import { RatingModule } from 'primeng/rating';
import { RadioButtonModule } from 'primeng/radiobutton';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { DividerModule } from 'primeng/divider';
import { CheckboxModule } from 'primeng/checkbox';
import { SidebarModule } from 'primeng/sidebar';
import { PanelMenuModule } from 'primeng/panelmenu';
import { ConfirmPopupModule } from 'primeng/confirmpopup';
import { ConfirmationService, MessageService } from "primeng/api";
import { AvatarModule } from "primeng/avatar";
import { ChartModule } from 'primeng/chart';
import { EchartsxModule } from 'echarts-for-angular';

@NgModule({
  declarations: [AqMainComponent, ReportCaNhanComponent, NhanSuAqComponent, NgayNghiChungComponent],
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
    AqMainRoutingModule,
    TableModule,
    ButtonModule,
    TableModule,
    CalendarModule,
    SliderModule,
    DialogModule,
    MultiSelectModule,
    ContextMenuModule,
    DropdownModule,
    ButtonModule,
    ToastModule,
    InputTextModule,
    ProgressBarModule,
    HttpClientModule,
    FileUploadModule,
    ToolbarModule,
    RatingModule,
    CheckboxModule,
    FormsModule,
    RadioButtonModule,
    InputNumberModule,
    InputTextareaModule,
    DividerModule,
    SidebarModule,
    PanelMenuModule,
    ConfirmPopupModule,
    AvatarModule,
    ChartModule,
    EchartsxModule
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA, NO_ERRORS_SCHEMA],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true },
    { provide: LocationStrategy, useClass: HashLocationStrategy },
    DataServices, TransTextService, UploadFilesService, ConfirmationService,
    MessageService
  ],
  entryComponents: [MainComponent, CsCaseComponent, ThongkeComponent,
    UploadComponent, ReleaseComponent, ReleaseListComponent,
    EditRlcaseComponent, TraodoiComponent, Error404Component]
})
export class AqMainModule { }
