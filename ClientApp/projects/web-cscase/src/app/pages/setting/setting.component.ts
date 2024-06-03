import { Component, OnInit } from '@angular/core';
import { HttpClient } from "@angular/common/http";

@Component({
  selector: 'app-setting',
  templateUrl: './setting.component.html',
  styleUrls: ['./setting.component.scss']
})
export class SettingComponent implements OnInit {
  public linkKhaoSat: string

  constructor(private http: HttpClient) {

  }

  ngOnInit(): void {
    this.http.post<any>("/api/main/view_khao_sat", "").subscribe({
      next: (res: any) => {
        this.linkKhaoSat = res.value
        console.log(res);
        // Your logic for handling the response
        localStorage.setItem("linkhaosat", res.value)
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

  public updateKhaoSat() {
    this.http.post<any>("/api/main/update_khao_sat", { "name": "khaosat", "value": this.linkKhaoSat }).subscribe(
      (res: any) => {
        if (res && res.code === 200) {
          localStorage.setItem("linkhaosat", res.value)
        } else {

        }
      }
    )
  }
}
