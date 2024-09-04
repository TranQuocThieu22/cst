import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {
  ConfirmationService,
  MessageService,
  PrimeNGConfig
} from "primeng/api";
import { Table } from 'primeng/table';
import { CommissionDay, CommissionMember } from './NgayCongTac';
import { BarChart } from 'echarts/charts';
import { TitleComponent, TooltipComponent, LegendComponent, ToolboxComponent, GridComponent, VisualMapComponent } from 'echarts/components';
import { map } from 'rxjs';

@Component({
  selector: 'app-ngay-cong-tac',
  templateUrl: './ngay-cong-tac.component.html',
  styleUrls: ['./ngay-cong-tac.component.scss']
})
export class NgayCongTacComponent implements OnInit {
  chartOptions
  CommissionDays: CommissionDay[] = [];
  memberDataMap: Map<string, number> = new Map<string, number>();
  CommissionDayInitState = {
    id: 0,
    dateFrom: '',
    dateTo: '',
    sumDay: 0,
    comissionContent: '',
    transportation: '',
    memberList: [],
    commissionExpenses: 0,
    note: ''
  };
  chartExtension
  CommissionDay: CommissionDay = {
    ...this.CommissionDayInitState
  };

  CommissionMemberList: CommissionMember[] = [];

  isValidDateRange: boolean = true;
  filter_datefrom: string = '';
  filter_dateto: string = '';
  user: any
  openDialog: boolean;
  isOpenMemberDialog: boolean;
  editCommissionDayDialog: boolean;
  addNewCommissionDayDialog: boolean;

  constructor(
    private https: HttpClient,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private primengConfig: PrimeNGConfig,
  ) { }

  ngOnInit() {
    this.user = JSON.parse(sessionStorage.getItem('current-user')).userData;

    this.chartExtension = [BarChart, TitleComponent, TooltipComponent, LegendComponent, ToolboxComponent, GridComponent, VisualMapComponent];

    this.filter_datefrom = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
    this.filter_dateto = new Date().toLocaleDateString('en-GB');

    this.fetchCommissionDaysData(this.convertDateFormat(this.filter_datefrom), this.convertDateFormat(this.filter_dateto));
    this.resetCalendarSelection();
    this.sumDay();

    this.primengConfig.ripple = true;
  }


  sumDay() {
    const date1 = new Date(this.CommissionDay.dateFrom);
    const date2 = new Date(this.CommissionDay.dateTo);
    let diffDays = 0;

    if (date1 > date2) {
      this.isValidDateRange = false;
      this.CommissionDay.sumDay = 0;
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
      id: 0,
      fullName: '',
      nickName: '',
      memberExpenses: 0
    });
  }

  removeMember(data: any) {
    console.log(data);

    // let processEditData = structuredClone(data);
    const index = this.CommissionDay.memberList.indexOf(data);
    this.CommissionDay.memberList.splice(index, 1);
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

  checkIsLeader() {
    return this.user
  }

  convertDateFormat(date: string): string {
    const [day, month, year] = date.split('/');
    return `${month}/${day}/${year}`;
  }

  openAddDialog() {
    this.fetchMemberCommissionData();
    this.CommissionDay = {
      ...this.CommissionDayInitState,
      memberList: []
    };
    this.resetCalendarSelection()
    this.sumDay();
    this.editCommissionDayDialog = false;
    this.addNewCommissionDayDialog = true;
    this.openDialog = true;
  }

  openEditDialog(data: any) {
    this.fetchMemberCommissionData();
    this.CommissionDay = {};
    this.CommissionDay = structuredClone(data);
    this.CommissionDay = {
      ...this.CommissionDay,
      dateFrom: new Date(data.dateFrom),
      dateTo: new Date(data.dateTo)
    };
    this.addNewCommissionDayDialog = false;
    this.editCommissionDayDialog = true;
    this.openDialog = true;
    console.log(this.CommissionDay);

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

    this.https.get<any>("/api/NgayCongTac", { params: params }).subscribe({
      next: (res: any) => {
        this.CommissionDays = res.data;
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.fetchMemberCommissionData();
        this.sortInitData();
      }
    });
  }

  sortInitData() {
    this.CommissionDays.sort((a, b) => {
      const dateA = new Date(a.dateFrom);
      const dateB = new Date(b.dateFrom);
      dateA.setHours(0, 0, 0, 0);
      dateB.setHours(0, 0, 0, 0);
      return dateB.getTime() - dateA.getTime();
    });
  }


  fetchMemberCommissionData() {
    this.https.get<any>("/api/ThongTinCaNhan/NhanVienCongTac").subscribe({
      next: (res: any) => {
        this.CommissionMemberList = [];
        this.CommissionMemberList = [...res.data];
        console.log(this.CommissionMemberList);

      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.loadMember_From_CommissionMemberList();
      }
    });
  }

  loadMember_From_CommissionMemberList() {
    const testData = new Map<string, number>()
    this.CommissionDays.forEach((commission: CommissionDay) => {
      commission.memberList.forEach((member: CommissionMember) => {
        const foundMember = this.CommissionMemberList.find((m: CommissionMember) => m.id === member.id);
        if (foundMember) {
          member.fullName = foundMember.fullName;
          member.nickName = foundMember.nickName;

        }
        // Aggregate sumDay by fullName
        if (testData.has(member.fullName)) {
          testData.set(member.fullName, testData.get(member.fullName) + commission.sumDay);
        } else {
          testData.set(member.fullName, commission.sumDay);
        }
        this.chartData(testData)
      });
    });

  }
  chartData(data) {
    const xAxisData = Array.from(data.keys());
    const SeriesData = Array.from(data.values());
    this.chartOptions = {
      xAxis: {
        type: 'category',
        data: xAxisData
      },
      yAxis: {
        type: 'value'
      },
      series: [
        {
          data: SeriesData,
          type: 'bar',
          label: {
            show: true,            // Enable labels
            position: 'inside',    // 'inside' or 'top' for label position
            formatter: '{c}',      // Display the value
            color: '#fff',         // Text color, adjust if needed
            fontWeight: 'bold'     // Make the text bold for better readability
          }
        }
      ]
    };
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
    CommissionDayArray.forEach((commissionDay: CommissionDay) => {
      commissionDay.memberList.forEach((member: CommissionMember) => {
        delete member.nickName;
        delete member.fullName;
      });
    });
    this.https.post<any>("/api/NgayCongTac", CommissionDayArray).subscribe({
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
        this.fetchCommissionDaysData();
      }
    });

    this.addNewCommissionDayDialog = false;
    this.CommissionDay = {};
    this.hideDialog();
  }

  updateCommissionDay() {
    this.https.put<any>("/api/NgayCongTac/" + this.CommissionDay.id, this.CommissionDay).subscribe({
      next: (res: any) => {
        //todo
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.fetchCommissionDaysData();
      }
    });
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

        this.https.delete<any>("/api/NgayCongTac/" + data.id, data).subscribe({
          next: (res: any) => {
            //todo
          },
          error: (error) => {
            console.log(error);
            // Your logic for handling errors
          },
          complete: () => {
            // Your logic for handling the completion event (optional)
            this.fetchCommissionDaysData();
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
    this.fetchCommissionDaysData();
    this.filter_datefrom = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
    this.filter_dateto = new Date().toLocaleDateString('en-GB');
  }

}
