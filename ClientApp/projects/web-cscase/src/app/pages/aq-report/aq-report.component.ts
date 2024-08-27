import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { NgxSpinnerService } from "ngx-spinner";
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexXAxis,
  ApexDataLabels,
  ApexStroke,
  ApexYAxis,
  ApexTitleSubtitle,
  ApexLegend,
  ApexPlotOptions,
  ApexFill,
  ApexMarkers,
  ApexTheme,
  ApexNonAxisChartSeries,
  ApexResponsive,
  ApexStates,
  ApexTooltip

} from "ng-apexcharts";
import { KeyValue } from '@angular/common';
import { forkJoin } from 'rxjs';

export type ChartOptions = {
  theme: ApexTheme;
  series: ApexAxisChartSeries | ApexNonAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  dataLabels: ApexDataLabels;
  yaxis: ApexYAxis;
  title: ApexTitleSubtitle;
  legend: ApexLegend;
  stroke: ApexStroke;
  plotOptions: ApexPlotOptions;
  fill: ApexFill;
  responsive: ApexResponsive[];
  labels: string[];
  states: ApexStates;
  tooltip: ApexTooltip;
  marker: ApexMarkers

};

interface ChartData {
  chartOptions: Partial<ChartOptions>;
  updateFunction: () => void;
  title: string;
  id: number;
}

@Component({
  selector: 'app-aq-report',
  templateUrl: './aq-report.component.html',
  styleUrls: ['./aq-report.component.scss']
})
export class AqReportComponent implements OnInit {

  datePickerValue: string = new Date().toISOString().split('T')[0];  // Initialize with current date
  formattedDate: string = this.formatDate(this.datePickerValue);
  public yAxisBieuDoTienDoXuLyCasesDev: string = 'Cases';
  public yAxisBieuDoPhanBoThoiGianDev: string = 'Giờ'
  public generalColor = ["#1f77b4", "#ff7f0e", "#d62728", "#9467bd", "#2ca02c", "#8c564b", "#e377c2", "#7f7f7f"];
  public generalColor2 = ["#1f77b4", "#d62728", "#ff7f0e", "#9467bd", "#2ca02c", "#8c564b", "#e377c2", "#7f7f7f"];
  public generalColor3 = ["#2ca02c", "#1f77b4", "#d62728", "#ff7f0e", "#9467bd", "#8c564b", "#e377c2", "#7f7f7f"];
  public charts: { [key: string]: ChartData, } = {};
  public AqReport_TongCase: number[] = [];
  public AqReport_treHan: number[] = [];
  public AqReport_conHan: number[] = [];
  public AqReport_nhanSu: number[] = [];
  public AqReport_name: number[] = [];
  public BieuDoPhanBoSoCaseTheoThoiGianCho_data: number[] = [];
  public BieuDoPhanBoSoCaseTheoThoiGianCho_ten: string[] = [];
  public BieuDoPhanBoSoCaseTheoThoiGianCho_count: number = 0;
  public assignedToListDev: string[] = [];
  public assignedToListSup: string[] = [];
  public CanXuLyListDev: number[] = [];
  public xuLyTreListDev: number[] = [];
  public soCaseTrongNgayListDev: number[] = [];
  public tgCanXyLyListDev: number[] = [];
  public luongGioTrongNgayListDev: number[] = [];
  public canXuLySup: number[] = [];
  public xuLyTreSup: number[] = [];
  public phanTichTreSup: number[] = [];
  public testTreSup: number[] = [];
  public CaseDaLamTrongNgaySup: number[] = [];

  constructor(private spinner: NgxSpinnerService, private http: HttpClient) {
    this.initializeCharts();
  }

  ngOnInit(): void {
    this.fetchAllData();
  }

  onDateChange(newDate: string) {
    this.formattedDate = this.formatDate(newDate);
    console.log(newDate);
    this.fetchAllData();
    console.log("Formatted Date:", this.formattedDate);
  }



  private formatDate(dateStr: string): string {
    const date = new Date(dateStr);

    // Get the components of the date
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');

    // Format the date to the desired string format
    return `${year}-${month}-${day}T00:00:00.0000000`;
  }


  initializeCharts() {
    this.charts = {
      BieuDoTienDoXyLyCasesDev: {
        chartOptions: this.getBaseChartOptions(this.yAxisBieuDoTienDoXuLyCasesDev),
        updateFunction: () => this.updateBieuDoTienDoXyLyCasesDev(),
        title: 'Biểu đồ tiến độ xử lý case',
        id: 0
      },
      BieuDoPhanBoThoiGianDev: {
        chartOptions: this.getBaseChartOptions(this.yAxisBieuDoPhanBoThoiGianDev),
        updateFunction: () => this.updateBieuDoPhanBoThoiGianDev(),
        title: 'Biểu đồ phân bổ thời gian',
        id: 1
      },
      BieuDoTienDoXyLyCasesSup: {
        chartOptions: this.getBaseChartOptions(this.yAxisBieuDoTienDoXuLyCasesDev),
        updateFunction: () => this.updateBieuDoTienDoCongViecSup(),
        title: 'Biểu đồ tiến độ công việc',
        id: 2
      },
      BieuDoPhanBoTienDoXuLy: {
        chartOptions: this.getPieChartOptions(),
        updateFunction: () => this.updateBieuDoTronCuaSup(),
        title: 'Biểu đồ phân bổ tiến độ xử lý',
        id: 3
      },
      BieuDoPhanBoSoCaseTheoThoiGianCho: { // Added new chart
        chartOptions: this.getPieChartOptions(),
        updateFunction: () => this.updateBieuDoPhanBoSoCaseTheoThoiGianCho(),
        title: 'Phân bố số case theo thời gian chờ',
        id: 4
      },
      BieuDoPhanBoSoCaseTheoThoiGianCho2: {
        chartOptions: this.getBaseChartOptions(this.yAxisBieuDoTienDoXuLyCasesDev),
        updateFunction: () => this.updateBieuDoPhanBoSoCaseTheoThoiGianCho2(),
        title: 'Phân bố số case theo thời gian chờ',
        id: 5
      },
      BieuDoTongHopAQReport: {
        chartOptions: this.getBaseChartOptions('Số lượng'),
        updateFunction: () => this.updateBieuDoTongHopAQReport(),
        title: 'Biểu đồ tổng hợp AQ Report',
        id: 6
      },
      BieuDoDEVReport: {
        chartOptions: this.getBaseChartOptions('Số lượng'),
        updateFunction: () => this.updateBieuDoDEVReport(),
        title: 'Biểu đồ DEV Report',
        id: 7
      },
      BieuDoSUPReport: {
        chartOptions: this.getBaseChartOptions('Số lượng'),
        updateFunction: () => this.updateBieuDoSUPReport(),
        title: 'Biểu đồ SUP Report',
        id: 8
      }

      // Add more charts here as needed
    };

  }
  getPieChartOptions(): Partial<ChartOptions> {
    return {
      series: [],
      chart: {
        type: "pie",
        height: 500
      },
      responsive: [{
        breakpoint: 480,
        options: {
          chart: {
            width: 200
          },
          legend: {
            position: "bottom"
          }
        }
      }],
      labels: [],
      legend: {
        position: 'bottom',
        fontSize: '20px',

        markers: {
          fillColors: this.generalColor,
        }
      },

      dataLabels: {
        enabled: true,
        style: {
          colors: ["#000000"]
        },
        background: {
          foreColor: "#E8E8E8",
          enabled: true,

        },
        formatter: function (val: string | number | number[], opts) {
          const value = typeof val === 'number' ? val : Number(val);
          if (!isNaN(value)) {

            // return `${this.canXuLySup[opts.seriesIndex]}: ${this.canXuLySup[opts.seriesIndex]} (${value.toFixed(1)}%)`;
            return `${opts.w.config.labels[opts.seriesIndex]}: ${opts.w.config.series[opts.seriesIndex]}( ${value.toFixed(1)}%)`
          }
          return opts.w.config.series[opts.seriesIndex].toString();

        }
      },
      fill: {
        opacity: 1,
        colors: this.generalColor
      },
      states: {
        hover: {
          filter: {
            type: 'none',
            value: 0.15
          }
        }
      },

      tooltip: {

        fillSeriesColor: false,
        onDatasetHover: {
          highlightDataSeries: true
        },
        marker: {
          show: false,

        },

      }
    };
  }

  getBaseChartOptions(yaxis: string): Partial<ChartOptions> {
    return {
      series: [],
      chart: {
        type: "bar",
        height: 700,
      },

      plotOptions: {
        bar: {
          horizontal: false,
          columnWidth: "55%",


          dataLabels: {
            position: "top",

          }
        }
      },

      dataLabels: {
        enabled: true,
        offsetY: -20,
        textAnchor: "middle",
        style: {
          colors: this.generalColor
        }
      },
      stroke: {
        show: true,
        width: 2,
        colors: ["transparent"]
      },
      xaxis: {
        categories: [],
        labels: {
          style: {
            fontSize: '12px',
          }
        }
      },
      yaxis: {
        max: function (max) {
          return max * 1.1; // Add 10% padding to the top
        },
        title: {
          text: yaxis,
          style: {
            fontSize: '15px',
          },

        }
      },
      fill: {
        opacity: 1,
        colors: this.generalColor
      },
      legend: {
        position: "bottom",
        fontSize: '20px',
        markers: {
          fillColors: this.generalColor,
        }
      },

      tooltip: {
        fillSeriesColor: false
      }
    };
  }

  fetchAllData() {
    this.spinner.show("spinner");

    forkJoin({
      coderData: this.http.post('/api/thongke/w-coderCaseReport', { data: this.formattedDate }),
      supData: this.http.post('/api/thongke/w-supCaseReport', {}),
      aqData: this.http.post('/api/thongke/w-AqCaseReport', {}),
      phanBoData: this.http.post('/api/thongke/w-PhanBoSoCaseTheoThoiGianChoCoder', {})
    }).subscribe(
      (results: any) => {
        this.processCoderData(results.coderData);
        this.processSupData(results.supData);
        this.processAqData(results.aqData);
        this.processPhanBoData(results.phanBoData);

        this.updateAllCharts();
        this.spinner.hide("spinner");
      },
      (error) => {
        console.error('Error fetching data:', error);
        this.spinner.hide("spinner");
      }
    );
  }

  private processCoderData(res: any) {
    if (res && res.code === 200 && res.data) {
      this.assignedToListDev = res.data.map(item => item.assignedto.split(' ')[0] || '');
      this.CanXuLyListDev = res.data.map(item => item.canXuLy || 0);
      this.xuLyTreListDev = res.data.map(item => item.xuLyTre || 0);
      this.soCaseTrongNgayListDev = res.data.map(item => item.soCaseTrongNgay || 0);
      this.tgCanXyLyListDev = res.data.map(item => item.tgCanXyLy || 0);
      this.luongGioTrongNgayListDev = res.data.map(item => item.luongGioTrongNgay || 0);
    } else {
      console.error('Invalid response format or empty data for coder data');
    }
  }

  private processSupData(res: any) {
    if (res && res.code === 200 && res.data) {
      this.assignedToListSup = res.data.map(item => item.assignedto.split(' ')[0] || '');
      this.canXuLySup = res.data.map(item => item.canXuLy || 0);
      this.xuLyTreSup = res.data.map(item => item.xuLyTre || 0);
      this.phanTichTreSup = res.data.map(item => item.phanTichTre || 0);
      this.testTreSup = res.data.map(item => item.testTre || 0);
      this.CaseDaLamTrongNgaySup = res.data.map(item => item.caseLamTrongNgay || 0);
    } else {
      console.error('Invalid response format or empty data for sup data');
    }
  }

  private processAqData(res: any) {
    if (res && res.code === 200 && res.data) {
      this.AqReport_TongCase = res.data.map(item => item.tongCase || 0);
      this.AqReport_treHan = res.data.map(item => item.treHan || 0);
      this.AqReport_conHan = res.data.map(item => item.conHan || 0);
      this.AqReport_nhanSu = res.data.map(item => item.nhanSu || 0);
      this.AqReport_name = res.data.map(item => item.name || 0);
    } else {
      console.error('Invalid response format or empty data for AQ data');
    }
  }

  private processPhanBoData(res: any) {
    if (res && res.code === 200 && res.data) {
      this.BieuDoPhanBoSoCaseTheoThoiGianCho_data = res.data.map(item => item.soCase || 0);
      this.BieuDoPhanBoSoCaseTheoThoiGianCho_ten = res.data.map(item => item.tuanSo || 0);
      this.BieuDoPhanBoSoCaseTheoThoiGianCho_count = res.count;
    } else {
      console.error('Invalid response format or empty data for PhanBo data');
    }
  }

  updateAllCharts() {
    Object.values(this.charts).forEach(chart => chart.updateFunction());
    var sort_array = [];
    for (var key in this.charts) {
      sort_array.push({ key: key, net_total: this.charts[key].id });
    }
    // Now sort it:
    sort_array.sort(function (x, y) { return x.net_total - y.net_total });
    // Now process that object with it:

    // Object.values(this.charts).forEach(chart => chart.updateFunction());
  }

  sortCharts(charts: { [key: string]: ChartData, }) {
    const chartsArray = Object.values(charts);

    var test = chartsArray.sort((a, b) => a.id - b.id);
    console.log(test);

    charts = chartsArray.reduce((acc, chart) => {
      const key = Object.keys(charts).find(key => charts[key].id === chart.id);
      if (key) {
        acc[key] = chart;
      }
      return acc;
    }, {} as { [key: string]: ChartData });
  }


  updateBieuDoTienDoXyLyCasesDev() {
    if (!this.CanXuLyListDev || !this.soCaseTrongNgayListDev || !this.xuLyTreListDev || !this.assignedToListDev) {
      console.error('One or more data arrays for Chart 1 are undefined');
      return;
    }
    const specificColorIndex = 1;
    this.charts.BieuDoTienDoXyLyCasesDev.chartOptions.series = [
      { name: "Cần xử lý", data: this.CanXuLyListDev },
      { name: "Số case trong ngày", data: this.soCaseTrongNgayListDev },
      { name: "Xử lý trễ", data: this.xuLyTreListDev }
    ];
    this.charts.BieuDoTienDoXyLyCasesDev.chartOptions.xaxis = { categories: this.assignedToListDev };
  }

  updateBieuDoPhanBoThoiGianDev() {
    if (!this.tgCanXyLyListDev || !this.luongGioTrongNgayListDev || !this.assignedToListDev) {
      console.error('One or more data arrays for Chart 2 are undefined');
      return;
    }
    this.charts.BieuDoPhanBoThoiGianDev.chartOptions.series = [
      { name: "Số giờ cần xử lý tất cả case", data: this.tgCanXyLyListDev },
      { name: "Lượng giờ trong ngày", data: this.luongGioTrongNgayListDev }
    ];
    this.charts.BieuDoPhanBoThoiGianDev.chartOptions.xaxis = { categories: this.assignedToListDev };
  }
  updateBieuDoTienDoCongViecSup() {
    if (!this.canXuLySup || !this.xuLyTreSup || !this.phanTichTreSup || !this.testTreSup) {
      console.error('One or more data arrays for Chart 2 are undefined');
      return;
    }
    this.charts.BieuDoTienDoXyLyCasesSup.chartOptions.series = [
      { name: "Số case đã xử lý", data: this.CaseDaLamTrongNgaySup },
      { name: "Cần xử lý", data: this.canXuLySup },
      { name: "Xử lý trễ", data: this.xuLyTreSup },
      { name: "Phân tích trễ", data: this.phanTichTreSup },
      { name: "Test trễ", data: this.testTreSup },
    ];
    this.charts.BieuDoTienDoXyLyCasesSup.chartOptions.xaxis = { categories: this.assignedToListSup };
    this.charts.BieuDoTienDoXyLyCasesSup.chartOptions.fill.colors = this.generalColor3;
    this.charts.BieuDoTienDoXyLyCasesSup.chartOptions.dataLabels.style.colors = this.generalColor3;
    this.charts.BieuDoTienDoXyLyCasesSup.chartOptions.legend.markers.fillColors = this.generalColor3;
  }
  updateBieuDoTronCuaSup() {
    if (!this.canXuLySup || !this.assignedToListSup) {
      console.error('canXuLySup or assignedToListSup is undefined');
      return;
    }
    const total = this.canXuLySup.reduce((sum, value) => sum + value, 0);
    const seriesData = this.canXuLySup.map(value => Number(((value / total) * 100).toFixed(1)));
    this.charts.BieuDoPhanBoTienDoXuLy.chartOptions.series = this.canXuLySup;
    this.charts.BieuDoPhanBoTienDoXuLy.chartOptions.labels = this.assignedToListSup;
  };
  // Order by ascending property value
  originalOrder = (a: KeyValue<string, ChartData>, b: KeyValue<string, ChartData>): number => {
    return 0;
  };

  updateBieuDoPhanBoSoCaseTheoThoiGianCho() {
    this.charts.BieuDoPhanBoSoCaseTheoThoiGianCho.chartOptions.series = this.BieuDoPhanBoSoCaseTheoThoiGianCho_data;
    this.charts.BieuDoPhanBoSoCaseTheoThoiGianCho.chartOptions.labels = this.BieuDoPhanBoSoCaseTheoThoiGianCho_ten;

  }
  updateBieuDoPhanBoSoCaseTheoThoiGianCho2() {
    if (!this.BieuDoPhanBoSoCaseTheoThoiGianCho_ten || !this.BieuDoPhanBoSoCaseTheoThoiGianCho_data) {
      console.error('One or more data arrays for Chart 2 are undefined');
      return;
    }
    this.charts.BieuDoPhanBoSoCaseTheoThoiGianCho2.chartOptions.series = [
      { name: "số case", data: this.BieuDoPhanBoSoCaseTheoThoiGianCho_data },
    ];
    this.charts.BieuDoPhanBoSoCaseTheoThoiGianCho2.chartOptions.xaxis = { categories: this.BieuDoPhanBoSoCaseTheoThoiGianCho_ten };
  }
  updateBieuDoTongHopAQReport() {

    this.charts.BieuDoTongHopAQReport.chartOptions.series = [
      { name: "Tổng case", data: this.AqReport_TongCase },
      { name: "Còn hạn", data: this.AqReport_conHan },
      { name: "Trễ hạn", data: this.AqReport_treHan },
      { name: "Nhân sự", data: this.AqReport_nhanSu }
    ];
    this.charts.BieuDoTongHopAQReport.chartOptions.xaxis = { categories: this.AqReport_name };
  }

  updateBieuDoDEVReport() {
    this.charts.BieuDoDEVReport.chartOptions.series = [
      { name: "Cần xử lý", data: this.CanXuLyListDev },
      { name: "Số case trong ngày", data: this.soCaseTrongNgayListDev },
      { name: "Xử lý trễ", data: this.xuLyTreListDev }
    ];
    this.charts.BieuDoDEVReport.chartOptions.xaxis = { categories: this.assignedToListDev };
    this.charts.BieuDoDEVReport.chartOptions.chart.height = 300;
  }

  updateBieuDoSUPReport() {
    this.charts.BieuDoSUPReport.chartOptions.series = [
      { name: "Số case đã xử lý", data: this.CaseDaLamTrongNgaySup },
      { name: "Cần xử lý", data: this.canXuLySup },
      { name: "Xử lý Trễ", data: this.xuLyTreSup }
    ];
    this.charts.BieuDoSUPReport.chartOptions.xaxis = { categories: this.assignedToListSup };
    this.charts.BieuDoSUPReport.chartOptions.chart.height = 300;
  }
}
