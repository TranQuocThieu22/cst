import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  ConfirmationService,
  MessageService,
  PrimeNGConfig
} from "primeng/api";
import { Table } from 'primeng/table';
import { CommissionDay } from './NgayCongTac';

@Component({
  selector: 'app-ngay-cong-tac',
  templateUrl: './ngay-cong-tac.component.html',
  styleUrls: ['./ngay-cong-tac.component.scss']
})
export class NgayCongTacComponent implements OnInit {

  CommissionDays: CommissionDay[] = [];

  CommissionDay: CommissionDay = {
    id: 0,
    dateFrom: new Date(),
    dateTo: new Date(),
    sumDay: 0,
    comissionContent: '',
    transportation: '',
    memberList: [],
    commissionExpenses: 0,
    note: ''
  };

  filter_datefrom: string = '';
  filter_dateto: string = '';

  openDialog: boolean;
  isOpenMemberDialog: boolean;
  editCommissionDayDialog: boolean;
  addNewCommissionDayDialog: boolean;

  sumDay() {
    const date1 = new Date(this.CommissionDay.dateFrom);
    const date2 = new Date(this.CommissionDay.dateTo);
    let diffDays = 0;

    if (date1 > date2) {
      this.CommissionDay.sumDay = 0;
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
    this.CommissionDay.sumDay = diffDays;
  }

  sumExpenses(data: any) {
    let sum = 0;
    data.forEach((member: any) => {
      sum += member.memberExpenses;
    });
    return sum;
  }

  getMemberList(data: any) {
    let memberString = '';
    data.forEach((member: any) => {
      memberString += member.fullName + ', ';
    });
    memberString = memberString.slice(0, -2); // Remove the last comma and space
    return memberString;
  }

  addMember() {
    this.CommissionDay.memberList.push({
      fullName: '',
      nickName: '',
      memberExpenses: 0
    });
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
    this.fetchCommissionDaysData(input_filter_datefrom, input_filter_dateto);
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
    // this.fetchCommissionDaysData();
    this.CommissionDays = [
      {
        id: 1,
        dateFrom: new Date(),
        dateTo: new Date(),
        sumDay: 5,
        comissionContent: 'Sample comissionContent 1',
        transportation: 'Car',
        memberList: [
          {
            fullName: 'John Doe',
            nickName: 'JD',
            memberExpenses: 100
          },
          {
            fullName: 'Jane Smith',
            nickName: 'JS',
            memberExpenses: 150
          }
        ],
        commissionExpenses: 500000,
        note: 'Sample note 1'
      },
      {
        id: 2,
        dateFrom: new Date(),
        dateTo: new Date(),
        sumDay: 3,
        comissionContent: 'Sample comissionContent 2',
        transportation: 'Walk',
        memberList: [
          {
            fullName: 'Alice Johnson',
            nickName: 'AJ',
            memberExpenses: 200
          }
        ],
        commissionExpenses: 300000,
        note: 'Sample note 2'
      },
      {
        id: 3,
        dateFrom: new Date(),
        dateTo: new Date(),
        sumDay: 2,
        comissionContent: 'Sample comissionContent 3',
        transportation: 'Plane',
        memberList: [
          {
            fullName: 'Bob Williams',
            nickName: 'BW',
            memberExpenses: 120
          },
          {
            fullName: 'Emily Davis',
            nickName: 'ED',
            memberExpenses: 80
          },
          {
            fullName: 'Michael Brown',
            nickName: 'MB',
            memberExpenses: 90
          }
        ],
        commissionExpenses: 290000,
        note: 'Sample note 3'
      }
    ];

    this.resetCalendarSelection();
    this.sumDay();
    this.filter_datefrom = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
    this.filter_dateto = new Date().toLocaleDateString('en-GB');

    this.primengConfig.ripple = true;
  }

  openAddDialog() {
    console.log("CommissionDay:", this.CommissionDay);
    // this.CommissionDay = {};
    // this.editCommissionDayDialog = false;
    // this.addNewCommissionDayDialog = true;
    this.openDialog = true;
  }

  openEditDialog(data: any) {
    this.CommissionDay = {};
    this.CommissionDay = { ...data };
    this.CommissionDay.dateFrom = new Date(data.dateFrom);
    this.CommissionDay.dateTo = new Date(data.dateTo);
    this.addNewCommissionDayDialog = false;
    this.editCommissionDayDialog = true;
    this.openDialog = true;
  }

  openMemberDialog(data: any) {
    this.isOpenMemberDialog = true;
    this.CommissionDay = { ...data };
  }

  fetchCommissionDaysData(input_filter_datefrom?: string, input_filter_dateto?: string) {
    let params: any = {};
    if (input_filter_datefrom) {
      params.query_dateFrom = input_filter_datefrom + ' 00:00:00';
    }
    if (input_filter_dateto) {
      params.query_dateTo = input_filter_dateto + ' 00:00:00';
    }

    // this.https.get<any>("/api/NgayPhepChung", { params: params }).subscribe({
    //   next: (res: any) => {
    //     this.CommissionDays = res.data;

    //   },
    //   error: (error) => {
    //     console.log(error);

    //     // Your logic for handling errors
    //   },
    //   complete: () => {
    //     // Your logic for handling the completion event (optional)
    //   }
    // });
  }

  hideDialog() {
    this.CommissionDay = {};
    this.resetCalendarSelection();
    this.openDialog = false;
    this.editCommissionDayDialog = false;
    this.addNewCommissionDayDialog = false;
  }

  resetCalendarSelection() {
    this.CommissionDay.dateFrom = new Date();
    this.CommissionDay.dateFrom.setHours(0, 0, 0, 0);
    this.CommissionDay.dateTo = new Date();
    this.CommissionDay.dateTo.setHours(0, 0, 0, 0);
  }

  addNewCommissionDay() {
    let CommissionDayArray: CommissionDay[] = [this.CommissionDay];
    // this.https.post<any>("/api/NgayPhepChung/Insert", CommissionDayArray).subscribe({
    //   next: (res: any) => {
    //     this.CommissionDays = res.data
    //   },
    //   error: (error) => {
    //     console.log(error);
    //     // Your logic for handling errors
    //   },
    //   complete: () => {
    //     // Your logic for handling the completion event (optional)
    //     this.resetCalendarSelection();
    //   }
    // });
    // this.AQmembers = [...this.AQmembers];
    this.addNewCommissionDayDialog = false;
    this.CommissionDay = {};
    this.hideDialog();
  }

  updateCommissionDay() {
    // this.https.put<any>("/api/NgayPhepChung/" + this.CommissionDay.id, this.CommissionDay).subscribe({
    //   next: (res: any) => {
    //     this.CommissionDays = res.data
    //   },
    //   error: (error) => {
    //     console.log(error);
    //     // Your logic for handling errors
    //   },
    //   complete: () => {
    //     // Your logic for handling the completion event (optional)
    //   }
    // });
    this.hideDialog();
  }

  deleteCommissionDay(event: Event, data: any) {

    this.confirmationService.confirm({
      target: event.target,
      message: "Xóa ngày công tác này?",
      acceptLabel: 'Xóa',
      rejectLabel: 'Quay lại',
      icon: "pi pi-exclamation-triangle",
      accept: () => {
        this.messageService.add({
          severity: "info",
          summary: "Xóathành công",
          detail: "Đã xóa dữ liệu khỏi hệ thống"
        });

        // this.https.delete<any>("/api/NgayPhepChung/" + data.id, data).subscribe({
        //   next: (res: any) => {
        //     this.CommissionDays = res.data
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

  clear(table: Table) {
    table.clear();
    // this.fetchCommissionDaysData();
    this.filter_datefrom = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
    this.filter_dateto = new Date().toLocaleDateString('en-GB');
  }

}
