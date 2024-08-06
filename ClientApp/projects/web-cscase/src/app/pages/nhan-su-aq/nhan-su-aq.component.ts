import { Component, OnInit } from '@angular/core';
import { AQMember } from './AQMember';
import { AQRole } from './AQMember';
import { HttpClient } from '@angular/common/http';
import {
  ConfirmationService,
  MessageService,
  PrimeNGConfig
} from "primeng/api";

@Component({
  selector: 'app-nhan-su-aq',
  templateUrl: './nhan-su-aq.component.html',
  styleUrls: ['./nhan-su-aq.component.scss']
})


export class NhanSuAqComponent implements OnInit {

  AQmembers: AQMember[];
  aqmember: AQMember;
  submitted: boolean;
  addNewMemberDialog: boolean;
  editMemberDialog: boolean;
  deleteMemberDialog: boolean;
  openDialog: boolean;

  dt_filter: any;
  AQRoles: AQRole[];

  constructor(
    private https: HttpClient,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private primengConfig: PrimeNGConfig
  ) {
    this.AQRoles = [
      { role: 'Dev', code: '1' },
      { role: 'Support', code: '2' },
      { role: 'Sale', code: '3' },
      { role: 'HR', code: '4' },
      { role: 'BM', code: '5' },
    ];
  }

  ngOnInit(): void {
    this.constructor;
    this.primengConfig.ripple = true;

    this.https.get<any>("/api/ThongTinCaNhan").subscribe({
      next: (res: any) => {
        this.AQmembers = res.data
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

  openAddDialog() {
    this.aqmember = {};
    this.submitted = false;
    this.editMemberDialog = false;
    this.addNewMemberDialog = true;
    this.openDialog = true;
    console.log(this.aqmember);
  }

  openEditDialog(data: any) {
    this.aqmember = {};
    this.aqmember = { ...data };
    this.submitted = false;
    this.addNewMemberDialog = false;
    this.editMemberDialog = true;
    this.openDialog = true;
    console.log(this.aqmember);
  }

  openDeleteDialog(data: any) {
    this.deleteMemberDialog = true;
  }

  hideDialog() {
    this.openDialog = false;
    this.editMemberDialog = false;
    this.addNewMemberDialog = false;
    this.submitted = false;
  }

  addNewMember() {
    this.submitted = true;
    this.aqmember.avatar = "avatar content";
    let aqmemberArray: AQMember[] = [this.aqmember];

    this.https.post<any>("/api/ThongTinCaNhan/Insert", aqmemberArray).subscribe({
      next: (res: any) => {
        // console.log(res);
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
      }
    });

    this.AQmembers = [...this.AQmembers];
    this.addNewMemberDialog = false;
    this.aqmember = {};

    this.https.get<any>("/api/ThongTinCaNhan").subscribe({
      next: (res: any) => {
        this.AQmembers = res.data
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
      }
    });
    this.hideDialog();
  }

  updateMember() {
    this.https.put<any>("/api/ThongTinCaNhan/" + this.aqmember.id, this.aqmember).subscribe({
      next: (res: any) => {
        // console.log(res);
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


  confirm(event: Event, data: any) {
    console.log('delelte clicked', data);

    this.confirmationService.confirm({
      target: event.target,
      message: "Xóa bạn này khỏi công ty?",
      icon: "pi pi-exclamation-triangle",
      accept: () => {
        this.messageService.add({
          severity: "info",
          summary: "Confirmed",
          detail: "You have accepted"
        });

        this.https.delete<any>("/api/ThongTinCaNhan/" + data.id, data).subscribe({
          next: (res: any) => {
            // console.log(res);
          },
          error: (error) => {
            console.log(error);
            // Your logic for handling errors
          },
          complete: () => {
            // Your logic for handling the completion event (optional)
          }
        });
      },
      reject: () => {
        this.messageService.add({
          severity: "error",
          summary: "Rejected",
          detail: "You have rejected"
        });
      }
    });
  }
}




