import { Component, OnInit } from '@angular/core';
import * as FileSaver from 'file-saver';
import { ReportLunchPayment, Member } from './BaoBieuThongKe';

@Component({
  selector: 'app-bao-bieu-thong-ke',
  templateUrl: './bao-bieu-thong-ke.component.html',
  styleUrls: ['./bao-bieu-thong-ke.component.scss']
})
export class BaoBieuThongKeComponent implements OnInit {

  selectedMonthYear: any = null;

  AQLunchPaymentReport: ReportLunchPayment[];
  selectedRecords: any[];
  exportColumns: any[];

  constructor() { }

  ngOnInit(): void {

    // Sample data for AQReportLunchPayment
    this.AQLunchPaymentReport = [
      {
        member: {
          id: 1,
          fullName: "John Doe",
          nickName: "JD"
        },
        total_IndividualDayOff: 2,
        total_WorkingOnline: 15,
        total_CommissionDay: 5,
        total_AQDayOff: 3,
        actual_workingDay: 20
      },
      {
        member: {
          id: 2,
          fullName: "Jane Smithe",
          nickName: "Jm"
        },
        total_IndividualDayOff: 3,
        total_WorkingOnline: 18,
        total_CommissionDay: 4,
        total_AQDayOff: 2,
        actual_workingDay: 21
      }
    ];

  }

  exportExcel() {
    import("xlsx").then(xlsx => {
      const worksheet = xlsx.utils.json_to_sheet(this.AQLunchPaymentReport);
      const workbook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };
      const excelBuffer: any = xlsx.write(workbook, { bookType: 'xlsx', type: 'array' });
      this.saveAsExcelFile(excelBuffer, "lunch_payment_report");
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

}
