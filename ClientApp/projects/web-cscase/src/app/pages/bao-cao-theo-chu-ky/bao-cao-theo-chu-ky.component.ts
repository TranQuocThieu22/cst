import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { NgxSpinnerService } from "ngx-spinner";

@Component({
  selector: 'app-bao-cao-theo-chu-ky',
  templateUrl: './bao-cao-theo-chu-ky.component.html',
  styleUrls: ['./bao-cao-theo-chu-ky.component.scss']
})
export class BaoCaoTheoChuKyComponent implements OnInit {
  baoCaoChuKyDev: any
  baoCaoChuKySup: any
  dataEstimateTimeAndActualTime: any
  dataTongCaseAndLoaiCase: any
  constructor(private https: HttpClient, private spinner: NgxSpinnerService,) { }

  ngOnInit(): void {
    this.fetchSupData()
    this.fetchDevData()
  }
  fetchSupData() {
    this.spinner.show("spinner");
    let body: any = {
      FromDate: "2024-09-02T00:00:00.0000000",

      ToDate: "2024-09-06T00:00:00.0000000"
    };
    this.https.post<any>("/api/BaoCaoTheoChuKy/sup", body).subscribe({
      next: (res: any) => {
        this.baoCaoChuKySup = res.data

        console.log(this.baoCaoChuKySup);

      },
      error: (error) => {
        this.spinner.hide("spinner");
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        this.spinner.hide("spinner");
        // Your logic for handling the completion event (optional)
      }
    });
  }
  fetchDevData() {
    this.spinner.show("spinner");
    let body: any = {
      FromDate: "2024-09-02T00:00:00.0000000",

      ToDate: "2024-09-06T00:00:00.0000000"
    };
    this.https.post<any>("/api/BaoCaoTheoChuKy/dev", body).subscribe({
      next: (res: any) => {
        this.dataEstimateTimeAndActualTime = res.dataEstimateTimeAndActualTime
        this.dataTongCaseAndLoaiCase = res.dataTongCaseAndLoaiCase
        console.log(this.dataEstimateTimeAndActualTime);
        console.log(this.dataTongCaseAndLoaiCase);

      },
      error: (error) => {
        this.spinner.hide("spinner");
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        this.spinner.hide("spinner");
        // Your logic for handling the completion event (optional)
      }
    });
  }
}
