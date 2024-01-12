import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
  selector: 'app-edit-rlcase',
  templateUrl: './edit-rlcase.component.html',
  styleUrls: ['./edit-rlcase.component.scss']
})

export class EditRlcaseComponent {

  public version: string; public vesion: string;
  public dtOut: EventEmitter<any> = new EventEmitter();

  constructor(private http: HttpClient, private spinner: NgxSpinnerService, public bsModalRefEditor: BsModalRef) { }

  public save() {
    const body = { filter: { version: this.version, vesion: this.vesion } }

    this.http.post<any>('/api/main/update_case_rl', body)
      .subscribe((res: any) => {
        if (res) {
          this.dtOut.emit({ data: res, res: 200 });
          this.spinner.hide();
          this.bsModalRefEditor.hide();
        }
      });
  }

}
