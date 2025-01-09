import { Component, OnInit } from '@angular/core';
import * as FileSaver from 'file-saver';
import { HttpClient } from '@angular/common/http';
import { Table } from 'primeng/table';
import { IndividualQuotaReport } from './thong-ke-nghi-phep-nam-DT';
@Component({
  selector: 'app-thong-ke-nghi-phep-nam',
  templateUrl: './thong-ke-nghi-phep-nam.component.html',
  styleUrls: ['./thong-ke-nghi-phep-nam.component.scss']
})
export class ThongKeNghiPhepNamComponent implements OnInit {

  AQIndividualWFT_DayOffReport: IndividualQuotaReport[];
  selectedYearInput: any;

  selectedRecords: any[];
  exportColumns: any[];

  constructor(
    private https: HttpClient,
  ) { }

  ngOnInit(): void {
    this.selectedYearInput = new Date();
    this.selectedYearInput.setHours(0, 0, 0, 0);
    this.fetch_AnnualWFH_DayOff_Report(this.selectedYearInput.getFullYear());
  }

  convertDateFormat(date: string): string {
    const [day, month, year] = date.split('/');
    return `${month}/${day}/${year}`;
  }

  fetchFilteredData() {
    const year = this.selectedYearInput.getFullYear();
    this.fetch_AnnualWFH_DayOff_Report(year);
  }

  fetch_AnnualWFH_DayOff_Report(year?: any) {
    let params: any = {};
    if (year) {
      params = { year: year };
    }

    this.https.get<any>("/api/BaoBieuThongKe/Thongkehanmuccanhan", { params: params }).subscribe({
      next: (res: any) => {
        this.AQIndividualWFT_DayOffReport = res.data;
        console.log(res.data);
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

  countWeekends(startDate: Date, endDate: Date): number {
    let count = 0;
    const current = new Date(startDate);
    while (current <= endDate) {
      if (current.getDay() === 0 || current.getDay() === 6) {
        count++;
      }
      current.setDate(current.getDate() + 1);
    }
    return count;
  }

  calculateSummary(type: string, aqmember: any) {
    if (!this.AQIndividualWFT_DayOffReport) return 0;
    let result = 0;
    switch (type) {
      case 'songayphepconlai':
        result = aqmember.absenceQuota - aqmember.dayOffs;
        break;
      case 'hanmucsongayonline':
        result = this.calculateWfhQuotaNumber(aqmember);
        break;
      case 'songayonlineconlai':
        const wfhQuotaNumber = this.calculateWfhQuotaNumber(aqmember);
        result = wfhQuotaNumber - aqmember.total_wfh;
        break;

      default:
        break;
    }
    return result;
  }

  calculateWfhQuotaNumber(aqmember: any) {
    const year = this.selectedYearInput.getFullYear();
    const startDate = new Date(year, 0, 1);
    const endDate = new Date(year, 11, 31);
    const totalDays = Math.floor((endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24));
    const weekends = this.countWeekends(startDate, endDate);
    const workingDays = totalDays - weekends;
    const wfhQuotaNumber = Math.floor((workingDays * aqmember.wfhQuota) / 100);
    return wfhQuotaNumber;
  }

  exportExcel_individualDayOff(type: string) {
    let data = [];
    if (type === 'full') {
      data = this.AQIndividualWFT_DayOffReport;
    }
    else {
      data = this.selectedRecords;
    }

    import("xlsx").then(xlsx => {
      data = data.map(report => {
        return {
          'Họ tên': report.fullName,
          'Hạn mức ngày phép cơ bản': report.minAbsenceQuota,
          'Hạn mức ngày phép tăng thêm': report.additionalAbsenceQuota,
          'Tổng ngày đã nghỉ': report.totalDayOff,
          'Tổng ngày đã nghỉ sử dụng hạn mức cơ bản (cả ngày)': report.totalDayOffFullType1,
          'Tổng ngày đã nghỉ sử dụng hạn mức cơ bản (nửa ngày)': report.totalDayOffFullType2,
          'Tổng ngày đã nghỉ sử dụng hạn mức tăng thêm (cả ngày)': report.totalDayOffHalfType1,
          'Tổng ngày đã nghỉ sử dụng hạn mức tăng thêm (nửa ngày)': report.totalDayOffHalfType2,
          'Tổng ngày đã nghỉ không sử dụng ngày phép': report.totalDayOffType3_4,
          'Hạn mức cơ bản đã sử dụng': report.usedMinAbsenceQuota,
          'Hạn mức tăng thêm đã sử dụng': report.usedAdditionalAbsenceQuota,
          'Hạn mức cở bản còn lại': report.remainMinAbsenceQuota,
          'Hạn mức tăng thêm còn lại': report.remainAdditionalAbsenceQuota
        };
      });
      const worksheet = xlsx.utils.json_to_sheet(data);
      const workbook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };
      const excelBuffer: any = xlsx.write(workbook, { bookType: 'xlsx', type: 'array' });
      this.saveAsExcelFile(excelBuffer, "hanmucngaynghiphep");
    });
  }

  exportExcel_workingOnline(type: string) {
    let data = [];
    if (type === 'full') {
      data = this.AQIndividualWFT_DayOffReport;
    }
    else {
      data = this.selectedRecords;
    }

    import("xlsx").then(xlsx => {
      data = data.map(report => {
        return {
          // minWfhQuota?: number;
          // additionalWfhQuota?: number;
          // totalWorkOnlineDay?: number;
          // totalWorkOnlineDayFullType1?: number;
          // totalWorkOnlineDayFullType2?: number;
          // totalWorkOnlineDayHalfType1?: number;
          // totalWorkOnlineDayHalfType2?: number;
          // totalWorkOnlineDayType3_4?: number;
          // usedMinWfhQuota?: number;
          // usedAdditionalWfhQuota?: number;
          // remainMinWfhQuota?: number;
          // remainAdditionalWfhQuota?: number;
          'Họ tên': report.fullName,
          'Hạn mức làm việc online (%)': this.displayWFHPercent(report.minWfhQuota),
          'Hạn mức làm việc online (sl)': report.minWfhQuota,
          'Tổng ngày làm việc online': report.totalWorkOnlineDay,
          'Tổng ngày làm việc online sử dụng hạn mức(cả ngày)': report.totalWorkOnlineDayFullType1,
          'Tổng ngày làm việc online sử dụng hạn mức(nửa ngày)': report.totalWorkOnlineDayFullType2,
          'Tổng ngày làm việc online không sử dụng hạn mức': report.totalWorkOnlineDayType3_4,
          'Hạn mức đã sử dụng (sl)': report.usedMinWfhQuota,
          'Hạn mức còn lại (sl)': report.remainMinWfhQuota,
        };
      });
      const worksheet = xlsx.utils.json_to_sheet(data);
      const workbook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };
      const excelBuffer: any = xlsx.write(workbook, { bookType: 'xlsx', type: 'array' });
      this.saveAsExcelFile(excelBuffer, "hanmucngaynghiphep");
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

  displayWFHPercent(wfhQuota: number) {
    const currentYear = new Date().getFullYear();
    const totalDaysOfYear = (new Date(currentYear, 11, 31).getDate() === 31) ? 366 : 365;
    if (wfhQuota !== undefined) {
      return Math.round((wfhQuota / totalDaysOfYear) * 100);
    }
  }

  clear(table: Table) {
    table.clear();
    this.selectedYearInput = new Date();
    this.fetch_AnnualWFH_DayOff_Report(this.selectedYearInput.getFullYear());
  }
}
