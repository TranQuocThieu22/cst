import { Component, OnInit } from '@angular/core';
import { EduCase } from '@mylibs';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-chitietcase',
  templateUrl: './chitietcase.component.html',
  styleUrls: ['./chitietcase.component.scss']
})

export class ChitietcaseComponent implements OnInit {

  public data: EduCase[]; public messError: string = '';
  public orderby = 'macase'; public value: string; public reverseOrderby = false;

  constructor(public bsModalRefEditor: BsModalRef,) { }

  public ngOnInit(): void {
    this.messError = '';
    this.value = 'macase';
    if (!this.data || this.data.length <= 0) {
      this.messError = 'Không tìm thấy dữ liệu.';
    }
  }

  public async setOrder(value: string) {
    if (this.data) {
      if (this.orderby === value) {
        this.reverseOrderby = !this.reverseOrderby;
      }
      this.orderby = value;

      if (value === 'macase') {
        if (this.reverseOrderby) {
          this.data = this.data.sort((a, b) =>
            Number(a.macase) < Number(b.macase) ? -1 : Number(a.macase) > Number(b.macase) ? 1 : 0);
        }
        else {
          this.data = this.data.sort((a, b) =>
            Number(a.macase) < Number(b.macase) ? 1 : Number(a.macase) > Number(b.macase) ? -1 : 0);
        }
      }

      if (value === 'ngaynhan') {
        if (this.reverseOrderby) {
          this.data = this.data.sort((a, b) =>
            new Date(a.ngaynhan).getTime() > new Date(b.ngaynhan).getTime() ? 1 : -1);
        }
        else {
          this.data = this.data.sort((a, b) =>
            new Date(a.ngaynhan).getTime() > new Date(b.ngaynhan).getTime() ? -1 : 1);
        }
      }

      if (value === 'trangthai') {
        if (this.reverseOrderby) {
          this.data = this.data.sort((a, b) =>
            a.trangthai < b.trangthai ? -1 : a.trangthai > b.trangthai ? 1 : 0);
        }
        else {
          this.data = this.data.sort((a, b) =>
            a.trangthai < b.trangthai ? 1 : a.trangthai > b.trangthai ? -1 : 0);
        }
      }

      // if (value === 'ngaydukien') {
      //   if (this.reverseOrderby) {
      //     this.data = this.data.sort((a, b) =>
      //       new Date(a.ngaydukien).getTime() > new Date(b.ngaydukien).getTime() ? 1 : -1);
      //   }
      //   else {
      //     this.data = this.data.sort((a, b) =>
      //       new Date(a.ngaydukien).getTime() > new Date(b.ngaydukien).getTime() ? -1 : 1);
      //   }
      // }

    }
  }

  public trackByFn(index: number) { return index; }
}
