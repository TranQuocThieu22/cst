import { Component, OnInit } from '@angular/core';
// import { Router } from '@angular/router';
import { AQMember, AQRole } from './report-ca-nhan-DT';

import { TitleComponent } from 'echarts/components';
import { LegendComponent } from 'echarts/components';
import { RadarChart } from 'echarts/charts';
import { CanvasRenderer } from 'echarts/renderers';
import * as echarts from 'echarts/core';
import {
  TooltipComponent,
  GridComponent,
  DataZoomComponent
} from 'echarts/components';
import { CustomChart, BarChart } from 'echarts/charts';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-report-ca-nhan',
  templateUrl: './report-ca-nhan.component.html',
  styleUrls: ['./report-ca-nhan.component.scss']
})
export class ReportCaNhanComponent implements OnInit {

  user: any;
  AQRoles: AQRole[];

  readonly c1_echartsExtentions: any[];
  c1_echartsOptions: object = {};

  readonly c2_echartsExtentions: any[];
  c2_echartsOptions: object = {};

  // c2 variables
  yearCount = 7;
  categoryCount = 30;
  xAxisData = [];
  customData = [];
  legendData = [];
  dataList = [];
  encodeY = [];
  // c2 variables
  avatarURL: string
  userTotalDayOff_Week = 0;
  userTotalDayOff_Month = 0;

  constructor(
    private https: HttpClient
    // private router: Router

  ) {
    this.AQRoles = [
      { role: 'Developer', code: '1', total: 0 },
      { role: 'Support', code: '2', total: 0 },
      { role: 'Sale', code: '3', total: 0 },
      { role: 'Human Resource', code: '4', total: 0 },
      { role: 'Business Manager', code: '5', total: 0 },
    ];
    this.c1_echartsExtentions = [RadarChart, TitleComponent, LegendComponent, CanvasRenderer]

    this.c2_echartsExtentions = [
      TooltipComponent,
      GridComponent,
      LegendComponent,
      DataZoomComponent,
      CustomChart,
      BarChart,
      CanvasRenderer]
  }

  ngOnInit(): void {

    this.user = JSON.parse(sessionStorage.getItem('current-user')).userData;
    this.avatarURL = `data:image/png;base64,${this.user.avatar}`

    // this.fetchDataC1();
    // this.fetchDataC2();
    this.fetchLunchPaymentReport();
  }

  fetchDataC1() {
    this.c1_echartsOptions = {
      legend: {
        data: ['Mục tiêu', 'Thực tế'],
        right: 0,
        top: 0,
        orient: 'vertical',
        textStyle: {
          fontSize: 10
        }
      },
      radar: {
        indicator: [
          { name: 'Kinh doanh', max: 6500 },
          { name: 'Quản trị', max: 16000 },
          { name: 'Lập trình', max: 30000 },
          { name: 'Khởi nghiệp', max: 38000 },
          { name: 'Kiểm thử PM', max: 25000 },
          { name: 'Làm việc nhóm', max: 25000 },
          { name: 'Ăn nhậu', max: 25000 },
          { name: 'CSKH', max: 22000 }
        ],
        center: ['50%', '55%'],
        radius: '65%',
      },
      series: [
        {
          name: 'Budget vs spending',
          type: 'radar',
          data: [
            {
              value: [4200, 3000, 20000, 35000, 20000, 20000, 20000, 18000],
              name: 'Mục tiêu'
            },
            {
              value: [5000, 14000, 28000, 26000, 12000, 20000, 20000, 21000],
              name: 'Thực tế'
            }
          ]
        }
      ]
    };
  }

  fetchDataC2() {
    this.legendData.push('trend');
    for (var i = 0; i < this.yearCount; i++) {
      this.legendData.push(2010 + i + '');
      this.dataList.push([]);
      this.encodeY.push(1 + i);
    }
    console.log(this.legendData);

    for (var i = 0; i < this.categoryCount; i++) {
      var val = Math.random() * 1000;
      this.xAxisData.push('category' + i);
      var customVal = [i];
      this.customData.push(customVal);
      for (var j = 0; j < this.dataList.length; j++) {
        var value =
          j === 0
            ? echarts.number.round(val, 2)
            : echarts.number.round(
              Math.max(0, this.dataList[j - 1][i] + (Math.random() - 0.5) * 200),
              2
            );
        this.dataList[j].push(value);
        customVal.push(value);
      }
    }
    this.c2_echartsOptions = {
      tooltip: {
        trigger: 'axis'
      },
      legend: {
        data: this.legendData
      },
      dataZoom: [
        {
          type: 'slider',
          start: 50,
          end: 70
        },
        {
          type: 'inside',
          start: 50,
          end: 70
        }
      ],
      xAxis: {
        data: this.xAxisData
      },
      yAxis: {},
      series: [
        {
          type: 'custom',
          name: 'trend',
          renderItem: function (params, api) {
            var xValue = api.value(0);
            var currentSeriesIndices = api.currentSeriesIndices();
            var barLayout = api.barLayout({
              barGap: '30%',
              barCategoryGap: '20%',
              count: currentSeriesIndices.length - 1
            });
            var points = [];
            for (var i = 0; i < currentSeriesIndices.length; i++) {
              var seriesIndex = currentSeriesIndices[i];
              if (seriesIndex !== params.seriesIndex) {
                var point = api.coord([xValue, api.value(seriesIndex)]);
                point[0] += barLayout[i - 1].offsetCenter;
                point[1] -= 20;
                points.push(point);
              }
            }
            var style = api.style({
              stroke: api.visual('color'),
              fill: 'none'
            });
            return {
              type: 'polyline',
              shape: {
                points: points
              },
              style: style
            };
          },
          itemStyle: {
            borderWidth: 2
          },
          encode: {
            x: 0,
            y: this.encodeY
          },
          data: this.customData,
          z: 100
        },
        ...this.dataList.map(function (data, index) {
          return {
            type: 'bar',
            animation: false,
            name: this.legendData[index + 1],
            itemStyle: {
              opacity: 0.5
            },
            data: data
          };
        })
      ]
    };

  }

  fetchLunchPaymentReport(selectedMonthYear?: any) {
    let params: any = {};
    // if (selectedMonthYear) {
    //   params = {
    //     month: selectedMonthYear.getMonth() + 1,
    //     year: selectedMonthYear.getFullYear()
    //   };
    // }

    const currentDate = new Date();
    const currentMonth = currentDate.getMonth() + 1;
    const currentYear = currentDate.getFullYear();
    params = {
      month: currentMonth,
      year: currentYear,
      query_memberId: this.user.id,
    };
    console.log(params);

    this.https.get<any>("/api/BaoBieuThongKe/ThongKeNgayNghiCaNhan", { params: params }).subscribe({
      next: (res: any) => {
        console.log(res);

        this.userTotalDayOff_Month = res.data[0].userTotalDayOff_Month;

        // this.userTotalDayOff_Month = res.data.userTotalDayOff_Month;
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        console.log(this.userTotalDayOff_Month);
      }
    });
  }

  getRoleName(code: string): string {
    const roleObj = this.AQRoles.find(role => role.code === code);
    return roleObj ? roleObj.role : 'Unknown';
  }

  getRoleCode(code: string): string {
    const roleObj = this.AQRoles.find(role => role.code === code);
    return roleObj ? roleObj.code : 'Unknown';
  }


  public NavigateNhanSuAq() {
    // this.router.navigate(['main/nhansuaq']);
  }

}
