import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IndividualDayOff, Member, IndividualDayOff_API_DO } from './NgayPhepCaNhan';
import {
  ConfirmationService,
  MessageService,
  PrimeNGConfig
} from "primeng/api";
import { Table } from 'primeng/table';


@Component({
  selector: 'app-ngay-phep-ca-nhan',
  templateUrl: './ngay-phep-ca-nhan.component.html',
  styleUrls: ['./ngay-phep-ca-nhan.component.scss']
})
export class NgayPhepCaNhanComponent implements OnInit {

  approvalStatusOptions = [
    { label: 'Duyệt', value: 'Đã duyệt' },
    { label: 'Chưa duyệt', value: 'Chưa duyệt' },
    { label: 'Từ chối', value: 'Từ chối' }
  ]

  IndividualDayOffs: IndividualDayOff[];
  IndividualDayOffInitState = {
    dateFrom: '',
    dateTo: '',
    sumDay: 0,
    member: {
      id: 0,
      fullName: '',
      nickName: ''
    },
    isAnnual: false,
    isWithoutPay: false,
    approvalStatus: 'Chưa duyệt',
    reason: '',
    note: ''
  };
  IndividualDayOff: IndividualDayOff = {
    ...this.IndividualDayOffInitState
  };

  MemberList: Member[] = [];

  filter_datefrom: string = '';
  filter_dateto: string = '';

  openDialog: boolean;
  viewIndividualDayOffDialog: boolean;
  editIndividualDayOffDialog: boolean;
  addNewIndividualDayOffDialog: boolean;

  userInfo: any = {};

  constructor(
    private https: HttpClient,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private primengConfig: PrimeNGConfig,
  ) {

  }

  ngOnInit() {
    this.fetchIndividualDayOffsData();
    this.resetCalendarSelection();
    this.sumDay();
    this.filter_datefrom = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
    this.filter_dateto = new Date().toLocaleDateString('en-GB');
    this.primengConfig.ripple = true;
    this.fetchUserInfo();
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

    this.https.get<any>("/api/ThongTinCaNhan/" + userId).subscribe({
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
    const date1 = new Date(this.IndividualDayOff.dateFrom);
    const date2 = new Date(this.IndividualDayOff.dateTo);
    let diffDays = 0;

    if (date1 > date2) {
      this.IndividualDayOff.sumDay = 0;
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
    this.IndividualDayOff.sumDay = diffDays;
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
    this.fetchIndividualDayOffsData(input_filter_datefrom, input_filter_dateto);
  }

  convertDateFormat(date: string): string {
    const [day, month, year] = date.split('/');
    return `${month}/${day}/${year}`;
  }

  openViewDialog(data: any) {
    this.viewIndividualDayOffDialog = true;
    this.IndividualDayOff = { ...data };
    console.log(this.IndividualDayOff);

  }

  openAddDialog() {
    this.fetchMemberListData();
    this.IndividualDayOff = {
      ...this.IndividualDayOffInitState
    };
    this.resetCalendarSelection();
    this.sumDay();
    this.editIndividualDayOffDialog = false;
    this.addNewIndividualDayOffDialog = true;
    this.openDialog = true;
  }

  openEditDialog(data: any) {
    this.IndividualDayOff = {};
    this.IndividualDayOff = { ...data };
    this.IndividualDayOff.dateFrom = new Date(data.dateFrom);
    this.IndividualDayOff.dateTo = new Date(data.dateTo);
    this.addNewIndividualDayOffDialog = false;
    this.editIndividualDayOffDialog = true;
    this.openDialog = true;
  }

  fetchIndividualDayOffsData(input_filter_datefrom?: string, input_filter_dateto?: string) {
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

    this.https.get<any>("/api/NgayPhepCaNhan", { params: params }).subscribe({
      next: (res: any) => {
        this.convertType(res);
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

  convertType(res: any): void {
    this.IndividualDayOffs = res.data.map((item: IndividualDayOff_API_DO) => {
      const { memberId, ...rest } = item;
      const individualDayOff: IndividualDayOff = {
        ...rest,
        member: {
          id: memberId,
          fullName: '',
          nickName: ''
        },
      };
      return individualDayOff;
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
    this.IndividualDayOffs.forEach((individualdayoff: IndividualDayOff) => {
      const foundMember = this.MemberList.find((m: Member) => m.id === individualdayoff.member.id);
      if (foundMember) {
        individualdayoff.member.fullName = foundMember.fullName;
        individualdayoff.member.nickName = foundMember.nickName;
      }
    });
  }

  hideDialog() {
    this.IndividualDayOff = {};
    this.resetCalendarSelection();
    this.openDialog = false;
    this.editIndividualDayOffDialog = false;
    this.addNewIndividualDayOffDialog = false;
  }

  resetCalendarSelection() {
    this.IndividualDayOff.dateFrom = new Date();
    this.IndividualDayOff.dateFrom.setHours(0, 0, 0, 0);
    this.IndividualDayOff.dateTo = new Date();
    this.IndividualDayOff.dateTo.setHours(0, 0, 0, 0);
  }

  addNewIndividualDayOff() {
    let IndividualDayOffArray: IndividualDayOff[] = [this.IndividualDayOff];
    IndividualDayOffArray.forEach((item: any) => {
      const user = sessionStorage.getItem('current-user');
      if (!JSON.parse(user).isLeader) {
        item.memberId = JSON.parse(user).id
      } else {
        item.memberId = item.member.id;
      }
      delete item.member;
    });

    this.https.post<any>("/api/NgayPhepCaNhan", IndividualDayOffArray).subscribe({
      next: (res: any) => {
        this.convertType(res);
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.resetCalendarSelection();
        this.loadMember_From_MemberList();
      }
    });
    this.addNewIndividualDayOffDialog = false;
    this.IndividualDayOff = {};
    this.hideDialog();
  }

  updateIndividualDayOff() {
    //todo update user

    let IndividualDayOffData: any = structuredClone(this.IndividualDayOff);

    let user = JSON.parse(sessionStorage.getItem('current-user'));

    if (!user.isLeader) {
      IndividualDayOffData = {
        ...IndividualDayOffData,
        memberId: user.id
      }
    } else {
      IndividualDayOffData = {
        ...this.IndividualDayOff,
        memberId: this.IndividualDayOff.member.id
      }
    }

    this.https.put<any>("/api/NgayPhepCaNhan/" + this.IndividualDayOff.id, IndividualDayOffData).subscribe({
      next: (res: any) => {
        this.convertType(res);
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
    this.hideDialog();
  }

  deleteIndividualDayOff(event: Event, data: any) {

    this.confirmationService.confirm({
      target: event.target,
      message: "Xóa dữ liệu?",
      acceptLabel: 'Xóa',
      rejectLabel: 'Quay lại',
      icon: "pi pi-exclamation-triangle",
      accept: () => {
        this.messageService.add({
          severity: "info",
          summary: "Hủy thành công",
          detail: "Đã hủy dữ liệu khỏi hệ thống"
        });

        this.https.delete<any>("/api/NgayPhepCaNhan/" + data.id, data).subscribe({
          next: (res: any) => {
            this.convertType(res);
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
    this.https.put<any>("/api/NgayPhepCaNhan/DuyetNgayPhep", { id, approvalStatus }).subscribe({
      next: (res: any) => {
        this.convertType(res);
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

  clear(table: Table) {
    table.clear();
    this.fetchIndividualDayOffsData();
    this.filter_datefrom = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
    this.filter_dateto = new Date().toLocaleDateString('en-GB');
  }

}
