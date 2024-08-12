import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DayOff } from './NgayNghiChung';
import {
  ConfirmationService,
  MessageService,
  PrimeNGConfig
} from "primeng/api";

@Component({
  selector: 'app-ngay-nghi-chung',
  templateUrl: './ngay-nghi-chung.component.html',
  styleUrls: ['./ngay-nghi-chung.component.scss']
})

export class NgayNghiChungComponent implements OnInit {

  DayOffs: DayOff[];
  DayOff: DayOff = {
    datefrom: '',
    dateto: '',
    sumday: 0,
    reason: '',
    note: ''
  };

  // filter_datefrom: string = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
  // filter_dateto: string = new Date().toLocaleDateString('en-GB');

  filter_datefrom: string = '';
  filter_dateto: string = '';

  openDialog: boolean;
  editDayOffDialog: boolean;
  addNewDayOffDialog: boolean;

  sumDay() {
    const date1 = new Date(this.DayOff.datefrom);
    const date2 = new Date(this.DayOff.dateto);
    let diffDays = 0;

    if (date1 > date2) {
      this.DayOff.sumday = 0;
      return;
    }
    else {
      while (date1 <= date2) {
        if (date1.getDay() !== 0 && date1.getDay() !== 6) {
          diffDays++;
        }
        date1.setDate(date1.getDate() + 1);
      }
    }
    this.DayOff.sumday = diffDays;
  }

  fetchDataFiltered() {
    console.log(this.filter_datefrom, this.filter_dateto);
  }

  constructor(
    private https: HttpClient,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private primengConfig: PrimeNGConfig,
  ) {

  }

  ngOnInit() {
    this.fetchDayOffsData();
    this.resetCalendarSelection();
    this.sumDay();
    this.filter_datefrom = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
    this.filter_dateto = new Date().toLocaleDateString('en-GB');

    this.primengConfig.ripple = true;
  }

  openAddDialog() {
    // this.aqmember = {};
    // this.editMemberDialog = false;
    // this.addNewMemberDialog = true;
    this.openDialog = true;
    // console.log(this.aqmember);
  }

  openEditDialog(data: any) {
    this.DayOff = {};
    this.DayOff = { ...data };
    this.addNewDayOffDialog = false;
    this.editDayOffDialog = true;
    this.openDialog = true;
    console.log(this.DayOff);
  }

  fetchDayOffsData() {
    // this.https.get<any>("/api/ThongTinCaNhan").subscribe({
    //   next: (res: any) => {
    //     this.dayoffs = res.data
    //   },
    //   error: (error) => {
    //     console.log(error);
    //     // Your logic for handling errors
    //   },
    //   complete: () => {
    //     // Your logic for handling the completion event (optional)
    //   }
    // });
    this.DayOffs = [
      {
        id: 1,
        datefrom: new Date('2022-01-01 00:00:00'),
        dateto: new Date('2022-01-03 00:00:00'),
        sumday: 3,
        reason: 'Vacation',
        note: 'Enjoying some time off'
      },
      {
        id: 2,

        datefrom: new Date('2022-02-10 00:00:00'),
        dateto: new Date('2022-02-12 00:00:00'),
        sumday: 3,
        reason: 'Sick leave',
        note: 'Recovering from a flu'
      },
      {
        id: 3,
        datefrom: new Date('2022-03-20 00:00:00'),
        dateto: new Date('2022-03-21 00:00:00'),
        sumday: 2,
        reason: 'Personal day',
        note: 'Attending a family event'
      }
    ];

  }

  hideDialog() {
    this.DayOff = {};
    this.resetCalendarSelection();
    this.openDialog = false;
    this.editDayOffDialog = false;
    this.addNewDayOffDialog = false;
  }

  resetCalendarSelection() {
    this.DayOff.datefrom = new Date();
    this.DayOff.datefrom.setHours(0, 0, 0, 0);
    this.DayOff.dateto = new Date();
    this.DayOff.dateto.setHours(0, 0, 0, 0);
  }

  addNewDayOff() {
    console.log(this.DayOff);

    // let aqmemberArray: AQMember[] = [this.aqmember];

    // this.https.post<any>("/api/ThongTinCaNhan/Insert", aqmemberArray).subscribe({
    //   next: (res: any) => {
    //     // console.log(res);
    //   },
    //   error: (error) => {
    //     console.log(error);
    //     // Your logic for handling errors
    //   },
    //   complete: () => {
    //     // Your logic for handling the completion event (optional)
    //     this.fetchAQMemberData();
    //   }
    // });
    // this.AQmembers = [...this.AQmembers];
    this.addNewDayOffDialog = false;
    this.DayOff = {};
    this.resetCalendarSelection();
    this.hideDialog();
  }

  updateDayOff() {
    // this.https.put<any>("/api/ThongTinCaNhan/" + this.aqmember.id, this.aqmember).subscribe({
    //   next: (res: any) => {
    //     // console.log(res);
    //   },
    //   error: (error) => {
    //     console.log(error);
    //     // Your logic for handling errors
    //   },
    //   complete: () => {
    //     // Your logic for handling the completion event (optional)
    //     // this.fetchAQMemberData();
    //   }
    // });
    this.hideDialog();
  }

  deleteDayOff(event: Event, data: any) {
    console.log(data);

    this.confirmationService.confirm({
      target: event.target,
      message: "Hủy đợt nghỉ phép chung này?",
      acceptLabel: 'Hủy',
      rejectLabel: 'Quay lại',
      icon: "pi pi-exclamation-triangle",
      accept: () => {
        this.messageService.add({
          severity: "info",
          summary: "Hủy thành công",
          detail: "Đã hủy dữ liệu khỏi hệ thống"
        });

        // this.https.delete<any>("/api/ThongTinCaNhan/" + data.id, data).subscribe({
        //   next: (res: any) => {
        //     // console.log(res);
        //   },
        //   error: (error) => {
        //     console.log(error);
        //     // Your logic for handling errors
        //   },
        //   complete: () => {
        //     // Your logic for handling the completion event (optional)
        //     // this.fetchAQMemberData();
        //   }
        // });
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

}
