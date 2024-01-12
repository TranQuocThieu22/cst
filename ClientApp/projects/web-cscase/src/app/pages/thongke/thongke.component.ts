import { DatePipe } from '@angular/common';
import { Component, ViewChild } from '@angular/core';
import { DataTruong, User } from '@mylibs';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';
import { DatetimeBsConfigService } from '../../service/datetime-bs-config.service';
import {
  ApexNonAxisChartSeries, ApexPlotOptions, ApexChart, ApexFill, ChartComponent, ApexAxisChartSeries, ApexDataLabels,
  ApexTitleSubtitle, ApexStroke, ApexGrid, ApexYAxis, ApexXAxis, ApexLegend, ApexResponsive,
} from "ng-apexcharts";

import * as moment from 'moment';
import { BsModalService } from 'ngx-bootstrap/modal';

type ApexXAxis_cot = {
  type?: "category" | "datetime" | "numeric";
  categories?: any;
  labels?: {
    style?: {
      colors?: string | string[];
      fontSize?: string;
    };
  };
};

@Component({
  selector: 'app-thongke',
  templateUrl: './thongke.component.html',
  styleUrls: ['./thongke.component.scss']
})

export class ThongkeComponent {

  private data: any; public dataCase_BD1: any; public dataCase_gr: any;
  public dataCase_bar: any; public dataCase_line: any; private listmatruong: any;
  public currentUser: User; public isadmin: boolean = false; public cbMaTruong: DataTruong[];
  public selectmatruong = []; public disabled = false;
  public bsConfig: Partial<BsDatepickerConfig> = {};
  private tu_ngay_default: Date; private den_ngay_default: Date;

  private colors = ["#008FFB", "#00E396", "#FEB019", "#FF4560", "#775DD0", "#546E7A", "#26a69a", "#D10CE8", "#008000", "#F3B415", "#F27036", "#663F59",
    "#6A6E94", "#4E88B4", "#A9D794", "#00A7C6", "#46AF78", "#33A1FD", "#18D8D8", "#8C5E58", "#2176FF", "#A93F55", "#7A918D", "#BAFF29"];

  @ViewChild("chart1") chart1: ChartComponent; public chartOptions: Partial<ChartOptions>; public TongSoLuongCase1: number;
  public chartOptions1: Partial<ChartOptions> = { series: [] }; public chartOptions2: Partial<ChartOptions> = { series: [] };
  public chartOptions3: Partial<ChartOptions> = { series: [] }; public chartOptions4: Partial<ChartOptions> = { series: [] };
  public tu_ngay_bd1: Date; public den_ngay_bd1: Date;

  @ViewChild("chart_bar") chart_bar: ChartComponent; public chartOptions_bar: Partial<ChartOptions_bar>;
  public TongSoLuongCase_bar: number; public SelectAnDongCase: boolean = false; private datePipe: DatePipe;
  public tu_ngay_bar: Date; public den_ngay_bar: Date;

  @ViewChild("chart_line") chart_line: ChartComponent; public chartOptions_line: Partial<ChartOptions_line>;
  public chartOptions_line_ngay: Partial<ChartOptions_line> = { series: [] };
  private data_search; public SoCaseByThoiGian: any; public lstNam: any[]; public select_Nam: string; public title: string = '';
  public list_thang = [{ thang: '01' }, { thang: '02' }, { thang: '03' }, { thang: '04' }, { thang: '05' }, { thang: '06' },
  { thang: '07' }, { thang: '08' }, { thang: '09' }, { thang: '10' }, { thang: '11' }, { thang: '12' }];
  public TongSoLuongCase_line: number; public select_thang: string = '';

  @ViewChild("chart_cot") chart_cot: ChartComponent; public chartOptions_cot: Partial<ChartOptions_cot>;
  public tu_ngay_cot: Date; public den_ngay_cot: Date; public TongSoLuongCase_cot: number;

  @ViewChild("chart_tron_lhd") chart_tron_lhd: ChartComponent; public chartOptions_tron: Partial<ChartOptions_tron> = { series: [] };;
  @ViewChild("chart_tron_mucdo") chart_tron_mucdo: ChartComponent; public chartOptions_tron_mucdo: Partial<ChartOptions_tron> = { series: [] };;
  public chartOptions_tron_lhd: Partial<ChartOptions_tron> = { series: [] };

  public tu_ngay_tron_lhd: Date; public den_ngay_tron_lhd: Date; public TongSoLuongCase_tron_lhd: number;
  public tu_ngay_tron_mucdo: Date; public den_ngay_tron_mucdo: Date; public TongSoLuongCase_tron_mucdo: number;

  public View1_case: boolean = false; public View2_bar: boolean = false; public View3_line: boolean = false;
  public View4_cot: boolean = false; public View5_tron_lhd: boolean = false; public View6_tron_mucdo: boolean = false;

  constructor(private dtimeServices: DatetimeBsConfigService, private modalService: BsModalService,) {
    this.bsConfig = this.dtimeServices.bsConfig;

    if (sessionStorage.getItem('current-user')) {
      this.currentUser = JSON.parse(sessionStorage.getItem('current-user'));
      if (this.currentUser.roles.toLowerCase() === 'admin') {
        this.isadmin = true;
      }
    }

    this.tu_ngay_default = new Date('2022-01-01');
    this.den_ngay_default = new Date(new Date().getFullYear() + "-" + (new Date().getMonth() + 1) + "-" + new Date().getDate());

    this.tu_ngay_bd1 = this.tu_ngay_bar = this.tu_ngay_cot = this.tu_ngay_tron_lhd = this.tu_ngay_tron_mucdo = this.tu_ngay_default;
    this.den_ngay_bd1 = this.den_ngay_bar = this.den_ngay_cot = this.den_ngay_tron_lhd = this.den_ngay_tron_mucdo = this.den_ngay_default;
  }

  public ngOnInit(): void {
    this.DeclareBieuDo();

    window.scroll({ top: 190, behavior: 'smooth' });

    if (sessionStorage.getItem('db')) {
      let datagoc = JSON.parse(sessionStorage.getItem('db'));

      if (datagoc.data_truong && datagoc.data_truong.length > 0) {
        this.cbMaTruong = datagoc.data_truong;
        if (this.cbMaTruong.length === 1) {
          this.selectmatruong = [this.currentUser.maTruong];
          this.disabled = true;
        }
        else {
          this.selectmatruong = JSON.parse(sessionStorage.getItem('matruong'));
        }
      }

      this.data = datagoc.data_case;

      this.data.forEach(s => {
        s.nam = s.ngaynhan ? new DatePipe('en-US').transform(s.ngaynhan, 'yyyy') : '';
        s.thang = s.ngaynhan ? new DatePipe('en-US').transform(s.ngaynhan, 'MM') : '';
        s.lblthang = s.ngaynhan ? new DatePipe('en-US').transform(s.ngaynhan, 'MM/yyyy') : '';
        s.lblngayofThg = s.ngaynhan ? 'Ngày ' + new DatePipe('en-US').transform(s.ngaynhan, 'dd') : '';
        s.ngaynhan_ = s.ngaynhan ? new DatePipe('en-US').transform(s.ngaynhan, 'dd/MM/yyyy') : '';
      });

      // load toàn bộ Bieu do
      this.XuLyLoadDataBieuDo(this.data, moment(this.tu_ngay_default).format('DD/MM/YYYY'), moment(this.den_ngay_default).format('DD/MM/YYYY'), 0);

      if (sessionStorage.getItem('matruong')) { // get ma truong từ page case nếu có chọn (admin)
        let lsttruong = JSON.parse(sessionStorage.getItem('matruong'));
        let data_by_truong = this.data.filter(s => lsttruong.some(x => x === s?.matruong));
        // load toàn bộ Bieu do by truong
        this.XuLyLoadDataBieuDo(data_by_truong, moment(this.tu_ngay_default).format('DD/MM/YYYY'), moment(this.den_ngay_default).format('DD/MM/YYYY'), 0);
      }

    }
  }

  public changeTruong(event: any) {
    let t1 = this.tu_ngay_bd1 = this.tu_ngay_bar = this.tu_ngay_cot = this.tu_ngay_tron_lhd = this.tu_ngay_tron_mucdo = new Date('2022-01-01');
    let t2 = this.den_ngay_bd1 = this.den_ngay_bar = this.den_ngay_cot = this.den_ngay_tron_lhd = this.den_ngay_tron_mucdo = new Date(new Date().getFullYear() + "-" + (new Date().getMonth() + 1) + "-" + new Date().getDate());

    if (!event) {
      this.listmatruong = ''; this.selectmatruong = null;
      this.XuLyLoadDataBieuDo('', moment(t1).format('DD/MM/YYYY'), moment(t2).format('DD/MM/YYYY'), 0);
      return;
    }
    else {
      let listmatruong = event;
      const databytruong = this.data.filter(s => s.matruong
        && ((listmatruong && listmatruong.length > 0) ? listmatruong.some(x => x.matruong === s.matruong) : true)
      );
      this.XuLyLoadDataBieuDo(databytruong, moment(t1).format('DD/MM/YYYY'), moment(t2).format('DD/MM/YYYY'), 0);
    }
  }

  private XuLyLoadDataBieuDo(data_, startDate: string, endDate: string, loadBD: number = 0) {
    if (!data_ || data_.length <= 0) { return; }

    let databytruong = [];
    if (this.selectmatruong && this.selectmatruong.length > 0) {
      databytruong = data_.filter(s => this.selectmatruong.find(x => x === s?.matruong));
    }
    else {
      databytruong = data_;
    }

    let data_sort = databytruong.sort((a, b) => new Date(a.ngaynhan) > new Date(b.ngaynhan) ? 1 : -1);

    ///// BIEU DO CUM 1  /////////////////////////////////
    if (loadBD === 1 || loadBD === 0) {
      let tmp_ = data_sort.filter(a => {
        var date = new Date(a.ngaynhan);
        return (date >= this.getformatdate(startDate) && date <= this.getformatdate(endDate, 'end'));
      });

      ///// BIEU DO CUM 1 /////////////////////////////////
      let data_sort_loaicase = tmp_.sort((a, b) => a.loaicase > b.loaicase ? 1 : -1);
      let tpm = data_sort_loaicase.filter(s => s.loaicase.startsWith('BF') || s.loaicase.startsWith('EX')
        || s.loaicase.startsWith('NF') || s.loaicase.startsWith('CV'));

      // console.log(tpm);

      // let tpm_ex = tmp_.filter(s => !s.loaicase.startsWith('BF') && !s.loaicase.startsWith('EX')
      //   && !s.loaicase.startsWith('NF') && !s.loaicase.startsWith('CV'));
      // console.log(tpm_ex);

      this.TongSoLuongCase1 = tpm.length || 0;

      this.groupByKeys(tpm);

      let tmp_BF = this.dataCase_gr.filter(s => s.loaicase.startsWith('BF'));
      if (!tmp_BF || tmp_BF.length <= 0) {
        this.dataCase_gr.push({ loaicase: "BF - Lỗi cần sửa code", phantram: 0, soluong: 0, type: 1, })
      }

      let tmp_EX = this.dataCase_gr.filter(s => s.loaicase.startsWith('EX'));
      if (!tmp_EX || tmp_EX.length <= 0) {
        this.dataCase_gr.push({ loaicase: "EX - Lỗi Exception cần sửa code", phantram: 0, soluong: 0, type: 2, })
      }

      let tmp_NF = this.dataCase_gr.filter(s => s.loaicase.startsWith('NF'));
      if (!tmp_NF || tmp_NF.length <= 0) {
        this.dataCase_gr.push({ loaicase: "NF - Yêu cầu mới cần sửa code", phantram: 0, soluong: 0, type: 3, })
      }

      let tmp_CV = this.dataCase_gr.filter(s => s.loaicase.startsWith('CV'));
      if (!tmp_CV || tmp_CV.length <= 0) {
        this.dataCase_gr.push({ loaicase: "CV - Trao đổi hoặc chưa phân loại", phantram: 0, soluong: 0, type: 4, })
      }

      this.dataCase_gr.map(s => {
        if (s.loaicase.startsWith('BF')) {
          s.type = 1; s.phantram = s.soluong * 100 / this.TongSoLuongCase1;
          this.LoadBieuDoCum1(s.phantram || 0, s.type);
        }
        if (s.loaicase.startsWith('EX')) {
          s.type = 2; s.phantram = s.soluong * 100 / this.TongSoLuongCase1;
          this.LoadBieuDoCum1(s.phantram || 0, s.type);
        }
        if (s.loaicase.startsWith('NF')) {
          s.type = 3; s.phantram = s.soluong * 100 / this.TongSoLuongCase1;
          this.LoadBieuDoCum1(s.phantram || 0, s.type);
        }
        if (s.loaicase.startsWith('CV')) {
          s.type = 4; s.phantram = s.soluong * 100 / this.TongSoLuongCase1;
          this.LoadBieuDoCum1(s.phantram || 0, s.type);
        }
      });

      this.dataCase_gr = this.dataCase_gr.sort((a, b) => a.type < b.type ? -1 : a.type > b.type ? 1 : 0);
      this.View1_case = true;
    }

    ///// BIEU DO CUM 2 BAR  /////////////////////////////////
    if (loadBD === 2 || loadBD === 0) {
      let data2 = data_sort.filter(a => {
        var date = new Date(a.ngaynhan);
        return (date >= this.getformatdate(startDate) && date <= this.getformatdate(endDate, 'end'));
      });
      this.TongSoLuongCase_bar = data2.length || 0;
      this.LoadBieuDoCum_bar(data2);
    }

    ///// BIEU DO CUM 3 LINE  /////////////////////////////////
    if (loadBD === 3 || loadBD === 0) {
      let tpm_nam = this.groupArry(data_sort);
      this.lstNam = tpm_nam;
      this.lstNam = this.lstNam.sort((a, b) => a.nam < b.nam ? -1 : a.nam > b.nam ? 1 : 0);
      this.select_Nam = this.lstNam.slice(-1)[0]?.nam;
      this.LoadBieuDoCum_line(data_sort, 2);
    }

    ///// BIEU DO CUM 4 BAR /////////////////////////////////
    if (loadBD === 4 || loadBD === 0) {

      let data4 = data_sort.filter(a => {
        var date = new Date(a.ngaynhan);
        return (date >= this.getformatdate(startDate) && date <= this.getformatdate(endDate, 'end'));
      });

      this.loadBieDo_Cot(data4);
    }

    ///// BIEU DO CUM 5 TRON LHD /////////////////////////////////
    if (loadBD === 5 || loadBD === 0) {
      let data5 = data_sort.filter(a => {
        var date = new Date(a.ngaynhan);
        return (date >= this.getformatdate(startDate) && date <= this.getformatdate(endDate, 'end'));
      });
      this.loadBieuDo_Tron(data5, 1);
    }

    if (loadBD === 6 || loadBD === 0) {
      let data6 = data_sort.filter(a => {
        var date = new Date(a.ngaynhan);
        return (date >= this.getformatdate(startDate) && date <= this.getformatdate(endDate, 'end'));
      });
      this.loadBieuDo_Tron(data6, 2);
    }
  }

  //cum 1 case
  private LoadBieuDoCum1(sl: number, type) {
    let sl_ = this.MathRoungDiem(sl)

    // BF - Lỗi cần sửa code
    if (type === 1) { this.chartOptions1.series = [sl_]; this.chartOptions1.colors = ["#FEB019"]; }
    // EX - Lỗi Exception cần sửa code
    if (type === 2) { this.chartOptions2.series = [sl_]; this.chartOptions2.colors = ["#FF4560"]; }
    // NF - Yêu cầu mới cần sửa code
    if (type === 3) { this.chartOptions3.series = [sl_]; this.chartOptions3.colors = ["#008FFB"]; }
    // CV - Trao đổi hoặc chưa phân loại
    if (type === 4) { this.chartOptions4.series = [sl_]; this.chartOptions4.colors = ["#26a69a"]; }
  };

  //cum 2 bar
  private LoadBieuDoCum_bar(datacase) {
    this.dataCase_bar = datacase;
    let bar2 = datacase.sort((a, b) =>
      a.trangthai < b.trangthai ? -1 : a.trangthai > b.trangthai ? 1 : 0)
      .reduce((acc, it) => {
        acc[it.trangthai] = acc[it.trangthai] + 1 || 1;
        return acc;
      }, []);

    let loaicase = Object.keys(bar2) as [];
    // let soluong = Object.values(bar2) as [];

    let data_series: any[] = [];

    let case_; let BF; let EX; let NF; let CV;
    loaicase.forEach((s, i) => {
      case_ = datacase.filter(x => x.trangthai === s);
      data_series.push({ name: s });
      BF = case_.filter(v => v.loaicase.startsWith('BF')).length || 0;
      EX = case_.filter(v => v.loaicase.startsWith('EX')).length || 0;
      NF = case_.filter(v => v.loaicase.startsWith('NF')).length || 0;
      CV = case_.filter(v => v.loaicase.startsWith('CV')).length || 0;
      // data_series.push({ name: s, data: [{ 'BF': BF }, { 'EX': EX }, { 'NF': NF }, { 'CV': CV }] });
      data_series[i].data = [{ 'BF': BF }, { 'EX': EX }, { 'NF': NF }, { 'CV': CV }];
    });

    let MC_BF = data_series.filter(m => m.name === 'Mở Case')[0]?.data[0]?.BF || 0;
    let MC_EX = data_series.filter(m => m.name === 'Mở Case')[0]?.data[1]?.EX || 0;
    let MC_NF = data_series.filter(m => m.name === 'Mở Case')[0]?.data[2]?.NF || 0;
    let MC_CV = data_series.filter(m => m.name === 'Mở Case')[0]?.data[3]?.CV || 0;

    let PT_BF = data_series.filter(m => m.name === 'Đang phân tích')[0]?.data[0]?.BF || 0;
    let PT_EX = data_series.filter(m => m.name === 'Đang phân tích')[0]?.data[1]?.EX || 0;
    let PT_NF = data_series.filter(m => m.name === 'Đang phân tích')[0]?.data[2]?.NF || 0;
    let PT_CV = data_series.filter(m => m.name === 'Đang phân tích')[0]?.data[3]?.CV || 0;

    let XL_BF = data_series.filter(m => m.name === 'Đang xử lý')[0]?.data[0]?.BF || 0;
    let XL_EX = data_series.filter(m => m.name === 'Đang xử lý')[0]?.data[1]?.EX || 0;
    let XL_NF = data_series.filter(m => m.name === 'Đang xử lý')[0]?.data[2]?.NF || 0;
    let XL_CV = data_series.filter(m => m.name === 'Đang xử lý')[0]?.data[3]?.CV || 0;

    let DC_BF = data_series.filter(m => m.name === 'Đóng case')[0]?.data[0]?.BF || 0;
    let DC_EX = data_series.filter(m => m.name === 'Đóng case')[0]?.data[1]?.EX || 0;
    let DC_NF = data_series.filter(m => m.name === 'Đóng case')[0]?.data[2]?.NF || 0;
    let DC_CV = data_series.filter(m => m.name === 'Đóng case')[0]?.data[3]?.CV || 0;

    // Mở case #008FFB   // Đang phân tích #008000   // Đang xử lý #6f0080   // Đang test (đã xử lý) #b10754 // Đóng case #546E7A

    // BF - Lỗi cần sửa code   #FEB019            // EX - Lỗi Exception cần sửa code    #FF4560
    // NF - Yêu cầu mới cần sửa code   #008FFB   // CV - Trao đổi hoặc chưa phân loại  #26a69a


    this.chartOptions_bar = {
      // series: [{ name: "SL case", data: soluong }],
      series: [
        { name: "BF", data: [MC_BF, PT_BF, XL_BF, DC_BF] },
        { name: "EX", data: [MC_EX, PT_EX, XL_EX, DC_EX] },
        { name: "NF", data: [MC_NF, PT_NF, XL_NF, DC_NF] },
        { name: "CV", data: [MC_CV, PT_CV, XL_CV, DC_CV] },
      ],

      // series: data_series,
      chart: { type: "bar", height: 450, toolbar: { show: false } },

      plotOptions: { bar: { distributed: false, horizontal: true, barHeight: '85%', dataLabels: { position: 'top' }, } },
      legend: {
        show: true,
        fontSize: '15px', horizontalAlign: 'center',
        onItemClick: {
          toggleDataSeries: true
        },
        onItemHover: {
          highlightDataSeries: true
        },
      },
      colors: ['#FEB019', '#FF4560', '#008FFB', '#26a69a'],
      dataLabels: { enabled: true, textAnchor: 'start', offsetX: 20, style: { fontSize: '20px', colors: ["#888"] } },
      stroke: { show: true, width: 1, colors: ["#fff"] },
      xaxis: { categories: loaicase }
    };

    this.View2_bar = true;
  }

  // cum 3 line
  public LoadBieuDoCum_line(datacase, type) {
    this.dataCase_line = datacase;

    if (type === 1) { // Nam
      let tpm = [];
      if ((this.select_Nam && this.select_Nam.length > 0)) {
        tpm = this.dataCase_line.filter(s => s.ngaynhan && ((this.select_Nam && this.select_Nam.length > 0) ? this.select_Nam === s.nam : true))
      }
      else {
        tpm = this.dataCase_line;
      }

      this.TongSoLuongCase_line = tpm.length || 0;
      this.data_search = tpm.sort((a, b) => new Date(a.thang) > new Date(b.thang) ? 1 : -1);

      this.SoCaseByThoiGian = this.data_search.reduce((acc, it) => {
        acc[it.nam] = acc[it.nam] + 1 || 1;
        return acc;
      }, []);

      let sl_ = Object.values(this.SoCaseByThoiGian) as [];
      let ngay_ = Object.keys(this.SoCaseByThoiGian) as [];

      this.title = "Số lượng case trong năm " + (this.select_Nam ? this.select_Nam : this.lstNam.map(s => s.nam));
      this.chartOptions_line_ngay.series = [{ name: "Số lượng", data: sl_ }];
      this.chartOptions_line_ngay.xaxis = { categories: ngay_, labels: { rotate: -70 } };
      this.View3_line = true;
    }
    if (type === 2) { // tháng
      // get 1 năm
      let tpm = this.dataCase_line.filter(s => s.ngaynhan
        && ((this.select_Nam && this.select_Nam.length > 0) ? this.select_Nam === s.nam : true)
      );

      this.TongSoLuongCase_line = tpm || 0;
      this.data_search = tpm.sort((a, b) => new Date(a.thang) > new Date(b.thang) ? 1 : -1);

      this.SoCaseByThoiGian = this.data_search.reduce((acc, it) => {
        acc[it.lblthang] = acc[it.lblthang] + 1 || 1;
        return acc;
      }, []);

      let sl_ = Object.values(this.SoCaseByThoiGian) as [];
      let ngay_ = Object.keys(this.SoCaseByThoiGian) as [];
      this.title = "Số lượng case các tháng trong năm " + this.select_Nam;
      this.chartOptions_line_ngay.series = [{ name: "SL case", data: sl_ }];
      this.chartOptions_line_ngay.xaxis = { categories: ngay_, labels: { rotate: -70 } };
      this.View3_line = true;
    }
    if (type === 3) { // ngày trong thang
      // get 1 năm
      if (!this.select_Nam) {
        this.select_Nam = this.lstNam.slice(-1)[0]?.nam;
      }

      let tpm = this.dataCase_line.filter(s => s.ngaynhan && this.select_Nam === s.nam && this.select_thang === s.thang);
      this.TongSoLuongCase_line = tpm.length || 0;

      this.data_search = tpm.sort((a, b) => new Date(a.ngaynhan) > new Date(b.ngaynhan) ? 1 : -1);

      this.SoCaseByThoiGian = this.data_search.reduce((acc, it) => {
        acc[it.ngaynhan_] = acc[it.ngaynhan_] + 1 || 1;
        return acc;
      }, []);

      let sl_ = Object.values(this.SoCaseByThoiGian) as [];
      let ngay_ = Object.keys(this.SoCaseByThoiGian) as [];

      this.title = "Số lượng case của tháng " + this.select_thang + '/' + this.select_Nam;
      this.chartOptions_line_ngay.series = [{ name: "SL case", data: sl_ }];
      this.chartOptions_line_ngay.xaxis = { categories: ngay_, labels: { rotate: -70 } };
      this.View3_line = true;
    }
  }

  //cum 4 cot
  public loadBieDo_Cot(datacase) {

    this.TongSoLuongCase_cot = datacase.length || 0;

    let cot = datacase.sort((a, b) =>
      a.phanhe < b.phanhe ? -1 : a.phanhe > b.phanhe ? 1 : 0)
      .reduce((acc, it) => {
        acc[it.phanhe] = acc[it.phanhe] + 1 || 1;
        return acc;
      }, []);


    let phanhe = Object.keys(cot) as [];
    let soluong = Object.values(cot) as [];

    this.chartOptions_cot = {
      series: [{ name: "SL case", data: soluong }],
      chart: { height: 470, type: "bar", toolbar: { show: false }, },
      colors: this.colors,
      plotOptions: { bar: { columnWidth: "45%", distributed: true, dataLabels: { position: 'top' }, } },
      dataLabels: { enabled: true, offsetY: -23, style: { fontSize: '3px', colors: ["#888"] } },
      legend: {
        show: false,
        showForSingleSeries: false,
        onItemHover: { highlightDataSeries: true },
        onItemClick: { toggleDataSeries: true },
      },
      grid: { show: true },
      xaxis: {
        categories: phanhe,
        labels: { style: { colors: this.colors, fontSize: "10px" } }
      }
    };
    this.View4_cot = true;
  }

  // cum 5 tron // loai hop dong, muc do
  public loadBieuDo_Tron(datacase, type) {
    if (type === 1) { // loai hop dong
      this.TongSoLuongCase_tron_lhd = datacase.length || 0;
      let tron_lhd = datacase.sort((a, b) =>
        a.loaihopdong < b.loaihopdong ? -1 : a.loaihopdong > b.loaihopdong ? 1 : 0)
        .reduce((acc, it) => {
          acc[it.loaihopdong] = acc[it.loaihopdong] + 1 || 1;
          return acc;
        }, []);

      let lables = Object.keys(tron_lhd) as [];
      let soluong = Object.values(tron_lhd) as [];

      this.chartOptions_tron_lhd.series = soluong;
      this.chartOptions_tron_lhd.labels = lables;
      this.chartOptions_tron_lhd.colors = ["#775DD0", "#26a69a", "#D10CE8", "#663F59", "#4E88B4"];
      //  01 - HĐ triển khai      #775DD0
      //  02 - HĐ bảo hành        #26a69a
      //  03 - HĐ bảo trì         #D10CE8
      //  04 - HĐ bổ sung add-in  #663F59
      //  09 - HĐ hết hạn         #4E88B4
      this.View5_tron_lhd = true;
    }
    if (type === 2) { // muc do

      this.TongSoLuongCase_tron_mucdo = datacase.length || 0;

      let tron_mucdo = datacase.sort((a, b) =>
        a.mucdo < b.mucdo ? -1 : a.mucdo > b.mucdo ? 1 : 0)
        .reduce((acc, it) => {
          acc[it.mucdo] = acc[it.mucdo] + 1 || 1;
          return acc;
        }, []);

      let lables = Object.keys(tron_mucdo) as [];
      let lables_ = lables.map(item => item === 'null' ? ' ' : item)
      let soluong = Object.values(tron_mucdo) as [];

      this.chartOptions_tron_mucdo.series = soluong;
      this.chartOptions_tron_mucdo.labels = lables_;

      this.chartOptions_tron_mucdo.colors = ["#FF4560", "#F27036", "#A93F55", "#2176FF", "#8C5E58"];
      // "1 - Độ ưu tiên rất cao"     "#FF4560"
      // "2 - Độ ưu tiên cao"         "#F27036"
      // "3 - Độ ưu tiên trung bình"  "#A93F55"
      // " "                          "#2176FF"
      //  "Thực hiện theo kế hoạch"   "#8C5E58"
      this.View6_tron_mucdo = true;
    }
  }

  public ChangeNam(event) {
    this.select_thang = '';
    if (event) {
      this.select_Nam = event.nam;
    }
    else {
      this.select_Nam = null;
    }
    this.LoadBieuDoCum_line(this.dataCase_line, 1);
  }

  public ViewByNam() {
    this.select_Nam = null;
    this.select_thang = '';
    this.LoadBieuDoCum_line(this.dataCase_line, 1); // nam
  }

  public ViewByThang() {
    this.select_thang = '';
    if (!this.select_Nam) {
      this.select_Nam = this.lstNam.slice(-1)[0]?.nam;
      if (!this.select_Nam) { this.select_Nam = new DatePipe('en-US').transform(new Date(), 'yyyy') }
    }
    this.LoadBieuDoCum_line(this.dataCase_line, 2); // thang
  }

  public ViewByNgay(thang) {
    this.select_thang = thang;
    if (this.select_thang) {
      this.LoadBieuDoCum_line(this.dataCase_line, 3); // ngay trong thang
    }
    else { this.select_thang = ''; }
  }

  public MathRoungDiem(Diem: any) {
    return Math.round(Diem * 10) / 10;
  }

  private groupByKeys(data) {
    const m = new Map();

    data.forEach(({ loaicase }) => {
      const hash = JSON.stringify([['loaicase', loaicase]]);
      m.set(hash, (m.get(hash) || 0) + 1);
    });

    const myOutputObject = [...m].map(([rec, soluong]) => ({
      ...Object.fromEntries(JSON.parse(rec)), soluong,
    }));

    this.dataCase_gr = myOutputObject;
    return myOutputObject;
  }

  private groupArry(arr: any) {
    let clazz: any;
    const group = {};
    arr.forEach((a, i) => {
      clazz = group[a.nam] || {
        nam: a.nam,
        // data: []
      };
      // clazz.data = [...clazz.data, a];
      group[a.nam] = clazz;
    });
    return Object.values(group);
  }

  public customSearchFn(term: string, item: DataTruong) {
    term = term.toLowerCase();
    return (
      item.matruong.toLowerCase().indexOf(term) > -1 ||
      item.tentruong.toLowerCase().indexOf(term) > -1 ||
      item.tentruong.toLowerCase() === term
    );
  }

  private DeclareBieuDo() {
    this.chartOptions = {
      series: [],
      colors: [],
      chart: { height: 250, type: "radialBar", toolbar: { show: false }, },
      plotOptions: {
        radialBar: {
          startAngle: -135, endAngle: 225, offsetY: -20, hollow: {
            margin: 0, size: "45%", background: "#fff", image: undefined,
            position: "front", dropShadow: { enabled: true, top: 3, left: 0, blur: 4, opacity: 0.24 }
          },
          track: { background: "#fff", strokeWidth: "80%", margin: 3, dropShadow: { enabled: true, top: -3, left: 5, blur: 6, opacity: 0.3 } },
          dataLabels: {
            show: true, name: { offsetY: -5, show: true, color: "#888", fontSize: "17px" },
            value: {
              offsetY: 6,
              formatter: function (val) { return val.toString() + '%' }, color: "#111", fontSize: "40px", show: true
            }
          }
        }
      },
      // fill: {
      //   type: "gradient",
      //   gradient: { shade: "dark", type: "horizontal", shadeIntensity: 0.5, gradientToColors: ["#ABE5A1"], inverseColors: true, opacityFrom: 1, opacityTo: 1, stops: [0, 100] }
      // },
      stroke: { lineCap: "round" },
      labels: ["Tỷ lệ"]
    };

    this.chartOptions_line = {
      series: [],
      chart: { height: 420, type: "line", zoom: { enabled: false }, toolbar: { show: false }, },
      dataLabels: { enabled: true }, stroke: { curve: "straight" }, title: {},
      grid: { row: { colors: ["#f3f3f3", "transparent"], opacity: 0.5 } }, xaxis: { categories: [], },
      yaxis: { labels: { formatter: function (val) { return val.toFixed(0); } } }
    };

    this.chartOptions_tron = {
      series: [],
      chart: {
        height: 230, type: "pie"
      },
      labels: [],
      responsive: [{ breakpoint: 480, options: { chart: { height: 230 }, legend: { position: "bottom" } } }],
    };
  }

  public onChangeDate(value: Date, loai, group): void {
    if (value && value?.toString().toLowerCase() === 'invalid date') {
      return;
    }
    else {
      if (value) {
        if (group === 1) { // cum 1
          if (loai === 1) { this.tu_ngay_bd1 = value; }
          else { this.den_ngay_bd1 = value; }
        }
        if (group === 2) { // cum 2 bar
          if (loai === 1) { this.tu_ngay_bar = value; }
          else { this.den_ngay_bar = value; }
        }
        if (group === 4) { // cum 4 cot
          if (loai === 1) { this.tu_ngay_cot = value; }
          else { this.den_ngay_cot = value; }
        }
        if (group === 5) { // cum 5 tron loai hop dong
          if (loai === 1) { this.tu_ngay_tron_lhd = value; }
          else { this.den_ngay_tron_lhd = value; }
        }
        if (group === 6) { // cum 6 tron muc do
          if (loai === 1) { this.tu_ngay_tron_mucdo = value; }
          else { this.den_ngay_tron_mucdo = value; }
        }
      }
    }
  }

  public ViewBD(loaibd: number) {

    let t1: string = ''; let t2: string = ''

    if (loaibd === 1) {   //  // load only bd 1
      t1 = new DatePipe('en-US').transform(this.tu_ngay_bd1, 'dd/MM/yyyy');
      t2 = new DatePipe('en-US').transform(this.den_ngay_bd1, 'dd/MM/yyyy');
    }
    if (loaibd === 2) {   //  load only bd bar
      t1 = new DatePipe('en-US').transform(this.tu_ngay_bar, 'dd/MM/yyyy');
      t2 = new DatePipe('en-US').transform(this.den_ngay_bar, 'dd/MM/yyyy');
    }
    if (loaibd === 4) {   //  load only bd cot
      t1 = new DatePipe('en-US').transform(this.tu_ngay_cot, 'dd/MM/yyyy');
      t2 = new DatePipe('en-US').transform(this.den_ngay_cot, 'dd/MM/yyyy');
    }

    if (loaibd === 5) {   //  load only bd tron loại hợp đồng
      t1 = new DatePipe('en-US').transform(this.tu_ngay_tron_lhd, 'dd/MM/yyyy');
      t2 = new DatePipe('en-US').transform(this.den_ngay_tron_lhd, 'dd/MM/yyyy');
    }

    if (loaibd === 6) {   //  load only bd tron muc do
      t1 = new DatePipe('en-US').transform(this.tu_ngay_tron_mucdo, 'dd/MM/yyyy');
      t2 = new DatePipe('en-US').transform(this.den_ngay_tron_mucdo, 'dd/MM/yyyy');
    }

    this.XuLyLoadDataBieuDo(this.data, t1, t2, loaibd);

  }

  private getformatdate(date_, end = '') {
    const dateStr = date_;
    const [day, month, year] = dateStr.split('/');
    const t_day: number = +day;
    const t_month: number = +month;
    const t_year: number = +year

    if (end) {
      return new Date(t_year, t_month - 1, t_day, 23, 59, 59);
    }
    else {
      return new Date(t_year, t_month - 1, t_day);
    }
  }

  public trackByFn(index: number) { return index; }
}


export type ChartOptions = {
  series: ApexNonAxisChartSeries; chart: ApexChart; labels: string[]; colors: string[]; plotOptions: ApexPlotOptions; fill: ApexFill; stroke: ApexStroke; grid: any
};

export type ChartOptions_bar = {
  series: ApexAxisChartSeries; chart: ApexChart; dataLabels: ApexDataLabels; plotOptions: ApexPlotOptions; stroke: ApexStroke; xaxis: ApexXAxis; legend: ApexLegend, colors: string[];
};

export type ChartOptions_line = {
  series: ApexAxisChartSeries; chart: ApexChart; xaxis: ApexXAxis; yaxis: ApexYAxis; dataLabels: ApexDataLabels; grid: ApexGrid; stroke: ApexStroke; title: ApexTitleSubtitle;
};

export type ChartOptions_cot = {
  series: ApexAxisChartSeries; chart: ApexChart; dataLabels: ApexDataLabels; plotOptions: ApexPlotOptions; yaxis: ApexYAxis; xaxis: ApexXAxis_cot;
  grid: ApexGrid; colors: string[]; legend: ApexLegend;
};

export type ChartOptions_tron = {
  series: ApexNonAxisChartSeries; colors: string[]; chart: ApexChart; responsive: ApexResponsive[]; labels: any,
};


