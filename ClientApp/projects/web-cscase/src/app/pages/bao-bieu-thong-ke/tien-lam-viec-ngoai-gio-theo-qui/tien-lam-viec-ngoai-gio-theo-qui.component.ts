import { Component, OnInit } from '@angular/core';
import * as FileSaver from 'file-saver';
import { ReportWorkingOTPayment } from './tien-lam-viec-ngoai-gio-DT';
import { HttpClient } from '@angular/common/http';
import { Table } from 'primeng/table';

@Component({
  selector: 'app-tien-lam-viec-ngoai-gio-theo-qui',
  templateUrl: './tien-lam-viec-ngoai-gio-theo-qui.component.html',
  styleUrls: ['./tien-lam-viec-ngoai-gio-theo-qui.component.scss']
})
export class TienLamViecNgoaiGioTheoQuiComponent implements OnInit {

  AQWorkingOTPaymentReport: ReportWorkingOTPayment[];
  quarterList: any[] = [];

  selectedYearInput: any;
  selectedQuarter: any;

  selectedRecords: any[];
  exportColumns: any[];

  constructor(
    private https: HttpClient,
  ) { }

  ngOnInit(): void {
    this.selectedYearInput = new Date();
    this.selectedYearInput.setHours(0, 0, 0, 0);
    this.setQuarterList(this.selectedYearInput.getFullYear());
    this.selectedQuarter = this.getCurrentQuarter();
    this.fetchOTPaymentReport(this.getCurrentQuarter());

    // console.log(this.quarterList);
    // console.log(this.selectedQuarter);

  }

  convertDateFormat(date: string): string {
    const [day, month, year] = date.split('/');
    return `${month}/${day}/${year}`;
  }

  setQuarterList(year: number) {
    this.quarterList = [
      { name: 'Q1', value: { dateFrom: `01/01/${year}`, dateTo: `31/03/${year}` } },
      { name: 'Q2', value: { dateFrom: `01/04/${year}`, dateTo: `30/06/${year}` } },
      { name: 'Q3', value: { dateFrom: `01/07/${year}`, dateTo: `30/09/${year}` } },
      { name: 'Q4', value: { dateFrom: `01/10/${year}`, dateTo: `31/12/${year}` } }
    ];
  }



  getCurrentQuarter() {
    const currentMonth = new Date().getMonth() + 1;
    const currentYear = this.selectedYearInput.getFullYear();

    let result = null;
    if (currentMonth >= 1 && currentMonth <= 3) {
      result = { dateFrom: `01/01/${currentYear}`, dateTo: `31/03/${currentYear}` };
    } else if (currentMonth >= 4 && currentMonth <= 6) {
      result = { dateFrom: `01/04/${currentYear}`, dateTo: `30/06/${currentYear}` };
    } else if (currentMonth >= 7 && currentMonth <= 9) {
      result = { dateFrom: `01/07/${currentYear}`, dateTo: `30/09/${currentYear}` };
    } else {
      result = { dateFrom: `01/10/${currentYear}`, dateTo: `31/12/${currentYear}` };
    }
    return result;
  }

  fetchFilteredOTPaymentReport() {
    this.fetchOTPaymentReport(this.selectedQuarter);
  }


  fetchOTPaymentReport(selectedQuarter?: any) {

    let params: any = {};
    if (selectedQuarter) {
      params.query_dateFrom = this.convertDateFormat(selectedQuarter.dateFrom) + ' 00:00:00';
      params.query_dateTo = this.convertDateFormat(selectedQuarter.dateTo) + ' 00:00:00';
      params.year = this.selectedYearInput.getFullYear();
    }

    this.https.get<any>("/api/LamViecNgoaiGio/ThongKeTienLamViecNgoaiGio", { params: params }).subscribe({
      next: (res: any) => {
        this.AQWorkingOTPaymentReport = res.data;
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
      }
    });
  }


  calculateTotal(type: string) {
    if (!this.AQWorkingOTPaymentReport) return 0;
    let total = 0;
    switch (type) {
      case 'total_sumHours':
        total = this.AQWorkingOTPaymentReport.reduce((acc, report) => acc + report.sumHours, 0);
        break;
      default:
        break;
    }
    return total;
  }

  exportExcel(type: string) {
    let data = [];
    if (type === 'full') {
      data = this.AQWorkingOTPaymentReport;
    }
    else {
      data = this.selectedRecords;
    }

    import("xlsx").then(xlsx => {
      data = data.map(report => {
        return {
          'Full Name': report.fullName,
          'Nick Name': report.nickName,
          'OT sumHours': report.sumHours
        };
      });
      const worksheet = xlsx.utils.json_to_sheet(data);
      const workbook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };
      const excelBuffer: any = xlsx.write(workbook, { bookType: 'xlsx', type: 'array' });
      this.saveAsExcelFile(excelBuffer, "workingOT_payment_report");
    });
  }

  saveAsExcelFile(buffer: any, fileName: string): void {
    let EXCEL_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;charset=UTF-8';
    let EXCEL_EXTENSION = '.xlsx';
    const data: Blob = new Blob([buffer], {
      type: EXCEL_TYPE
    });
    FileSaver.saveAs(data, fileName + '_export_' + new Date().getTime() + EXCEL_EXTENSION);
  }


  clear(table: Table) {
    table.clear();
    this.selectedYearInput = new Date();
    this.selectedQuarter = this.getCurrentQuarter();
    this.fetchOTPaymentReport(this.selectedQuarter);
  }
}

