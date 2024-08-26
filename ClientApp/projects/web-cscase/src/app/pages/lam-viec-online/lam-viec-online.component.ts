import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WorkingOnline, Member, WorkingOnline_API_DO } from './LamViecOnline';
import {
  ConfirmationService,
  MessageService,
  PrimeNGConfig
} from "primeng/api";
import { Table } from 'primeng/table';
import { TitleComponent, TooltipComponent, LegendComponent, ToolboxComponent, GridComponent, VisualMapComponent } from 'echarts/components';
import { BarChart } from 'echarts/charts';

@Component({
  selector: 'app-lam-viec-online',
  templateUrl: './lam-viec-online.component.html',
  styleUrls: ['./lam-viec-online.component.scss']
})
export class LamViecOnlineComponent implements OnInit {
  chartOptions: any;
  chartExtension: any;

  approvalStatusOptions = [
    { label: 'Duyệt', value: 'Đã duyệt' },
    { label: 'Chưa duyệt', value: 'Chưa duyệt' },
    { label: 'Từ chối', value: 'Từ chối' }
  ]

  WorkingOnlines: WorkingOnline[];
  WorkingOnlineInitState = {
    dateFrom: '',
    dateTo: '',
    sumDay: 0,
    member: {
      id: 0,
      fullName: '',
      nickName: ''
    },
    approvalStatus: 'Chưa duyệt',
    reason: '',
    note: ''
  };
  WorkingOnline: WorkingOnline = {
    ...this.WorkingOnlineInitState
  };

  MemberList: Member[] = [];

  filter_datefrom: string = '';
  filter_dateto: string = '';

  openDialog: boolean;
  viewWorkingOnlineDialog: boolean;
  editWorkingOnlineDialog: boolean;
  addNewWorkingOnlineDialog: boolean;

  userInfo: any = {};

  constructor(
    private https: HttpClient,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private primengConfig: PrimeNGConfig,
  ) {

  }

  ngOnInit() {
    this.fetchWorkingOnlinesData();
    this.resetCalendarSelection();
    this.sumDay();
    this.filter_datefrom = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
    this.filter_dateto = new Date().toLocaleDateString('en-GB');
    this.primengConfig.ripple = true;
    this.fetchUserInfo();
    this.chartExtension = [BarChart, TitleComponent, TooltipComponent, LegendComponent, ToolboxComponent, GridComponent, VisualMapComponent];
  }

  checkIsLeader() {
    const user = sessionStorage.getItem('current-user');
    return JSON.parse(user).isLeader;
  }

  fetchUserInfo() {
    const user = sessionStorage.getItem('current-user');
    const userInfo = JSON.parse(user);
    const userId = +userInfo["id"];

    if (userInfo.role === 'admin') {
      return;
    }

    this.https.get<any>("/api/LamViecOnline/" + userId).subscribe({
      next: (res: any) => {
        this.userInfo = res.data[0];
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

  sumDay() {
    const date1 = new Date(this.WorkingOnline.dateFrom);
    const date2 = new Date(this.WorkingOnline.dateTo);
    let diffDays = 0;

    if (date1 > date2) {
      this.WorkingOnline.sumDay = 0;
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
    this.WorkingOnline.sumDay = diffDays;
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
    this.fetchWorkingOnlinesData(input_filter_datefrom, input_filter_dateto);
  }

  convertDateFormat(date: string): string {
    const [day, month, year] = date.split('/');
    return `${month}/${day}/${year}`;
  }

  openViewDialog(data: any) {
    this.viewWorkingOnlineDialog = true;
    this.WorkingOnline = { ...data };
    console.log(this.WorkingOnline);

  }

  openAddDialog() {
    this.fetchMemberListData();
    this.WorkingOnline = {
      ...this.WorkingOnlineInitState
    };
    this.resetCalendarSelection();
    this.sumDay();
    this.editWorkingOnlineDialog = false;
    this.addNewWorkingOnlineDialog = true;
    this.openDialog = true;
  }

  openEditDialog(data: any) {
    this.WorkingOnline = {};
    this.WorkingOnline = { ...data };
    this.WorkingOnline.dateFrom = new Date(data.dateFrom);
    this.WorkingOnline.dateTo = new Date(data.dateTo);
    this.addNewWorkingOnlineDialog = false;
    this.editWorkingOnlineDialog = true;
    this.openDialog = true;
  }

  fetchWorkingOnlinesData(input_filter_datefrom?: string, input_filter_dateto?: string) {
    let user = JSON.parse(sessionStorage.getItem('current-user'));
    let params: any = {};
    if (input_filter_datefrom) {
      params.query_dateFrom = input_filter_datefrom + ' 00:00:00';
    }
    if (input_filter_dateto) {
      params.query_dateTo = input_filter_dateto + ' 00:00:00';
    }

    if (!user.isLeader) {
      params.query_memberId = user.id;
    }

    this.https.get<any>("/api/LamViecOnline", { params: params }).subscribe({
      next: (res: any) => {
        this.convertType(res);
        this.sortInitData();
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.fetchMemberListData();
      }
    });
  }

  calculateData(): any {
    const memberTotalWorkingOnlines = {};
    this.WorkingOnlines.forEach((WorkingOnline: WorkingOnline) => {
      const memberName = WorkingOnline.member.fullName;
      if (memberTotalWorkingOnlines.hasOwnProperty(memberName)) {
        memberTotalWorkingOnlines[memberName] += WorkingOnline.sumDay;
      } else {
        memberTotalWorkingOnlines[memberName] = WorkingOnline.sumDay;
      }
    });

    return memberTotalWorkingOnlines;
  }

  chartData(data) {
    const xAxisData = Object.keys(data);
    const SeriesData = Object.values(data);
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

  sortInitData() {
    this.WorkingOnlines.sort((a, b) => {
      const dateA = new Date(a.dateFrom);
      const dateB = new Date(b.dateFrom);
      dateA.setHours(0, 0, 0, 0);
      dateB.setHours(0, 0, 0, 0);
      return dateB.getTime() - dateA.getTime();
    });
  }

  convertType(res: any): void {
    this.WorkingOnlines = res.data.map((item: WorkingOnline_API_DO) => {
      const { memberId, ...rest } = item;
      const WorkingOnline: WorkingOnline = {
        ...rest,
        member: {
          id: memberId,
          fullName: '',
          nickName: ''
        },
      };
      return WorkingOnline;
    });
  }

  fetchMemberListData() {
    this.https.get<any>("/api/ThongTinCaNhan/NhanVienCongTac").subscribe({
      next: (res: any) => {
        this.MemberList = [];
        this.MemberList = [...res.data];
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.loadMember_From_MemberList();
      }
    });
  }

  loadMember_From_MemberList() {
    this.WorkingOnlines.forEach((WorkingOnline: WorkingOnline) => {
      const foundMember = this.MemberList.find((m: Member) => m.id === WorkingOnline.member.id);
      if (foundMember) {
        WorkingOnline.member.fullName = foundMember.fullName;
        WorkingOnline.member.nickName = foundMember.nickName;
      }
    });
    this.chartData(this.calculateData());
  }


  hideDialog() {
    this.WorkingOnline = {};
    this.resetCalendarSelection();
    this.openDialog = false;
    this.editWorkingOnlineDialog = false;
    this.addNewWorkingOnlineDialog = false;
  }

  resetCalendarSelection() {
    this.WorkingOnline.dateFrom = new Date();
    this.WorkingOnline.dateFrom.setHours(0, 0, 0, 0);
    this.WorkingOnline.dateTo = new Date();
    this.WorkingOnline.dateTo.setHours(0, 0, 0, 0);
  }

  addNewWorkingOnline() {
    let WorkingOnlineArray: WorkingOnline[] = [this.WorkingOnline];
    WorkingOnlineArray.forEach((item: any) => {
      const user = sessionStorage.getItem('current-user');
      if (!JSON.parse(user).isLeader) {
        item.memberId = JSON.parse(user).id
      } else {
        item.memberId = item.member.id;
      }
      delete item.member;
    });

    this.https.post<any>("/api/LamViecOnline", WorkingOnlineArray).subscribe({
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
        this.fetchWorkingOnlinesData();
      }
    });
    this.addNewWorkingOnlineDialog = false;
    this.WorkingOnline = {};
    this.hideDialog();
  }

  updateWorkingOnline() {
    let WorkingOnlineData: any = structuredClone(this.WorkingOnline);

    let user = JSON.parse(sessionStorage.getItem('current-user'));

    if (!user.isLeader) {
      WorkingOnlineData = {
        ...WorkingOnlineData,
        memberId: user.id
      }
    } else {
      WorkingOnlineData = {
        ...this.WorkingOnline,
        memberId: this.WorkingOnline.member.id
      }
    }

    this.https.put<any>("/api/LamViecOnline/" + this.WorkingOnline.id, WorkingOnlineData).subscribe({
      next: (res: any) => {
        // todo
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.fetchWorkingOnlinesData();
      }
    });
    this.hideDialog();
  }

  deleteWorkingOnline(event: Event, data: any) {

    this.confirmationService.confirm({
      target: event.target,
      message: "Xóa dữ liệu?",
      acceptLabel: 'Xóa',
      rejectLabel: 'Quay lại',
      icon: "pi pi-exclamation-triangle",
      accept: () => {
        this.messageService.add({
          severity: "info",
          summary: "Xóa thành công",
          detail: "Đã xóa dữ liệu khỏi hệ thống"
        });

        this.https.delete<any>("/api/LamViecOnline/" + data.id, data).subscribe({
          next: (res: any) => {
            //todo
          },
          error: (error) => {
            console.log(error);
            // Your logic for handling errors
          },
          complete: () => {
            // Your logic for handling the completion event (optional)
            this.fetchWorkingOnlinesData();
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

  saveApprovalStatus(data: any) {
    let id = data.id;
    let approvalStatus = data.approvalStatus;
    this.https.put<any>("/api/LamViecOnline/DuyetLamViecOnline", { id, approvalStatus }).subscribe({
      next: (res: any) => {
        //todo
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.fetchWorkingOnlinesData();
      }
    });

  }

  clear(table: Table) {
    table.clear();
    this.fetchWorkingOnlinesData();
    this.filter_datefrom = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
    this.filter_dateto = new Date().toLocaleDateString('en-GB');
  }

}
