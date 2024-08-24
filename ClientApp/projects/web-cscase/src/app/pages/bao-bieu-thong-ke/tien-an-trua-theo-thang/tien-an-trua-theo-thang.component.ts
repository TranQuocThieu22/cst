import { Component, OnInit } from '@angular/core';
import * as FileSaver from 'file-saver';
import { HttpClient } from '@angular/common/http';
import { LunchPaymentReport } from './tien-an-trua-DT'
@Component({
  selector: 'app-tien-an-trua-theo-thang',
  templateUrl: './tien-an-trua-theo-thang.component.html',
  styleUrls: ['./tien-an-trua-theo-thang.component.scss']
})
export class TienAnTruaTheoThangComponent implements OnInit {

  selectedMonthYear: Date;

  AQLunchPaymentReport: LunchPaymentReport[];
  selectedRecords: any[];
  exportColumns: any[];

  summary_lunch_report: any = {}


  constructor(
    private https: HttpClient
  ) { }

  ngOnInit(): void {
    this.selectedMonthYear = new Date();
    this.selectedMonthYear.setHours(0, 0, 0, 0);
    this.fetchLunchPaymentReport(this.selectedMonthYear);

  }
  fetchFilteredLunchPaymentReport() {
    this.fetchLunchPaymentReport(this.selectedMonthYear);
  }


  fetchLunchPaymentReport(selectedMonthYear?: any) {
    let params: any = {};
    if (selectedMonthYear) {
      params = {
        month: selectedMonthYear.getMonth() + 1,
        year: selectedMonthYear.getFullYear()
      };
    }

    this.https.get<any>("/api/BaoBieuThongKe/ThongKeTinhTienAnTrua", { params: params }).subscribe({
      next: (res: any) => {
        this.AQLunchPaymentReport = res.data;
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.handleAfterFetchLunchReportData();
      }
    });
  }

  async handleAfterFetchLunchReportData(): Promise<void> {
    await this.calculateEachMemberActualWorkingDay();
    this.calculateSummary();
  }

  calculateEachMemberActualWorkingDay(): Promise<void> {
    const daysInMonth = new Date(this.selectedMonthYear.getFullYear(), this.selectedMonthYear.getMonth() + 1, 0).getDate();
    const weekends = ['Saturday', 'Sunday'];
    let totalWeekendDays = 0;

    for (let i = 1; i <= daysInMonth; i++) {
      const currentDate = new Date(this.selectedMonthYear.getFullYear(), this.selectedMonthYear.getMonth(), i);
      if (weekends.includes(currentDate.toLocaleString('en-us', { weekday: 'long' }))) {
        totalWeekendDays++;
      }
    }
    const TotalWorkingDay = daysInMonth - totalWeekendDays;

    this.AQLunchPaymentReport.forEach((member) => {
      member.actual_workingDay = TotalWorkingDay - member.total_IndividualDayOff - member.total_WorkingOnline - member.total_CommissionDay - member.total_AQDayOff;
    });
    return Promise.resolve();
  }

  calculateSummary(): void {
    const summary_data = {
      total_IndividualDayOff: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.total_IndividualDayOff, 0),
      total_WorkingOnline: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.total_WorkingOnline, 0),
      total_CommissionDay: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.total_CommissionDay, 0),
      total_AQDayOff: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.total_AQDayOff, 0),
      total_AQActual_workingDay: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.actual_workingDay, 0)
    };
    this.summary_lunch_report = summary_data;
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
          'Member ID': report.id,
          'Full Name': report.fullName,
          'Nick Name': report.nickName,
          'Total Individual Day Off': report.total_IndividualDayOff,
          'Total Working Online': report.total_WorkingOnline,
          'Total Commission Day': report.total_CommissionDay,
          'Total AQ Day Off': report.total_AQDayOff,
          'Actual Working Day': report.actual_workingDay
        };
      });

      // Calculate the sum of each column
      const summary = {
        'Member ID': '',
        'Full Name': '',
        'Nick Name': 'Tổng cộng',
        'Total Individual Day Off': data.reduce((sum, report) => sum + report['Total Individual Day Off'], 0),
        'Total Working Online': data.reduce((sum, report) => sum + report['Total Working Online'], 0),
        'Total Commission Day': data.reduce((sum, report) => sum + report['Total Commission Day'], 0),
        'Total AQ Day Off': data.reduce((sum, report) => sum + report['Total AQ Day Off'], 0),
        'Actual Working Day': data.reduce((sum, report) => sum + report['Actual Working Day'], 0)
      };

      data.push(summary);

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

