import { Component, OnInit } from '@angular/core';
import * as FileSaver from 'file-saver';
import { HttpClient } from '@angular/common/http';
import { LunchPaymentReport } from './tien-an-trua-DT'
import { Table } from 'primeng/table';
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

  lunchPaymentType1: number = 40000;
  lunchPaymentType2: number = 20000;
  lunchPaymentType3: number = 20000;


  isLoading: boolean = false;

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
    this.isLoading = true;
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
        this.isLoading = false;
      }
    });
  }

  async handleAfterFetchLunchReportData(): Promise<void> {
    await this.calculateEachMemberActualWorkingDayAndPayment();
    this.calculateSummary();
  }

  calculateEachMemberActualWorkingDayAndPayment(): Promise<void> {
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
      member.office_workingDay = TotalWorkingDay - member.total_IndividualDayOff - member.total_WorkingOnline - member.total_CommissionDay_full - member.total_CommissionDay_half - member.total_AQDayOff;
      switch (member.employeeType) {
        case 1:
          member.lunchPayment = member.office_workingDay * this.lunchPaymentType1;
          break;
        case 2:
          member.lunchPayment = member.office_workingDay * this.lunchPaymentType2;
          break;
        case 3:
          member.lunchPayment = member.office_workingDay * this.lunchPaymentType3;
          break;
        default:
          member.lunchPayment = 0;
          break;
      }
    });
    return Promise.resolve();
  }

  calculateSummary(): void {
    const summary_data = {
      sum_total_IndividualDayOff: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.total_IndividualDayOff, 0),
      sum_total_IndividualDayOff_full: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.total_IndividualDayOff_full, 0),
      sum_total_IndividualDayOff_half: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.total_IndividualDayOff_half, 0),
      sum_total_WorkingOnline: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.total_WorkingOnline, 0),
      sum_total_WorkingOnline_full: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.total_WorkingOnline_full, 0),
      sum_total_WorkingOnline_half: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.total_WorkingOnline_half, 0),
      sum_total_CommissionDay_full: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.total_CommissionDay_full, 0),
      sum_total_CommissionDay_half: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.total_CommissionDay_half, 0),
      sum_total_AQDayOff: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.total_AQDayOff, 0),
      sum_total_Office_workingDay: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.office_workingDay, 0),
      sum_lunchPayment: this.AQLunchPaymentReport.reduce((acc, report) => acc + report.lunchPayment, 0),
    };
    this.summary_lunch_report = summary_data;
    // this.summary_lunch_report = {
    //   total_IndividualDayOff: 0,
    //   total_WorkingOnline: 0,
    //   total_CommissionDay: 0,
    //   total_AQDayOff: 0,
    //   total_AQActual_workingDay: 0,
    // }
  }

  calculateSummaryLunchPayment() {
    this.calculateEachMemberActualWorkingDayAndPayment();
    this.calculateSummary();
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
          // id: number;
          // fullName?: string;
          // nickName?: string;
          // employeeType?: number;
          // total_IndividualDayOff?: number;
          // total_WorkingOnline?: number;
          // total_IndividualDayOff_full?: number;
          // total_IndividualDayOff_half?: number;
          // total_WorkingOnline_full?: number;
          // total_WorkingOnline_half?: number;
          // total_CommissionDay_full?: number;
          // total_CommissionDay_half?: number;
          // total_AQDayOff?: number;
          // office_workingDay?: number;
          // lunchPayment?: number;
          'Họ tên': report.fullName,
          'Tổng ngày nghỉ': report.total_IndividualDayOff,
          'Nghỉ (cả ngày)': report.total_IndividualDayOff_full,
          'Nghỉ (nửa ngày)': report.total_IndividualDayOff_half,
          'Tổng ngày làm việc online': report.total_WorkingOnline,
          'Làm online (cả ngày)': report.total_WorkingOnline_full,
          'làm online (nửa ngày)': report.total_WorkingOnline_half,
          'Công tác (cả ngày)': report.total_CommissionDay_full,
          'Công tác (nửa ngày)': report.total_CommissionDay_half,
          'Ngày nghỉ chung': report.total_AQDayOff,
          'Đi làm thực tế': report.office_workingDay,
          'Tiền ăn trưa': report.lunchPayment
        };
      });

      // Calculate the sum of each column
      const summary = {
        'Họ tên': '',
        'Tổng ngày nghỉ': '',
        'Nghỉ (cả ngày)': '',
        'Nghỉ (nửa ngày)': '',
        'Tổng ngày làm việc online': '',
        'Làm online (cả ngày)': '',
        'làm online (nửa ngày)': '',
        'Công tác (cả ngày)': '',
        'Công tác (nửa ngày)': '',
        'Ngày nghỉ chung': '',
        'Đi làm thực tế': 'Tổng cộng',
        'Tiền ăn trưa': data.reduce((sum, report) => sum + report['Tiền ăn trưa'], 0)
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

  clear(table: Table) {
    table.clear();
    this.selectedMonthYear = new Date();
    this.fetchLunchPaymentReport(this.selectedMonthYear);
    this.lunchPaymentType1 = 40000;
    this.lunchPaymentType2 = 20000;
    this.lunchPaymentType3 = 20000;
    this.calculateEachMemberActualWorkingDayAndPayment();
  }
}

