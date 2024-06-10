import { Component, OnInit } from '@angular/core';
import { EduCase } from '@mylibs';
import { BsModalRef } from 'ngx-bootstrap/modal';

@Component({
  selector: 'app-noidungcasenew',
  templateUrl: './noidungcasenew.component.html',
  styleUrls: ['./noidungcasenew.component.scss']
})
export class NoidungcasenewComponent implements OnInit {
  public data: EduCase[]; public type: number;
  public datacase: Partial<EduCase> = {};

  constructor(public bsModalRefEditor: BsModalRef,) { }

  ngOnInit(): void {
    if (this.data && this.data.length > 0) {
      this.data[0].thongtinkh = this.data[0].thongtinkh ?? '';
      this.data[0].dapungcongty = this.data[0].dapungcongty ?? ''
      this.datacase = this.data[0];
    }
  }
  public openBase64InNewTab(data, mimeType) {
    var byteCharacters = atob(data);
    var byteNumbers = new Array(byteCharacters.length);
    for (var i = 0; i < byteCharacters.length; i++) {
      byteNumbers[i] = byteCharacters.charCodeAt(i);
    }
    var byteArray = new Uint8Array(byteNumbers);
    var file = new Blob([byteArray], { type: mimeType + ';base64' });
    var fileURL = URL.createObjectURL(file);
    window.open(fileURL);
  }

  public onNoteClick(event) {
    if (event) {
      if (event.target && event.target.currentSrc) {
        let data = event.target.currentSrc;
        let w = window.open('about:blank');
        let image = new Image();
        image.src = data;
        setTimeout(function () { w.document.write(image.outerHTML); }, 0);
      }
    }
  }

  public trackByFn(index: number) { return index; }
}
