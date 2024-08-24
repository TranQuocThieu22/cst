import { Component, OnInit } from '@angular/core';
import * as FileSaver from 'file-saver';
import { HttpClient } from '@angular/common/http';
import { Table } from 'primeng/table';
import { IndividualDayOffReport } from './thong-ke-nghi-phep-nam-DT';
@Component({
  selector: 'app-thong-ke-nghi-phep-nam',
  templateUrl: './thong-ke-nghi-phep-nam.component.html',
  styleUrls: ['./thong-ke-nghi-phep-nam.component.scss']
})
export class ThongKeNghiPhepNamComponent implements OnInit {

  AQIndividualWFT_DayOffReport: IndividualDayOffReport[];
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

    this.https.get<any>("/api/NgayPhepCaNhan/Thongkenghiphepnam", { params: params }).subscribe({
      next: (res: any) => {
        this.AQIndividualWFT_DayOffReport = res.data;
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

  exportExcel(type: string) {
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
          'Full Name': report.fullName,
          'Nick Name': report.nickName,
          'absenceQuota': report.absenceQuota,
          'dayOffs': report.dayOffs,
          'absenceQuotaLeft': report.absenceQuotaLeft,
          'wfhQuota': report.wfhQuota,
          'wfhQuotaNumber': report.wfhQuotaNumber,
          'total_wfh': report.total_wfh,
          'wfhQuotaLeft': report.wfhQuotaLeft
        };
      });
      const worksheet = xlsx.utils.json_to_sheet(data);
      const workbook = { Sheets: { 'data': worksheet }, SheetNames: ['data'] };
      const excelBuffer: any = xlsx.write(workbook, { bookType: 'xlsx', type: 'array' });
      this.saveAsExcelFile(excelBuffer, "annual_wfh&leave_report");
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
    this.fetch_AnnualWFH_DayOff_Report(this.selectedYearInput.getFullYear());
  }
}
