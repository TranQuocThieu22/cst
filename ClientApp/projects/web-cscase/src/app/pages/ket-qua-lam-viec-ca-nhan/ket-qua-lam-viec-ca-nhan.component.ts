import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { ConfirmationService, MessageService, PrimeNGConfig } from 'primeng/api';
import { NgxSpinnerService } from "ngx-spinner";
import { CaseMetrics } from 'projects/libs/src/lib/edu.models';
import { Table } from 'primeng/table';
import { LineChart } from "echarts/charts";
import { TitleComponent, TooltipComponent, LegendComponent, ToolboxComponent, GridComponent, VisualMapComponent } from "echarts/components";
import { Member } from './DataObject';
@Component({
  selector: 'app-ket-qua-lam-viec-ca-nhan',
  templateUrl: './ket-qua-lam-viec-ca-nhan.component.html',
  styleUrls: ['./ket-qua-lam-viec-ca-nhan.component.scss']
})
export class KetQuaLamViecCaNhanComponent implements OnInit {
  caseMetricsList: CaseMetrics[] = [];

  public SoGioLamViecTrongNgay: [];
  public SoGioLamThieu;
  public SoLuongCaseThucHienTrongTuan
  public SoLuotCaseBiMoLai
  public dateValue = new Date()
  public SoGioUocLuongCase
  public TiLeMoCaseChartOptions: object = {};
  public TiLeMoCaseChartExtensions: object = {};
  public SoGioLamThieuOptions: object = {};
  public SoGioLamThieuExtensions: object = {};
  public TiLeMoCaseChartPieces: object[] = [
    {
      gt: 0,
      lte: 20,
      color: 'green'
    },
    {
      gt: 20,
      lte: 80,
      color: 'red'
    }
  ]
  public SoGioLamThieuPieces: object[] = [
    {
      gt: 0,
      lte: 5,
      color: 'green'
    },
    {
      gt: 5,
      lte: 40,
      color: 'red'
    }
  ]
  public SoGioThucTeLamCase
  public SoGioThamGiaMeeting
  public PhanTramTiLeMoCase
  public PhanTramTiLeChenhLechUocLuongVaThucTe
  public products

  MemberList: Member[] = [];
  selectedMember: string = "tin <AQ\\tin>";

  constructor(
    private https: HttpClient,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private primengConfig: PrimeNGConfig,
    private spinner: NgxSpinnerService
  ) { }

  ngOnInit(): void {
    this.TiLeMoCaseChartExtensions = [LineChart, TitleComponent, TooltipComponent, LegendComponent, ToolboxComponent, GridComponent, VisualMapComponent]
    // this.SoGioLamThieuExtensions = [LineChart, TitleComponent, TooltipComponent, LegendComponent, ToolboxComponent, GridComponent, VisualMapComponent]
    this.products = {
      code: 1,
      name: "lam",
      category: "asd",
      quantity: 1
    }
    this.primengConfig.ripple = true;
    this.FetchKetQua(this.dateValue.getFullYear())
    this.fetchMemberListData();
  }


  FetchKetQua(data?: any) {
    this.spinner.show("spinner-ketqualamvieccanhan");

    const body = {
      // user: "tin <AQ\\tin>",
      user: this.selectedMember,
      year: this.dateValue.getFullYear()
    }
    this.https.post<any>("/api/KetQuaLamViecCaNhan/KetQuaLamViecCaNhan", body).subscribe({
      next: (res: any) => {
        if (res && res.code === 200) {
          if (res.data) {

            const dataLength = res.data.soLuongCaseThucHienTrongTuan.length;

            this.caseMetricsList = Array.from({ length: dataLength }, (_, index) => ({
              weekNumber: index + 1,
              SoGioLamThieu: res.data.soGioLamThieu[index] ?? 0,
              SoGioLamViecTrongNgay: res.data.soGioLamViecTrongNgay[index] ?? 0,
              // SoGioLamViecTrongNgay: 0,
              // SoGioLamThieu: 0,
              SoLuongCaseThucHienTrongTuan: res.data.soLuongCaseThucHienTrongTuan[index],
              SoLuotCaseBiMoLai: res.data.soLuotCaseBiMoLai[index],
              SoGioUocLuongCase: res.data.soGioUocLuongCase[index],
              SoGioThucTeLamCase: res.data.soGioThucTeLamCase[index],
              SoGioThamGiaMeeting: res.data.soGioThamGiaMeeting[index],
              PhanTramTiLeMoCase: res.data.phanTramTiLeMoCase[index],
              PhanTramTiLeChenhLechUocLuongVaThucTe: res.data.phanTramTiLeChenhLechUocLuongVaThucTe[index],
            }));
            this.PhanTramTiLeMoCase = res.data.phanTramTiLeMoCase
            const xAxisData = this.PhanTramTiLeMoCase.map((_, index) => `Week ${index + 1}`);
            console.log(xAxisData);
            this.SoGioLamThieu = res.data.soGioLamThieu
            console.log(this.SoGioLamThieu);


            this.LineChartTyLeMoCaseOptions(xAxisData, this.PhanTramTiLeMoCase, this.TiLeMoCaseChartPieces,)
            // this.LineChartTyLeMoCaseOptions(xAxisData, this.SoGioLamThieu, this.SoGioLamThieuPieces, this.SoGioLamThieuOptions)
            this.caseMetricsList.sort((a, b) => b.weekNumber - a.weekNumber);


            this.spinner.hide("spinner-ketqualamvieccanhan");

          } else {

          }
        } else {
          this.spinner.hide("spinner-ketqualamvieccanhan");


        }

      },
    })

  }
  LineChartTyLeMoCaseOptions(xAxisData, data, TiLeMoCaseChartPieces) {
    this.TiLeMoCaseChartOptions = {
      title: {
        text: 'Tỷ lệ mở Case',
        subtext: `năm ${this.dateValue.getFullYear()}`
      },
      tooltip: {
        trigger: 'axis',
        axisPointer: {
          type: 'cross'
        }
      },
      toolbox: {
        show: true,
        feature: {
          saveAsImage: {}
        }
      },
      xAxis: {
        type: 'category',
        boundaryGap: false,
        data: xAxisData
      },
      yAxis: {
        type: 'value',
        axisLabel: {
          formatter: '{value} %'
        },
        axisPointer: {
          snap: true
        }
      },
      visualMap: {
        show: false,
        dimension: 1,
        pieces: TiLeMoCaseChartPieces
      },
      series: [
        {
          name: 'Electricity',
          type: 'line',
          smooth: true,
          data: data,
          markArea: {
            itemStyle: {
              color: 'rgba(255, 173, 177, 0.4)'
            },
          }

        }
      ]
    };

  }


  fetchMemberListData() {
    this.https.get<any>("/api/ThongTinCaNhan/NhanVienTFSName").subscribe({
      next: (res: any) => {
        this.MemberList = [];
        this.MemberList = [...res.data];
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        console.log(this.MemberList);

      }
    });
  }

  clear(table: Table) {
    table.clear();
  }
}
