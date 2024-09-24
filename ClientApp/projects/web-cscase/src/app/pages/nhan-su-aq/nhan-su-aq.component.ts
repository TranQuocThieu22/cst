import { Component, OnInit } from '@angular/core';
import {
  AQMember, AQMemberUpdateDO, AQMemberInsertDO,
  detailContract
} from './AQMember';
import { AQRole } from './AQMember';
import { HttpClient } from '@angular/common/http';
import {
  ConfirmationService,
  MessageService,
  PrimeNGConfig
} from "primeng/api";
import { Table } from 'primeng/table';
import { PieChart } from "echarts/charts";
import { TitleComponent, TooltipComponent, LegendComponent } from "echarts/components";


@Component({
  selector: 'app-nhan-su-aq',
  templateUrl: './nhan-su-aq.component.html',
  styleUrls: ['./nhan-su-aq.component.scss']
})


export class NhanSuAqComponent implements OnInit {

  AQmembers: AQMember[];

  detailContractInsert: detailContract = {
    contractStartDate: new Date(),
    contractExpireDate: new Date(),
    contractDuration: 1,
    contractType: ""
  }

  aqmemberInsert: AQMemberInsertDO = {
    detailContract: this.detailContractInsert
  }

  detailContractUpdate: detailContract;

  aqmemberUpdate: AQMemberUpdateDO;

  addNewMemberDialog: boolean;
  editMemberDialog: boolean;
  deleteMemberDialog: boolean;
  openDialog: boolean;

  dt_filter: any;
  AQRoles: AQRole[];

  //data pie chart
  data_RolePieChart: any;
  role_chartOptions: any;

  //pie chart 2
  readonly pc2_echartsExtentions: any[];
  pc2_echartsOptions: object = {};

  constructor(
    private https: HttpClient,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private primengConfig: PrimeNGConfig,
  ) {
    this.AQRoles = [
      { role: 'Developer', code: '1', total: 0 },
      { role: 'Support', code: '2', total: 0 },
      { role: 'Sale', code: '3', total: 0 },
      { role: 'HR', code: '4', total: 0 },
      { role: 'BM', code: '5', total: 0 },
    ];
    this.pc2_echartsExtentions = [PieChart, TitleComponent, TooltipComponent, LegendComponent]
  }

  ngOnInit(): void {
    this.constructor;
    this.primengConfig.ripple = true;
    this.fetchAQMemberData();
  }

  checkIsLeader() {
    let user = sessionStorage.getItem('current-user');
    return JSON.parse(user).isLeader
  }

  handleUploadAvatar(event) {
    if (event.target.files && event.target.files[0]) {
      var reader = new FileReader();
      reader.readAsDataURL(event.target.files[0]); // read file as data url
      reader.onload = (event) => {
        // called once readAsDataURL is completed
        if (this.openAddDialog) this.aqmemberInsert.avatar = event.target.result as string;
        if (this.openEditDialog) this.aqmemberUpdate.avatar = event.target.result as string;
      };
    }
  }
  deleteInputImg() {
    if (this.aqmemberInsert) this.aqmemberInsert.avatar = null;
    if (this.aqmemberUpdate) this.aqmemberUpdate.avatar = null;
  }

  fetchAQMemberData() {
    this.https.get<any>("/api/ThongTinCaNhan").subscribe({
      next: (res: any) => {
        this.AQmembers = res.data;
        this.sortInitData();
        // this.AQmembers.forEach(member => {
        //   if (member.detailContract === null) {
        //     member.detailContract = {
        //       contractStartDate: new Date(),
        //       contractExpireDate: new Date(),
        //       contractDuration: 1
        //     }
        //   }
        //   if (member.detailLunch === null) {
        //     member.detailLunch = [{
        //       year: new Date().getFullYear(),
        //       lunchByMonth: [{
        //         month: new Date().getMonth() + 1,
        //         isLunch: false,
        //         lunchFee: 0,
        //         note: ""
        //       }]
        //     }]
        //   }
        // });

        // console.log(this.AQmembers);
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.AQRoles.forEach(role => {
          role.total = this.AQmembers.filter(member => member.role === role.code).length;
        });
        this.fetchDataPC2();
      }
    });
  }

  fetchDataPC2() {
    this.pc2_echartsOptions = {
      title: {
        text: 'Nhân sự AQ',
        subtext: 'Tỉ lệ và số lượng nhân viên các phòng ban',
        left: 'center',
        textStyle: {
          fontFamily: 'Arial, sans-serif',
          fontSize: 20,
          fontWeight: 'bold'
        }
      },
      tooltip: {
        trigger: 'item',
        formatter: '{b}: {c} ({d}%)' // Display label, value, and percentage in tooltip
      },
      legend: {
        orient: 'vertical',
        bottom: 'bottom'
      },
      series: [
        {
          name: 'Phòng ban',
          type: 'pie',
          radius: '50%',
          data: this.AQRoles.map((role, index) => ({
            value: role.total,
            name: role.role,
            itemStyle: {
              color: this.getColor(index) // Call getColor function to get color for each label
            },
            label: {
              show: true,
              formatter: '{b}: {c} ({d}%)' // Display label and percentage in label
            },
            emphasis: {
              itemStyle: {
                shadowBlur: 10,
                shadowOffsetX: 0,
                shadowColor: 'rgba(0, 0, 0, 0.5)'
              },
              label: {
                show: true,
                formatter: '{b}: {c} ({d}%)' // Display label and percentage in emphasis
              }
            }
          })),
        }
      ]
    };
  }

  getColor(index: number): string {
    const colors = [
      "#b3e5fc",
      "#ffd8b2",
      "#8dbca1",
      "#fe1155",
      "#0a3d62"
    ];
    return colors[index % colors.length];
  }

  getRoleName(code: string): string {
    const roleObj = this.AQRoles.find(role => role.code === code);
    return roleObj ? roleObj.role : 'Unknown';
  }

  sortInitData() {
    this.AQmembers.sort((a, b) => {
      const roleA = new Date(a.role);
      const roleB = new Date(b.role);
      return roleA < roleB ? 1 : -1;
    });
  }

  openAddDialog() {
    this.aqmemberInsert = {
      detailContract: this.detailContractInsert
    };
    this.editMemberDialog = false;
    this.addNewMemberDialog = true;
    this.openDialog = true;
  }

  openEditDialog(data: any) {
    this.aqmemberUpdate = {};
    this.aqmemberUpdate = structuredClone(data);

    this.aqmemberUpdate = {
      ...this.aqmemberUpdate,
      birthDate: new Date(data.birthDate),
      startDate: new Date(data.startDate),
      detailContract: data.detailContract === null ?
        {
          contractStartDate: new Date(),
          contractExpireDate: new Date(),
          contractDuration: 0,
          contractType: ""
        }
        : {
          ...this.aqmemberUpdate.detailContract,
          contractStartDate: new Date(data.detailContract.contractStartDate),
          contractExpireDate: new Date(data.detailContract.contractExpireDate)
        }
    };
    this.addNewMemberDialog = false;
    this.editMemberDialog = true;
    this.openDialog = true;
  }

  handleIsLunchStatusChange(event) {
    this.aqmemberUpdate.isLunchStatus = event.checked;
    const currentYear = new Date().getFullYear();
    const detailLunchCurrentYear = this.aqmemberUpdate.detailLunch.find(item => item.year === currentYear);

    if (detailLunchCurrentYear) {
      detailLunchCurrentYear.lunchByMonth.forEach(lunchByMonth => {
        lunchByMonth.isLunch = event.checked;
      });
    }
  }

  openDeleteDialog(data: any) {
    this.deleteMemberDialog = true;
  }

  hideDialog() {
    this.openDialog = false;
    this.editMemberDialog = false;
    this.addNewMemberDialog = false;
  }

  addNewMember() {
    if (this.aqmemberInsert.avatar === undefined || this.aqmemberInsert.avatar === '') {
      this.aqmemberInsert.avatar = null;
    }
    let aqmemberArray: AQMemberInsertDO[] = [this.aqmemberInsert];

    this.https.post<any>("/api/ThongTinCaNhan", aqmemberArray).subscribe({
      next: (res: any) => {
        console.log(res);
        this.AQmembers.push(res.data[0]);
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        console.log(this.AQmembers);

      }
    });

    this.AQmembers = [...this.AQmembers];
    this.addNewMemberDialog = false;
    this.aqmemberInsert = {};
    this.deleteInputImg();
    this.hideDialog();
  }

  updateMember() {
    console.log('before: ', this.AQmembers);

    this.https.put<any>("/api/ThongTinCaNhan/" + this.aqmemberUpdate.id, this.aqmemberUpdate).subscribe({
      next: (res: any) => {
        const index = this.AQmembers.findIndex(member => member.id === this.aqmemberUpdate.id);
        if (index !== -1) {
          this.AQmembers[index] = res.data;
        }
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


  deleteMember(event: Event, data: any) {
    this.confirmationService.confirm({
      target: event.target,
      message: "Xóa bạn này khỏi công ty?",
      acceptLabel: 'Xóa',
      rejectLabel: 'Quay lại',
      icon: "pi pi-exclamation-triangle",
      accept: () => {
        this.messageService.add({
          severity: "info",
          summary: "Xóa thành công",
          detail: "Đã xóa tài khoản khỏi hệ thống"
        });

        this.https.delete<any>("/api/ThongTinCaNhan/" + data.id, data).subscribe({
          next: (res: any) => {
            // console.log(res);
            this.AQmembers = this.AQmembers.filter(val => val.id !== data.id);
          },
          error: (error) => {
            console.log(error);
            // Your logic for handling errors
          },
          complete: () => {
            // Your logic for handling the completion event (optional)
            this.fetchAQMemberData();
          }
        });
      },
      reject: () => {
        this.messageService.add({
          severity: "error",
          summary: "Hủy xóa tài khoản",
          detail: "Hủy xóa tài khoản khỏi hệ thống"
        });
      }
    });
  }

  clear(table: Table) {
    table.clear();
    this.fetchAQMemberData();
  }
}

function hexToRgb(hex: string): { r: number; g: number; b: number } {
  const bigint = parseInt(hex, 16);
  const r = (bigint >> 16) & 255;
  const g = (bigint >> 8) & 255;
  const b = bigint & 255;
  return { r, g, b };
}

