import { Component } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-mess-box',
  templateUrl: './mess-box.component.html',
  styleUrls: ['./mess-box.component.scss']
})

export class MessBoxComponent {

  public onConfirm: Function = undefined;
  public data: any;
  public icon = '';
  public message = '';
  public titleIcon = '';
  public button = '';

  constructor(public bsModalRef: BsModalRef) { }

  public ngOnInit() {
    if (this.icon === 'q') {
      this.titleIcon = '<i class="fas fa-question-circle fa-2x text-danger pr-2"></i>';
    }
    else if (this.icon === 'w') {
      this.titleIcon = '<i class="fas fa-exclamation-circle fa-2x text-warning pr-2"></i>';
    }
    else if (this.icon === 'i') {
      this.titleIcon = '<i class="fas fa-info-circle fa-2x text-info pr-2"></i>';
    }
    else if (this.icon === 'n') {
      this.titleIcon = '<i class="fas fa-globe fa-2x text-primary pr-2"></i>';
    }
    else if (this.icon === 'e') {
      this.titleIcon = '<i class="far fa-file-excel fa-2x text-primary pr-2"></i>';
    }
    else {
      this.titleIcon = '';
    }
  }

  public doConfirm() {
    if (this.onConfirm) {
      this.onConfirm(this.data);
    }
    this.bsModalRef.hide();
  }
}
