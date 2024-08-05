import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-nhan-su-aq',
  templateUrl: './nhan-su-aq.component.html',
  styleUrls: ['./nhan-su-aq.component.scss']
})
export class NhanSuAqComponent implements OnInit {
  public TFSName: string;
  public fullName: string;
  public email: string;
  public phone: string;
  public avatar: string;
  public birthDate: string;
  public startDate: string;
  public nickName: string;
  public role: string;
  public isLeader: boolean;
  public isLunch: boolean;
  public WFHQuota: number;
  public absenceQuota: number;
  public isActive: boolean;
  public nhanSuAq: object[];

  constructor() {
    this.nhanSuAq = [
      {
        id: 1,
        TFSName: "minhlam",
        fullName: "Phạm Minh Lâm",
        email: "lamworkspace74@gmail.com",
        phone: "0901366905",
        avatar: "", // Add avatar URL or base64 string
        birthDate: "01/11/2002",
        startDate: "01/01/2023", // Example start date
        nickName: "Lâm", // Example nickname
        role: "Developer", // Example role
        isLeader: false,
        isLunch: true, // Example value, change as needed
        WFHQuota: 10, // Example quota, change as needed
        absenceQuota: 5, // Example quota, change as needed
        isActive: true
      },
      {
        id: 2,
        TFSName: "quocthieu",
        fullName: "Trần Quốc Thiệu",
        email: "quocthieu@example.com",
        phone: "0901123456",
        avatar: "", // Add avatar URL or base64 string
        birthDate: "02/12/1995", // Example birth date
        startDate: "01/02/2023", // Example start date
        nickName: "Thiệu", // Example nickname
        role: "Tester", // Example role
        isLeader: false,
        isLunch: false, // Example value, change as needed
        WFHQuota: 8, // Example quota, change as needed
        absenceQuota: 4, // Example quota, change as needed
        isActive: true
      },
      {
        id: 3,
        TFSName: "huuluan",
        fullName: "Nguyễn Hữu Luân",
        email: "huuluan@example.com",
        phone: "0902233445",
        avatar: "", // Add avatar URL or base64 string
        birthDate: "03/05/1993", // Example birth date
        startDate: "01/03/2023", // Example start date
        nickName: "Luân", // Example nickname
        role: "PM", // Example role
        isLeader: true,
        isLunch: true, // Example value, change as needed
        WFHQuota: 12, // Example quota, change as needed
        absenceQuota: 6, // Example quota, change as needed
        isActive: true
      }
    ];



  }

  ngOnInit(): void {
  }

}
