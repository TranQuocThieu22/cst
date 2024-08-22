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
      },
      {
        member: {
          id: 3,
          fullName: "John Doe 2",
          nickName: "JD 2"
        },
        total_IndividualDayOff: 2,
        total_WorkingOnline: 15,
        total_CommissionDay: 5,
        total_AQDayOff: 3,
        actual_workingDay: 20
      },
      {
        member: {
          id: 4,
          fullName: "Jane Smithe 3",
          nickName: "Jm 3"
        },
        total_IndividualDayOff: 3,
        total_WorkingOnline: 18,
        total_CommissionDay: 4,
        total_AQDayOff: 2,
        actual_workingDay: 21
      }
    ];

  }

  showSelectedRecords() {
    console.log(this.selectedRecords);
  }

  exportExcel(type: string) {
    let data = [];
    if (type === 'full') {
      data = this.AQLunchPaymentReport;
    }
    else {
      data = this.selectedRecords;
    }

    import("xlsx").then(xlsx => {
      data = data.map(report => {
        return {
          'Member ID': report.member.id,
          'Full Name': report.member.fullName,
          'Nick Name': report.member.nickName,
          'Total Individual Day Off': report.total_IndividualDayOff,
          'Total Working Online': report.total_WorkingOnline,
          'Total Commission Day': report.total_CommissionDay,
          'Total AQ Day Off': report.total_AQDayOff,
          'Actual Working Day': report.actual_workingDay
        };
      });
      const worksheet = xlsx.utils.json_to_sheet(data);
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
