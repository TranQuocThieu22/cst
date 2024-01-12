import { DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter } from '@angular/core';
import { User } from '@mylibs';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-traodoi',
  templateUrl: './traodoi.component.html',
  styleUrls: ['./traodoi.component.scss']
})


export class TraodoiComponent {
  public traodoiOut: EventEmitter<any> = new EventEmitter();
  public macase: string; public tieu_de: string; public data_comment: string;
  public str_noidung: string = ''; public da_ban_giao: boolean = false;
  public currentUser: User;

  constructor(public bsModalRefEditor: BsModalRef, private http: HttpClient, private spinner: NgxSpinnerService, private toastr: ToastrService,) {
    if (sessionStorage.getItem('current-user')) {
      this.currentUser = JSON.parse(sessionStorage.getItem('current-user'));
    }
  }

  public ngOnInit(): void {

    // call view 1 case
    const body = { filter: { macase: this.macase } }
    this.spinner.show('spinner-td');
    this.http.post<any>('/api/main/viewonecase', body)
      .subscribe((res: any) => {
        if (res && res.data) {
          let temp = res.data?.onecase;
          if (temp && temp[0]) {
            this.data_comment = temp[0]?.fields['AQ.Comment'] || "";
            if (this.data_comment) {
              if (this.data_comment === "0" || this.data_comment === "1") {
                this.data_comment = "";
              }
            }
            if (this.data_comment) {
              this.data_comment = this.data_comment.split('\n').join('</br>');
            }
          }

          this.spinner.hide('spinner-td');
        }
        else { this.spinner.hide('spinner-td'); }
      }, (error) => {
        this.spinner.hide('spinner-td');
      });
  }

  public Send() {

    if (!this.str_noidung || this.str_noidung === '') {
      return;
    }

    let User = "";

    if (this.currentUser.roles.toLowerCase() !== 'admin' && this.currentUser.roles.toLowerCase() !== 'administrator') {
      User = `\n * KH: ${this.currentUser.maTruong} -- ${new DatePipe('en-US').transform(new Date(), 'dd/MM/yyyy HH:mm')} \n `;
    }
    else {
      User = `\n * AQ: ${new DatePipe('en-US').transform(new Date(), 'dd/MM/yyyy HH:mm')} \n `;
    }

    let str_noidungall = "";
    str_noidungall = (this.data_comment ?? this.data_comment + '\n') + User + this.str_noidung;
    str_noidungall = str_noidungall.split('</br>').join('\n');

    const body_up = {
      filter: {
        macase: this.macase.toString(),
        body_update: `[{"op": "add", "path": "/fields/AQ.Comment", "from": null, "value": \"${str_noidungall}\"}]`
      }
    };

    this.http.post('/api/main/updatecase', body_up)
      .subscribe((res3: any) => {
        if (res3 && res3.code === 200) {
          this.str_noidung = "";
          if (str_noidungall) {
            this.data_comment = str_noidungall.split('\n').join('</br>');
            this.traodoiOut.emit({ data: this.data_comment, res: 200 });
          }
        }
      });

  }

}
