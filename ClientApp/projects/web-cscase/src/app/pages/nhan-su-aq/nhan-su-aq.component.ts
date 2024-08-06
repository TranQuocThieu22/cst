import { Component, OnInit } from '@angular/core';
import { AQMember } from './AQMember';
import { AQRole } from './AQMember';
import { HttpClient } from '@angular/common/http';

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

  dt_filter: any;
  AQRoles: AQRole[];

  constructor(private https: HttpClient) {
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

    this.https.get<any>("/api/ThongTinCaNhan").subscribe({
      next: (res: any) => {
        this.AQmembers = res.data
        console.log(this.AQmembers);

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

  editMember(data: any) {
    // this.product = {...product};
    // this.productDialog = true;
    console.log("edit");
  }
  deleteMember(data: any) {
    // this.confirmationService.confirm({
    //     message: 'Are you sure you want to delete ' + product.name + '?',
    //     header: 'Confirm',
    //     icon: 'pi pi-exclamation-triangle',
    //     accept: () => {
    //         this.products = this.products.filter(val => val.id !== product.id);
    //         this.product = {};
    //         this.messageService.add({severity:'success', summary: 'Successful', detail: 'Product Deleted', life: 3000});
    //     }
    // });
    console.log("delete");
  }

  openNew() {
    this.aqmember = {};
    this.submitted = false;
    this.addNewMemberDialog = true;
  }

  hideDialog() {
    this.addNewMemberDialog = false;
    this.submitted = false;
  }

  saveMember() {

    this.submitted = true;

    this.aqmember.avatar = "avatar content";
    let aqmemberArray: AQMember[] = [this.aqmember];

    console.log(aqmemberArray[0]);


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
  }
}




