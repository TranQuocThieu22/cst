import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WorkingOT, Member, WorkingOT_API_DO } from './LamViecNgoaiGio';
import {
  ConfirmationService,
  MessageService,
  PrimeNGConfig
} from "primeng/api";
import { Table } from 'primeng/table';

@Component({
  selector: 'app-lam-viec-ngoai-gio',
  templateUrl: './lam-viec-ngoai-gio.component.html',
  styleUrls: ['./lam-viec-ngoai-gio.component.scss']
})
export class LamViecNgoaiGioComponent implements OnInit {

  WorkingOTs: WorkingOT[];
  WorkingOTInitState = {
    date: '',
    time: 0,
    member: {
      id: 0,
      fullName: '',
      nickName: ''
    },
    note: ''
  };
  WorkingOT: WorkingOT = {
    ...this.WorkingOTInitState
  };

  MemberList: Member[] = [];

  filter_datefrom: string = '';
  filter_dateto: string = '';

  openDialog: boolean;
  editWorkingOTDialog: boolean;
  addNewWorkingOTDialog: boolean;

  fetchDataFiltered() {
    let input_filter_datefrom = null;
    let input_filter_dateto = null;
    if (this.filter_datefrom) {
      input_filter_datefrom = this.convertDateFormat(this.filter_datefrom);
    }
    if (this.filter_dateto) {
      input_filter_dateto = this.convertDateFormat(this.filter_dateto);
    }
    this.fetchWorkingOTsData(input_filter_datefrom, input_filter_dateto);
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
    this.fetchWorkingOTsData();
    this.resetCalendarSelection();
    this.filter_datefrom = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
    this.filter_dateto = new Date().toLocaleDateString('en-GB');

    this.primengConfig.ripple = true;
  }

  checkIsLeader() {
    const user = sessionStorage.getItem('current-user');
    return JSON.parse(user).isLeader;
  }

  openAddDialog() {

    this.fetchMemberListData();
    this.WorkingOT = {
      ...this.WorkingOTInitState
    };
    this.resetCalendarSelection();
    this.editWorkingOTDialog = false;
    this.addNewWorkingOTDialog = true;
    this.openDialog = true;
  }

  openEditDialog(data: any) {
    this.WorkingOT = {};
    this.WorkingOT = { ...data };
    this.WorkingOT.date = new Date(data.date);
    this.addNewWorkingOTDialog = false;
    this.editWorkingOTDialog = true;
    this.openDialog = true;
  }

  fetchWorkingOTsData(input_filter_datefrom?: string, input_filter_dateto?: string) {
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

    this.https.get<any>("/api/LamViecNgoaiGio", { params: params }).subscribe({
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
    this.WorkingOTs = res.data.map((item: WorkingOT_API_DO) => {
      const { memberId, ...rest } = item;
      const WorkingOT: WorkingOT = {
        ...rest,
        member: {
          id: memberId,
          fullName: '',
          nickName: ''
        },
      };
      return WorkingOT;
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
    this.WorkingOTs.forEach((WorkingOT: WorkingOT) => {
      const foundMember = this.MemberList.find((m: Member) => m.id === WorkingOT.member.id);
      if (foundMember) {
        WorkingOT.member.fullName = foundMember.fullName;
        WorkingOT.member.nickName = foundMember.nickName;
      }
    });
  }

  hideDialog() {
    this.WorkingOT = {};
    this.resetCalendarSelection();
    this.openDialog = false;
    this.editWorkingOTDialog = false;
    this.addNewWorkingOTDialog = false;
  }

  resetCalendarSelection() {
    this.WorkingOT.date = new Date();
    this.WorkingOT.date.setHours(0, 0, 0, 0);
  }

  addNewWorkingOT() {
    let WorkingOTArray: WorkingOT[] = [this.WorkingOT];
    WorkingOTArray.forEach((item: any) => {
      const user = sessionStorage.getItem('current-user');
      if (!JSON.parse(user).isLeader) {
        item.memberId = JSON.parse(user).id
      } else {
        item.memberId = item.member.id;
      }
      delete item.member;
    });

    this.https.post<any>("/api/LamViecNgoaiGio", WorkingOTArray).subscribe({
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
        this.fetchWorkingOTsData();
      }
    });
    this.addNewWorkingOTDialog = false;
    this.WorkingOT = {};
    this.hideDialog();
  }

  updateWorkingOT() {
    let WorkingOTData: any = structuredClone(this.WorkingOT);

    let user = JSON.parse(sessionStorage.getItem('current-user'));

    if (!user.isLeader) {
      WorkingOTData = {
        ...WorkingOTData,
        memberId: user.id
      }
    } else {
      WorkingOTData = {
        ...this.WorkingOT,
        memberId: this.WorkingOT.member.id
      }
    }

    this.https.put<any>("/api/LamViecNgoaiGio/" + this.WorkingOT.id, WorkingOTData).subscribe({
      next: (res: any) => {
        // todo
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.fetchWorkingOTsData();
      }
    });
    this.hideDialog();
  }

  deleteWorkingOT(event: Event, data: any) {
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

        this.https.delete<any>("/api/LamViecNgoaiGio/" + data.id, data).subscribe({
          next: (res: any) => {
            //todo
          },
          error: (error) => {
            console.log(error);
            // Your logic for handling errors
          },
          complete: () => {
            // Your logic for handling the completion event (optional)
            this.fetchWorkingOTsData();
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
    this.fetchWorkingOTsData();
    this.filter_datefrom = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
    this.filter_dateto = new Date().toLocaleDateString('en-GB');
  }


}
