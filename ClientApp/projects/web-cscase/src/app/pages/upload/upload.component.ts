import { Component, OnInit } from '@angular/core';
import * as XLSX from 'xlsx';
import { HttpClient } from '@angular/common/http';
import { NgxSpinnerService } from 'ngx-spinner';
import { EduCase_Excel } from '@mylibs';

@Component({
  selector: 'app-upload',
  templateUrl: './upload.component.html',
  styleUrls: ['./upload.component.scss']
})

export class UploadComponent implements OnInit {
  public FileName = '';
  private dataFile: any;
  private validateFile = false;
  public messError = '';
  public messOK = '';
  public xlsData_Error: EduCase_Excel[] = [];

  constructor(private http: HttpClient, public spinner: NgxSpinnerService) { }

  public ngOnInit(): void {
    this.messError = '';
    this.messOK = '';
    this.xlsData_Error = [];
  }

  public getFile(ev) {
    if (ev) {
      this.spinner.show('up');
      this.messError = '';
      this.messOK = '';
      this.dataFile = null;
      this.validateFile = false;
      const reader = new FileReader();
      const file = ev.target.files[0];
      reader.onload = (event) => {
        if (event) {
          this.FileName = file.name;
          if (this.FileName.lastIndexOf('.xlsx') < 0 || this.FileName.lastIndexOf('.xls') < 0) {
            this.dataFile = null;
            this.validateFile = false;
            this.spinner.hide('up');
            this.messError = 'Vui lòng chọn File excel';
          }
          else {
            this.dataFile = reader.result;
            this.validateFile = true;
            this.spinner.hide('up');
          }
        }
      };
      reader.readAsBinaryString(file);
      this.spinner.hide('up');
    }
  }

  public UploadFile() {
    this.spinner.show('up');
    this.messError = '';
    this.messOK = '';
    this.xlsData_Error = [];

    if (!this.FileName || this.FileName === '') {
      this.spinner.hide('up');
      this.messError = 'Vui lòng chọn File excel để upload';
      return;
    }

    if (this.dataFile && this.validateFile) {

      let workBook = null;
      let WorkSheet = null;
      let xlsData_Goc: EduCase_Excel[] = [];
      let xlsData_OK: EduCase_Excel[] = [];
      workBook = XLSX.read(this.dataFile, { type: 'binary', cellDates: true, dateNF: 'dd/MM/yyyy;@' });
      WorkSheet = workBook.Sheets[workBook.SheetNames[0]];
      xlsData_Goc = XLSX.utils.sheet_to_json(WorkSheet, { raw: false });
      if (xlsData_Goc && xlsData_Goc.length > 0) {
        xlsData_Goc = xlsData_Goc.map((v) => ({
          macase: v.macase ? v.macase.trim() : '',
          matruong: v.matruong ? v.matruong.trim() : '',
          ngaynhan: v.ngaynhan ? v.ngaynhan.trim() : '',
          chitietyc: v.chitietyc ? v.chitietyc.trim() : '',
          trangthai: v.trangthai ? v.trangthai.trim() : '',
          loaicase: v.loaicase ? v.loaicase.trim() : '',
          phanhe: v.phanhe ? v.phanhe.trim() : '',
          // ngaydukien: v.ngaydukien ? v.ngaydukien.trim() : '',
          loaihopdong: v.loaihopdong ? v.loaihopdong.trim() : '',
          mucdo: v.mucdo ? v.mucdo.trim() : '',
          hieuluc: v.hieuluc ? v.hieuluc.trim() : '',
          dabangiao: v.dabangiao ? v.dabangiao.trim() : '',
          ngayemail: v.ngayemail ? v.ngayemail.trim() : '',
          mailto: v.mailto ? v.mailto.trim() : '',
          thongtinkh: v.thongtinkh ? v.thongtinkh.trim() : '',
          discussion: v.discussion ? v.discussion.trim() : '',
        }));

        xlsData_OK = xlsData_Goc.filter(s => (s.macase && Number(s.macase) > 0) && s.matruong && s.ngaynhan);

        const reg = /^(0[1-9]|[1,2][0-9]|3[0-1])\/(0[1-9]|1[0-2])\/(2(0(2[0-9]|[3-9][0-9])|[1-9][0-9]{3})|[3-9][0-9]{3})$/;

        // file OK
        xlsData_OK = xlsData_OK.filter(s => s.macase && s.matruong
          && s.ngaynhan && reg.test(s.ngaynhan)
          // && (!s.ngaydukien || reg.test(s.ngaydukien))
          && (!s.ngayemail || reg.test(s.ngayemail))
        );

        // File error
        this.xlsData_Error = xlsData_Goc.filter(s => !(s.macase && s.matruong
          && s.ngaynhan && reg.test(s.ngaynhan)
          // && (!s.ngaydukien || reg.test(s.ngaydukien))
          && (!s.ngayemail || reg.test(s.ngayemail)))
        );
      }
      else {
        xlsData_OK = [{
          macase: '', matruong: '', ngaynhan: '', chitietyc: '', trangthai: '', loaicase: '', phanhe: '',
          //  ngaydukien: '',
          loaihopdong: '', mucdo: '',
          hieuluc: '', dabangiao: '', ngayemail: '', mailto: '', thongtinkh: '', discussion: '',
        }];
      }

      this.spinner.show('up');
      this.http.post('api/main/upload', xlsData_OK)
        .subscribe(res => {
          if (res) {
            this.spinner.hide('up');
            this.messOK = 'Đã Upload thành công: ' + (xlsData_Goc && xlsData_Goc.length > 0 ? xlsData_OK.length : 0) + ' rows.';
            if (this.xlsData_Error.length > 0) {
              this.messError = 'Rows error: ' + this.xlsData_Error.length;
            }
          }
          else {
            this.spinner.hide('up');
            this.messError = 'Có lỗi phát sinh khi Upload';
          }
        }, error => {
          this.spinner.hide('up');
          this.messError = 'Có lỗi phát sinh khi Upload';
        });
    }
    setTimeout(() => { this.spinner.hide('up'); }, 20000);

  }

  public exportfileCauTruc() {
    const element = document.getElementById('excel-table');
    const ws: XLSX.WorkSheet = XLSX.utils.table_to_sheet(element);
    const wb: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Sheet1');
    XLSX.writeFile(wb, 'FileCauTruc.xlsx');
  }

  public ExportFileError() {
    const element = document.getElementById('excel-table');
    const ws: XLSX.WorkSheet = XLSX.utils.table_to_sheet(element);
    const wb: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Sheet1');
    XLSX.writeFile(wb, 'File_Error_CS_CASE.xlsx');
  }

  public trackByFn(index: number) { return index; }
}
