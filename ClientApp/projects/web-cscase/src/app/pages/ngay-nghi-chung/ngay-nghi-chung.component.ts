import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DayOff } from './NgayNghiChung';
import {
  ConfirmationService,
  MessageService,
  PrimeNGConfig
} from "primeng/api";
import { Table } from 'primeng/table';

@Component({
  selector: 'app-ngay-nghi-chung',
  templateUrl: './ngay-nghi-chung.component.html',
  styleUrls: ['./ngay-nghi-chung.component.scss']
})

export class NgayNghiChungComponent implements OnInit {

  DayOffs: DayOff[];
  DayOffInitState: DayOff = {
    dateFrom: '',
    dateTo: '',
    sumDay: 0,
    reason: '',
    note: '',
  };
  DayOff: DayOff = {
    ...this.DayOffInitState
  };

  isValidDateRange: boolean = true;
  isValidDateRangeFilter: boolean = true;

  filter_datefrom: string = '';
  filter_dateto: string = '';

  openDialog: boolean;
  editDayOffDialog: boolean;
  addNewDayOffDialog: boolean;

  sumDay() {
    const date1 = new Date(this.DayOff.dateFrom);
    const date2 = new Date(this.DayOff.dateTo);
    let diffDays = 0;

    if (date1 > date2) {
      this.DayOff.sumDay = 0;
      this.isValidDateRange = false;
      return;
    }
    else {
      while (date1 <= date2) {
        this.isValidDateRange = true;
        if (date1.getDay() !== 0 && date1.getDay() !== 6) {
          diffDays++;
        }
        date1.setDate(date1.getDate() + 1);
      }
    }
    this.DayOff.sumDay = diffDays;
  }

  validateInputDates() {
    if (this.filter_datefrom && this.filter_dateto) {
      let dateFrom = new Date(this.convertDateFormat(this.filter_datefrom));
      let dateTo = new Date(this.convertDateFormat(this.filter_dateto));

      if (dateFrom > dateTo) {
        this.isValidDateRangeFilter = false;
      } else {
        this.isValidDateRangeFilter = true;
      }
    }
  }

  fetchDataFiltered() {
    let input_filter_datefrom = null;
    let input_filter_dateto = null;
    if (this.filter_datefrom) {
      input_filter_datefrom = this.convertDateFormat(this.filter_datefrom);
    }
    if (this.filter_dateto) {
      input_filter_dateto = this.convertDateFormat(this.filter_dateto);
    }
    this.fetchDayOffsData(input_filter_datefrom, input_filter_dateto);
  }

  convertDateFormat(date: string): string {
    const [day, month, year] = date.split('/');
    return `${month}/${day}/${year}`;
  }

  constructor(
    private https: HttpClient,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private primengConfig: PrimeNGConfig,
  ) {

  }

  ngOnInit() {
    this.filter_datefrom = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
    this.filter_dateto = new Date().toLocaleDateString('en-GB');
    this.fetchDayOffsData(this.convertDateFormat(this.filter_datefrom), this.convertDateFormat(this.filter_dateto));
    this.resetCalendarSelection();
    this.sumDay();

    this.primengConfig.ripple = true;
  }

  checkIsLeader() {
    let user = sessionStorage.getItem('current-user');
    return JSON.parse(user).isLeader
  }

  openAddDialog() {
    this.isValidDateRange = true;
    this.DayOff = {
      ...this.DayOffInitState
    };
    this.resetCalendarSelection();
    this.sumDay();
    this.addNewDayOffDialog = true;
    this.editDayOffDialog = false;
    this.openDialog = true;
  }

  openEditDialog(data: any) {
    this.isValidDateRange = true;
    this.DayOff = {};
    this.DayOff = { ...data };
    this.DayOff.dateFrom = new Date(data.dateFrom);
    this.DayOff.dateTo = new Date(data.dateTo);
    this.addNewDayOffDialog = false;
    this.editDayOffDialog = true;
    this.openDialog = true;
  }

  fetchDayOffsData(input_filter_datefrom?: string, input_filter_dateto?: string) {

    let params: any = {};
    if (input_filter_datefrom) {
      params.query_dateFrom = input_filter_datefrom + ' 12:00:00 AM';
    }
    if (input_filter_dateto) {
      params.query_dateTo = input_filter_dateto + ' 12:00:00 AM';
    }

    this.https.get<any>("/api/NgayPhepChung", { params: params }).subscribe({
      next: (res: any) => {
        this.DayOffs = res.data;

      },
      error: (error) => {
        console.log(error);

        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.sortInitData();
      }
    });
  }

  sortInitData() {
    this.DayOffs.sort((a, b) => {
      const dateA = new Date(a.dateFrom);
      const dateB = new Date(b.dateFrom);
      dateA.setHours(0, 0, 0, 0);
      dateB.setHours(0, 0, 0, 0);
      return dateB.getTime() - dateA.getTime();
    });
  }

  hideDialog() {
    this.DayOff = {};
    this.resetCalendarSelection();
    this.openDialog = false;
    this.editDayOffDialog = false;
    this.addNewDayOffDialog = false;
  }

  resetCalendarSelection() {
    this.DayOff.dateFrom = new Date();
    this.DayOff.dateFrom.setHours(0, 0, 0, 0);
    this.DayOff.dateTo = new Date();
    this.DayOff.dateTo.setHours(0, 0, 0, 0);
  }

  addNewDayOff() {
    let dayOffArray: DayOff[] = [this.DayOff];
    this.https.post<any>("/api/NgayPhepChung", dayOffArray).subscribe({
      next: (res: any) => {
        //todo
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.resetCalendarSelection();
        this.fetchDayOffsData();
      }
    });
    // this.AQmembers = [...this.AQmembers];
    this.addNewDayOffDialog = false;
    this.DayOff = {};
    this.hideDialog();
  }

  updateDayOff() {
    this.https.put<any>("/api/NgayPhepChung/" + this.DayOff.id, this.DayOff).subscribe({
      next: (res: any) => {
        //todo
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.fetchDayOffsData();
      }
    });
    this.hideDialog();
  }

  deleteDayOff(event: Event, data: any) {

    this.confirmationService.confirm({
      target: event.target,
      message: "Xóa đợt nghỉ phép chung này?",
      acceptLabel: 'Xóa',
      rejectLabel: 'Quay lại',
      icon: "pi pi-exclamation-triangle",
      accept: () => {
        this.messageService.add({
          severity: "info",
          summary: "Hủy thành công",
          detail: "Đã hủy dữ liệu khỏi hệ thống"
        });

        this.https.delete<any>("/api/NgayPhepChung/" + data.id, data).subscribe({
          next: (res: any) => {
            //todo
          },
          error: (error) => {
            console.log(error);
            // Your logic for handling errors
          },
          complete: () => {
            // Your logic for handling the completion event (optional)
            this.fetchDayOffsData();
          }
        });
      },
      reject: () => {
        this.messageService.add({
          severity: "error",
          summary: "Hủy xóa dữ liệu",
          detail: "Hủy xóa dữ liệu khỏi hệ thống"
        });
      }
    });
  }

  clear(table: Table) {
    table.clear();
    this.filter_datefrom = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
    this.filter_dateto = new Date().toLocaleDateString('en-GB');
    this.fetchDayOffsData(this.convertDateFormat(this.filter_datefrom), this.convertDateFormat(this.filter_dateto));
  }
}
