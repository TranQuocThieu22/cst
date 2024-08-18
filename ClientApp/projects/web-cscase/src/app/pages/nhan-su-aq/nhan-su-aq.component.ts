import { Component, OnInit } from '@angular/core';
import { AQMember } from './AQMember';
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
  aqmember: AQMember;
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

  fetchAQMemberData() {
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
        this.AQRoles.forEach(role => {
          role.total = this.AQmembers.filter(member => member.role === role.code).length;
        });
        this.fetchDataPieChart();
        this.fetchDataPC2();
      }
    });
  }

  fetchDataPieChart() {
    this.data_RolePieChart = {
      labels: this.AQRoles.map(role => role.role),
      datasets: [
        {
          data: this.AQRoles.map(role => role.total),
          backgroundColor: [
            "#b3e5fc",
            "#ffd8b2",
            "#8dbca1",
            "#fe1155",
            "#0a3d62"
          ],
          hoverBackgroundColor: [
            "#b3e5fc",
            "#ffd8b2",
            "#8dbca1",
            "#fe1155",
            "#0a3d62"
          ]
        }
      ]
    };

    this.role_chartOptions = {
      plugins: {
        legend: {
          labels: {
            color: '#495057'
          }
        },
        labels: {
          render: 'percentage',
          fontColor: function (data) {
            var rgb = hexToRgb(data.dataset.backgroundColor[data.index]);
            var threshold = 140;
            var luminance = 0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b;
            return luminance > threshold ? 'black' : 'white';
          },
          precision: 2
        }
      }
    };

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

  openAddDialog() {
    this.aqmember = {};
    this.editMemberDialog = false;
    this.addNewMemberDialog = true;
    this.openDialog = true;
    console.log(this.aqmember);
  }

  openEditDialog(data: any) {
    this.aqmember = {};
    this.aqmember = { ...data };
    this.addNewMemberDialog = false;
    this.editMemberDialog = true;
    this.openDialog = true;
    console.log(this.aqmember);
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
    this.aqmember.avatar = "avatar content";
    let aqmemberArray: AQMember[] = [this.aqmember];

    this.https.post<any>("/api/ThongTinCaNhan", aqmemberArray).subscribe({
      next: (res: any) => {
        // console.log(res);
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

    this.AQmembers = [...this.AQmembers];
    this.addNewMemberDialog = false;
    this.aqmember = {};
    this.hideDialog();
  }

  updateMember() {
    this.https.put<any>("/api/ThongTinCaNhan/" + this.aqmember.id, this.aqmember).subscribe({
      next: (res: any) => {
        // console.log(res);
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
  }
}

function hexToRgb(hex: string): { r: number; g: number; b: number } {
  const bigint = parseInt(hex, 16);
  const r = (bigint >> 16) & 255;
  const g = (bigint >> 8) & 255;
  const b = bigint & 255;
  return { r, g, b };
}

