import { Component, OnInit } from '@angular/core';
import * as ExcelJS from "exceljs";
import * as XLSX from "xlsx";
import { HttpClient } from '@angular/common/http';
import { IndividualDayOff, Member, IndividualDayOff_API_DO } from './NgayPhepCaNhan';
import {
  ConfirmationService,
  MessageService,
  PrimeNGConfig
} from "primeng/api";
import { Table } from 'primeng/table';
import { TitleComponent, TooltipComponent, LegendComponent, ToolboxComponent, GridComponent, VisualMapComponent } from 'echarts/components';
import { BarChart } from 'echarts/charts';


@Component({
  selector: 'app-ngay-phep-ca-nhan',
  templateUrl: './ngay-phep-ca-nhan.component.html',
  styleUrls: ['./ngay-phep-ca-nhan.component.scss']
})
export class NgayPhepCaNhanComponent implements OnInit {
  chartOptions: any;
  chartExtension: any;

  approvalStatusOptions = [
    { label: 'Duyệt', value: 'Đã duyệt' },
    { label: 'Chưa duyệt', value: 'Chưa duyệt' },
    { label: 'Từ chối', value: 'Từ chối' }
  ]

  IndividualDayOffs: IndividualDayOff[];
  IndividualDayOffInitState = {
    dateFrom: '',
    dateTo: '',
    sumDay: 0,
    member: {
      id: 0,
      fullName: '',
      nickName: ''
    },
    isAnnual: false,
    isWithoutPay: false,
    approvalStatus: 'Chưa duyệt',
    reason: '',
    note: ''
  };
  IndividualDayOff: IndividualDayOff = {
    ...this.IndividualDayOffInitState
  };

  MemberList: Member[] = [];

  filter_datefrom: string = '';
  filter_dateto: string = '';
  isValidDateRange: boolean = true;
  isValidDateRangeFilter: boolean = true;


  openDialog: boolean;
  viewIndividualDayOffDialog: boolean;
  editIndividualDayOffDialog: boolean;
  addNewIndividualDayOffDialog: boolean;

  userInfo: any = {};

  constructor(
    private https: HttpClient,
    private confirmationService: ConfirmationService,
    private messageService: MessageService,
    private primengConfig: PrimeNGConfig,
  ) {

  }

  ngOnInit() {
    this.filter_datefrom = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
    this.filter_dateto = new Date().toLocaleDateString('en-GB');
    this.fetchIndividualDayOffsData(this.convertDateFormat(this.filter_datefrom), this.convertDateFormat(this.filter_dateto));
    this.resetCalendarSelection();
    this.sumDay();

    this.primengConfig.ripple = true;
    this.fetchUserInfo();
    this.chartExtension = [BarChart, TitleComponent, TooltipComponent, LegendComponent, ToolboxComponent, GridComponent, VisualMapComponent];
  }

  checkIsLeader() {
    const user = sessionStorage.getItem('current-user');
    return JSON.parse(user).isLeader;
  }

  fetchUserInfo() {
    const user = sessionStorage.getItem('current-user');
    const userInfo = JSON.parse(user);
    const userId = +userInfo["id"];

    if (userInfo.role === 'admin') {
      return;
    }

    this.https.get<any>("/api/ThongTinCaNhan/" + userId).subscribe({
      next: (res: any) => {
        this.userInfo = res.data[0];
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

  sumDay() {
    const date1 = new Date(this.IndividualDayOff.dateFrom);
    const date2 = new Date(this.IndividualDayOff.dateTo);
    let diffDays = 0;

    if (date1 > date2) {
      this.isValidDateRange = false;
      this.IndividualDayOff.sumDay = 0;
      return;
    }
    else {
      while (date1 <= date2) {
        this.isValidDateRange = true;
        if (date1.getDay() !== 0 && date1.getDay() !== 6) {
          diffDays++;
        }
        date1.setDate(date1.getDate() + 1);
      }
    }
    this.IndividualDayOff.sumDay = diffDays;
  }

  validateInputDates() {
    if (this.filter_datefrom && this.filter_dateto) {
      let dateFrom = new Date(this.convertDateFormat(this.filter_datefrom));
      let dateTo = new Date(this.convertDateFormat(this.filter_dateto));

      if (dateFrom > dateTo) {
        this.isValidDateRangeFilter = false;
      } else {
        this.isValidDateRangeFilter = true;
      }
    }
  }

  fetchDataFiltered() {
    let input_filter_datefrom = null;
    let input_filter_dateto = null;
    if (this.filter_datefrom) {
      input_filter_datefrom = this.convertDateFormat(this.filter_datefrom);
    }
    if (this.filter_dateto) {
      input_filter_dateto = this.convertDateFormat(this.filter_dateto);
    }
    this.fetchIndividualDayOffsData(input_filter_datefrom, input_filter_dateto);
  }

  convertDateFormat(date: string): string {
    const [day, month, year] = date.split('/');
    return `${month}/${day}/${year}`;
  }

  openViewDialog(data: any) {
    this.viewIndividualDayOffDialog = true;
    this.IndividualDayOff = { ...data };
    console.log(this.IndividualDayOff);

  }

  openAddDialog() {
    this.isValidDateRange = true;
    this.fetchMemberListData();
    this.IndividualDayOff = {
      ...this.IndividualDayOffInitState
    };
    this.resetCalendarSelection();
    this.sumDay();
    this.editIndividualDayOffDialog = false;
    this.addNewIndividualDayOffDialog = true;
    this.openDialog = true;
  }

  openEditDialog(data: any) {
    this.isValidDateRange = true;
    this.IndividualDayOff = {};
    this.IndividualDayOff = { ...data };
    this.IndividualDayOff.dateFrom = new Date(data.dateFrom);
    this.IndividualDayOff.dateTo = new Date(data.dateTo);
    this.addNewIndividualDayOffDialog = false;
    this.editIndividualDayOffDialog = true;
    this.openDialog = true;
  }

  fetchIndividualDayOffsData(input_filter_datefrom?: string, input_filter_dateto?: string) {
    let user = JSON.parse(sessionStorage.getItem('current-user'));
    let params: any = {};
    if (input_filter_datefrom) {
      params.query_dateFrom = input_filter_datefrom + ' 00:00:00';
    }
    if (input_filter_dateto) {
      params.query_dateTo = input_filter_dateto + ' 00:00:00';
    }

    if (!user.isLeader) {
      params.query_memberId = user.id;
    }

    this.https.get<any>("/api/NgayPhepCaNhan", { params: params }).subscribe({
      next: (res: any) => {
        this.convertType(res);
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.fetchMemberListData();
      }
    });
  }

  sortInitData() {
    this.IndividualDayOffs.sort((a, b) => {
      const dateA = new Date(a.dateFrom);
      const dateB = new Date(b.dateFrom);
      dateA.setHours(0, 0, 0, 0);
      dateB.setHours(0, 0, 0, 0);
      return dateB.getTime() - dateA.getTime();
    });
  }


  calculateData(): any {
    const memberTotalDayOffs = {};
    this.IndividualDayOffs.forEach((individualDayOff: IndividualDayOff) => {
      const memberName = individualDayOff.member.fullName;
      if (memberTotalDayOffs.hasOwnProperty(memberName)) {
        memberTotalDayOffs[memberName] += individualDayOff.sumDay;
      } else {
        memberTotalDayOffs[memberName] = individualDayOff.sumDay;
      }
    });
    return memberTotalDayOffs;
  }

  chartData(data) {
    const xAxisData = Object.keys(data);
    const SeriesData = Object.values(data);
    this.chartOptions = {
      xAxis: {
        type: 'category',
        data: xAxisData
      },
      yAxis: {
        type: 'value'
      },
      series: [
        {
          data: SeriesData,
          type: 'bar',
          label: {
            show: true,            // Enable labels
            position: 'inside',    // 'inside' or 'top' for label position
            formatter: '{c}',      // Display the value
            color: '#fff',         // Text color, adjust if needed
            fontWeight: 'bold'     // Make the text bold for better readability
          }
        }
      ]
    };
  }


  convertType(res: any): void {
    this.IndividualDayOffs = res.data.map((item: IndividualDayOff_API_DO) => {
      const { memberId, ...rest } = item;
      const individualDayOff: IndividualDayOff = {
        ...rest,
        member: {
          id: memberId,
          fullName: '',
          nickName: ''
        },
      };
      return individualDayOff;
    });
  }

  fetchMemberListData() {
    this.https.get<any>("/api/ThongTinCaNhan/NhanVienCongTac").subscribe({
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
        this.loadMember_From_MemberList();
      }
    });
  }

  loadMember_From_MemberList() {
    this.IndividualDayOffs.forEach((individualdayoff: IndividualDayOff) => {
      const foundMember = this.MemberList.find((m: Member) => m.id === individualdayoff.member.id);
      if (foundMember) {
        individualdayoff.member.fullName = foundMember.fullName;
        individualdayoff.member.nickName = foundMember.nickName;
      }
    });
    this.chartData(this.calculateData());
    this.sortInitData();
  }

  hideDialog() {
    this.IndividualDayOff = {};
    this.resetCalendarSelection();
    this.openDialog = false;
    this.editIndividualDayOffDialog = false;
    this.addNewIndividualDayOffDialog = false;
  }

  resetCalendarSelection() {
    this.IndividualDayOff.dateFrom = new Date();
    this.IndividualDayOff.dateFrom.setHours(0, 0, 0, 0);
    this.IndividualDayOff.dateTo = new Date();
    this.IndividualDayOff.dateTo.setHours(0, 0, 0, 0);
  }

  addNewIndividualDayOff() {
    let IndividualDayOffArray: IndividualDayOff[] = [this.IndividualDayOff];
    IndividualDayOffArray.forEach((item: any) => {
      const user = sessionStorage.getItem('current-user');
      if (!JSON.parse(user).isLeader) {
        item.memberId = JSON.parse(user).id
      } else {
        item.memberId = item.member.id;
      }
      delete item.member;
    });

    this.https.post<any>("/api/NgayPhepCaNhan", IndividualDayOffArray).subscribe({
      next: (res: any) => {
        //todo
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.resetCalendarSelection();
        this.fetchIndividualDayOffsData();
      }
    });
    this.addNewIndividualDayOffDialog = false;
    this.IndividualDayOff = {};
    this.hideDialog();
  }

  updateIndividualDayOff() {
    let IndividualDayOffData: any = structuredClone(this.IndividualDayOff);

    let user = JSON.parse(sessionStorage.getItem('current-user'));

    if (!user.isLeader) {
      IndividualDayOffData = {
        ...IndividualDayOffData,
        memberId: user.id
      }
    } else {
      IndividualDayOffData = {
        ...this.IndividualDayOff,
        memberId: this.IndividualDayOff.member.id
      }
    }

    this.https.put<any>("/api/NgayPhepCaNhan/" + this.IndividualDayOff.id, IndividualDayOffData).subscribe({
      next: (res: any) => {
        //todo
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.fetchIndividualDayOffsData();
      }
    });
    this.hideDialog();
  }

  deleteIndividualDayOff(event: Event, data: any) {

    this.confirmationService.confirm({
      target: event.target,
      message: "Xóa dữ liệu?",
      acceptLabel: 'Xóa',
      rejectLabel: 'Quay lại',
      icon: "pi pi-exclamation-triangle",
      accept: () => {
        this.messageService.add({
          severity: "info",
          summary: "Hủy thành công",
          detail: "Đã hủy dữ liệu khỏi hệ thống"
        });

        this.https.delete<any>("/api/NgayPhepCaNhan/" + data.id, data).subscribe({
          next: (res: any) => {
            //todo
          },
          error: (error) => {
            console.log(error);
            // Your logic for handling errors
          },
          complete: () => {
            // Your logic for handling the completion event (optional)
            this.fetchIndividualDayOffsData();
          }
        });
      },
      reject: () => {
        this.messageService.add({
          severity: "error",
          summary: "Hủy xóa dữ liệu",
          detail: "Hủy xóa dữ liệu khỏi hệ thống"
        });
      }
    });
  }

  saveApprovalStatus(data: any) {
    let id = data.id;
    let approvalStatus = data.approvalStatus;
    this.https.put<any>("/api/NgayPhepCaNhan/DuyetNgayPhep", { id, approvalStatus }).subscribe({
      next: (res: any) => {
        //todo
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.fetchIndividualDayOffsData();
      }
    });

  }

  clear(table: Table) {
    table.clear();
    this.filter_datefrom = new Date(new Date().getFullYear(), 0, 1).toLocaleDateString('en-GB');
    this.filter_dateto = new Date().toLocaleDateString('en-GB');
    this.fetchIndividualDayOffsData(this.convertDateFormat(this.filter_datefrom), this.convertDateFormat(this.filter_dateto));
  }
  exportToExcel() {
    const workbook = new ExcelJS.Workbook();
    const worksheet = workbook.addWorksheet("AQ Members");
    // Define column headers based on AQMember interface
    worksheet.columns = [
      // Export headers including missing fields
      { header: "ID", key: "id", width: 10 },
      { header: "Trạng thái", key: "approvalStatus", width: 15 },
      { header: "Ngày bắt đầu", key: "dateFrom", width: 15 },
      { header: "Ngày kết thúc", key: "dateTo", width: 15 },
      { header: "Tổng số ngày nghỉ", key: "sumDay", width: 15 },
      { header: "Họ tên", key: "fullName", width: 30 },
      { header: "Nickname", key: "nickName", width: 15 },
      { header: "Lý do nghỉ", key: "reason", width: 15 },
      { header: "Nghỉ không lương", key: "isWithoutPay", width: 15 },
      { header: "Tính vào nghỉ phép năm", key: "isAnnual", width: 15 },
      { header: "note", key: "note", width: 15 },
      { header: "memberId", key: "memberId", width: 15 },
    ];
    this.IndividualDayOffs.forEach((day) => {


      worksheet.addRow({
        id: day.id,
        fullName: day.member.fullName,
        nickName: day.member.nickName,
        dateFrom: day.dateFrom instanceof Date
          ? day.dateFrom.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
          : new Date(day.dateFrom).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' }),
        dateTo: day.dateTo instanceof Date
          ? day.dateTo.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
          : new Date(day.dateTo).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' }),

        sumDay: day.sumDay,
        reason: day.reason,
        isAnnual: this.convertBooleanToString(day.isAnnual),
        isWithoutPay: this.convertBooleanToString(day.isWithoutPay),
        approvalStatus: day.approvalStatus,
        note: day.note,
        memberId: day.member.id
        // Add more properties if necessary
      });
    });

    // Save the workbook
    workbook.xlsx.writeBuffer().then((buffer) => {
      const blob = new Blob([buffer], { type: "application/octet-stream" });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = "NgayPhepCaNhan.xlsx";
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }

  importFromExcel(excelData: any) {
    console.log(this.IndividualDayOffs);

    // Assuming the first row of the excelData is the header
    const headers = excelData[0]; // Get the headers from the first row
    const ngayPheps: any[] = []; // Array to hold the transformed AQMember objects

    for (let i = 1; i < excelData.length; i++) {
      //Start from the second row
      const row = excelData[i];
      if (row.length === headers.length) {
        // Ensure the row length matches the headers
        const ngayPhep: any = {
          id: row[0] || null, // Assuming id is in the first column
          approvalStatus: row[1] || null,
          dateFrom: row[2] ? this.convertToISOString(row[2]) : null,
          dateTo: row[3] ? this.convertToISOString(row[3]) : null,
          sumDay: row[4] || null,
          fullName: row[5] || null, // Assuming tfsName is in the second column
          nickName: row[6] || null, // Assuming nickName is in the ninth column
          reason: row[7] || null,
          isWithoutPay: row[8] ? this.convertStringToBoolean(row[8]) : false,
          isAnnual: row[9] ? this.convertStringToBoolean(row[9]) : false,
          note: row[10] || null,
          memberId: row[11] || null,

        };
        ngayPheps.push(ngayPhep);
      }
    }
    console.log(ngayPheps);

    this.addImportNgayPhep(ngayPheps);
  }

  handleFileInput(files: FileList) {
    if (files.length > 0) {
      const file = files.item(0);
      const reader = new FileReader();
      reader.onload = (event: any) => {
        const data = new Uint8Array(event.target.result);
        const workbook = XLSX.read(data, { type: "array" });
        const worksheet = workbook.Sheets[workbook.SheetNames[0]];
        const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: 1 });

        this.importFromExcel(jsonData);
      };
      reader.readAsArrayBuffer(file); // Read the file as an ArrayBuffer
    }
  }
  addImportNgayPhep(ngayPheps: any) {
    const req = []
    ngayPheps.forEach(ngayphep => {

      const body = {
        id: ngayphep.id,
        fullName: ngayphep.fullName,
        nickName: ngayphep.nickName,
        dateFrom: ngayphep.dateFrom,
        dateTo: ngayphep.dateTo,
        sumDay: ngayphep.sumDay,
        reason: ngayphep.reason,
        isAnnual: ngayphep.isAnnual,
        isWithoutPay: ngayphep.isWithoutPay,
        approvalStatus: ngayphep.approvalStatus,
        note: ngayphep.note,
        memberId: ngayphep.memberId
      };
      req.push(body)
    });
    this.https.post<any>("/api/NgayPhepCaNhan", req).subscribe({
      next: (res: any) => {
        console.log(res);
        this.fetchIndividualDayOffsData();

      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        console.log(this.IndividualDayOffs);
      },
    });
  }
  convertToISOString(dateString: string): string {
    // Assume the input is in "dd/mm/yyyy" format
    const [day, month, year] = dateString.split('/').map(Number);

    // Create a new Date object (the month in JavaScript is 0-indexed)
    const date = new Date(year, month - 1, day);

    // Convert to ISO string (this gives UTC format)
    const isoString = date.toISOString();

    // Adjust the timezone if needed (assuming it's +07:00)
    const timezoneOffset = "+07:00";
    const localISOString = isoString.replace('Z', timezoneOffset);

    return localISOString;
  }
  convertStringToBoolean(value: string): boolean {
    // Check if the value is "có" for true, "không" for false, or keep it as it is (if valid boolean)
    return value.trim().toLowerCase() === 'có' ? true : value.trim().toLowerCase() === 'không' ? false : Boolean(value);
  }
  convertBooleanToString(value: boolean): string {
    return value ? 'có' : 'không';
  }

}
