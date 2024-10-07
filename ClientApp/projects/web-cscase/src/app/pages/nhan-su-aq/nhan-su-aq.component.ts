import * as ExcelJS from "exceljs";
import * as XLSX from "xlsx";
import * as Papa from "papaparse";
import { Component, OnInit } from "@angular/core";
import {
  AQMember,
  AQMemberUpdateDO,
  AQMemberInsertDO,
  detailContract,
} from "./AQMember";
import { AQRole } from "./AQMember";
import { HttpClient } from "@angular/common/http";
import {
  ConfirmationService,
  MessageService,
  PrimeNGConfig,
} from "primeng/api";
import { Table } from "primeng/table";
import { PieChart } from "echarts/charts";
import {
  TitleComponent,
  TooltipComponent,
  LegendComponent,
} from "echarts/components";

@Component({
  selector: "app-nhan-su-aq",
  templateUrl: "./nhan-su-aq.component.html",
  styleUrls: ["./nhan-su-aq.component.scss"],
})
export class NhanSuAqComponent implements OnInit {
  AQmembers: AQMember[];

  detailContractInsert: detailContract = {
    // contractStartDate: new Date(new Date().setHours(0, 0, 0, 0)),
    // contractExpireDate: new Date(new Date().setHours(0, 0, 0, 0)),
    // contractStartDate: new Date(),
    // contractExpireDate: new Date(),
    contractDuration: 1,
    contractType: "",
  };

  aqmemberInsert: AQMemberInsertDO = {
    detailContract: this.detailContractInsert,
  };

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
    private primengConfig: PrimeNGConfig
  ) {
    this.AQRoles = [
      { role: "Developer", code: "1", total: 0 },
      { role: "Support", code: "2", total: 0 },
      { role: "Sale", code: "3", total: 0 },
      { role: "HR", code: "4", total: 0 },
      { role: "BM", code: "5", total: 0 },
    ];
    this.pc2_echartsExtentions = [
      PieChart,
      TitleComponent,
      TooltipComponent,
      LegendComponent,
    ];
  }

  ngOnInit(): void {
    this.constructor;
    this.primengConfig.ripple = true;
    this.fetchAQMemberData();
  }

  checkIsLeader() {
    let user = sessionStorage.getItem("current-user");
    return JSON.parse(user).isLeader;
  }

  handleUploadAvatar(event) {
    if (event.target.files && event.target.files[0]) {
      var reader = new FileReader();
      reader.readAsDataURL(event.target.files[0]); // read file as data url
      reader.onload = (event) => {
        // called once readAsDataURL is completed
        if (this.openAddDialog)
          this.aqmemberInsert.avatar = event.target.result as string;
        if (this.openEditDialog)
          this.aqmemberUpdate.avatar = event.target.result as string;
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
        this.AQmembers.forEach((member) => { });

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
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.AQRoles.forEach((role) => {
          role.total = this.AQmembers.filter(
            (member) => member.role === role.code
          ).length;
        });
        this.fetchDataPC2();
      },
    });
  }

  fetchDataPC2() {
    this.pc2_echartsOptions = {
      title: {
        text: "Nhân sự AQ",
        subtext: "Tỉ lệ và số lượng nhân viên các phòng ban",
        left: "center",
        textStyle: {
          fontFamily: "Arial, sans-serif",
          fontSize: 20,
          fontWeight: "bold",
        },
      },
      tooltip: {
        trigger: "item",
        formatter: "{b}: {c} ({d}%)", // Display label, value, and percentage in tooltip
      },
      legend: {
        orient: "vertical",
        bottom: "bottom",
      },
      series: [
        {
          name: "Phòng ban",
          type: "pie",
          radius: "50%",
          data: this.AQRoles.map((role, index) => ({
            value: role.total,
            name: role.role,
            itemStyle: {
              color: this.getColor(index), // Call getColor function to get color for each label
            },
            label: {
              show: true,
              formatter: "{b}: {c} ({d}%)", // Display label and percentage in label
            },
            emphasis: {
              itemStyle: {
                shadowBlur: 10,
                shadowOffsetX: 0,
                shadowColor: "rgba(0, 0, 0, 0.5)",
              },
              label: {
                show: true,
                formatter: "{b}: {c} ({d}%)", // Display label and percentage in emphasis
              },
            },
          })),
        },
      ],
    };
  }

  getColor(index: number): string {
    const colors = ["#b3e5fc", "#ffd8b2", "#8dbca1", "#fe1155", "#0a3d62"];
    return colors[index % colors.length];
  }

  getRoleName(code: string): string {
    const roleObj = this.AQRoles.find((role) => role.code === code);
    return roleObj ? roleObj.role : "Unknown";
  }

  sortInitData() {
    this.AQmembers.sort((a, b) => {
      const roleA = new Date(a.role);
      const roleB = new Date(b.role);
      return roleA < roleB ? 1 : -1;
    });
  }

  openAddDialog() {
    console.log(this.aqmemberInsert);

    this.aqmemberInsert = {
      detailContract: this.detailContractInsert,
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
      detailContract:
        data.detailContract === null
          ? {
            // contractStartDate: new Date(),
            // contractExpireDate: new Date(),
            contractDuration: 0,
            contractType: "",
          }
          : {
            ...this.aqmemberUpdate.detailContract,
            contractStartDate: new Date(
              data.detailContract.contractStartDate
            ),
            contractExpireDate: new Date(
              data.detailContract.contractExpireDate
            ),
          },
    };
    this.addNewMemberDialog = false;
    this.editMemberDialog = true;
    this.openDialog = true;
  }

  handleIsLunchStatusChange(event) {
    this.aqmemberUpdate.isLunchStatus = event.checked;
    const currentYear = new Date().getFullYear();
    const detailLunchCurrentYear = this.aqmemberUpdate.detailLunch.find(
      (item) => item.year === currentYear
    );

    if (detailLunchCurrentYear) {
      detailLunchCurrentYear.lunchByMonth.forEach((lunchByMonth) => {
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
    if (
      this.aqmemberInsert.avatar === undefined ||
      this.aqmemberInsert.avatar === ""
    ) {
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
      },
    });

    this.AQmembers = [...this.AQmembers];
    this.addNewMemberDialog = false;
    this.aqmemberInsert = {};
    this.deleteInputImg();
    this.hideDialog();
  }

  updateMember() {
    console.log("before: ", this.AQmembers);

    this.https
      .put<any>(
        "/api/ThongTinCaNhan/" + this.aqmemberUpdate.id,
        this.aqmemberUpdate
      )
      .subscribe({
        next: (res: any) => {
          const index = this.AQmembers.findIndex(
            (member) => member.id === this.aqmemberUpdate.id
          );
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
        },
      });
    this.hideDialog();
  }

  deleteMember(event: Event, data: any) {
    this.confirmationService.confirm({
      target: event.target,
      message: "Xóa bạn này khỏi công ty?",
      acceptLabel: "Xóa",
      rejectLabel: "Quay lại",
      icon: "pi pi-exclamation-triangle",
      accept: () => {
        this.messageService.add({
          severity: "info",
          summary: "Xóa thành công",
          detail: "Đã xóa tài khoản khỏi hệ thống",
        });

        this.https
          .delete<any>("/api/ThongTinCaNhan/" + data.id, data)
          .subscribe({
            next: (res: any) => {
              // console.log(res);
              this.AQmembers = this.AQmembers.filter(
                (val) => val.id !== data.id
              );
            },
            error: (error) => {
              console.log(error);
              // Your logic for handling errors
            },
            complete: () => {
              // Your logic for handling the completion event (optional)
              this.fetchAQMemberData();
            },
          });
      },
      reject: () => {
        this.messageService.add({
          severity: "error",
          summary: "Hủy xóa tài khoản",
          detail: "Hủy xóa tài khoản khỏi hệ thống",
        });
      },
    });
  }

  clear(table: Table) {
    table.clear();
    this.fetchAQMemberData();
  }
  exportToExcel() {
    const workbook = new ExcelJS.Workbook();
    const worksheet = workbook.addWorksheet("AQ Members");
    // Define column headers based on AQMember interface
    worksheet.columns = [
      // Export headers including missing fields
      { header: "ID", key: "id", width: 10 },
      { header: "TFS Name", key: "tfsName", width: 30 },
      { header: "Họ tên", key: "fullName", width: 30 },
      { header: "Email", key: "email", width: 30 },
      { header: "Số điện thoại", key: "phone", width: 20 },
      { header: "Avatar", key: "avatar", width: 30 }, // Assuming URL or base64
      { header: "Ngày sinh", key: "birthDate", width: 15 },
      { header: "Ngày vào công ty", key: "startDate", width: 15 },
      { header: "Nickname", key: "nickName", width: 15 },
      { header: "Phòng ban", key: "role", width: 15 },
      { header: "Leader", key: "isLeader", width: 10 },
      { header: "Trợ cấp ăn trưa", key: "isLunchStatus", width: 15 },
      { header: "Trạng thái làm việc", key: "isActive", width: 10 },
      { header: "Mã số CCCD", key: "maSoCCCD", width: 20 },
      { header: "Địa chỉ", key: "address", width: 30 },
      { header: "Thâm niên", key: "workingYear", width: 15 },
      { header: "Loại hợp đồng", key: "contractType", width: 15 },
      { header: "Thời hạn hợp đồng", key: "contractDuration", width: 15 },
      { header: "Ngày bắt đầu hợp đồng", key: "contractStartDate", width: 15 },
      { header: "Ngày hợp đồng hết hạn", key: "contractExpireDate", width: 15 },
    ];
    this.AQmembers.forEach((member) => {
      console.log(member.detailContract.contractStartDate);

      worksheet.addRow({
        id: member.id,
        tfsName: member.tfsName,
        fullName: member.fullName,
        email: member.email,
        phone: member.phone,
        avatar: member.avatar,
        birthDate: member.birthDate instanceof Date
          ? member.birthDate.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
          : new Date(member.birthDate).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' }),
        startDate: member.startDate instanceof Date
          ? member.startDate.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
          : new Date(member.startDate).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' }),
        nickName: member.nickName,
        role: member.role,
        isLeader: this.convertBooleanToString(member.isLeader),
        isLunchStatus: this.convertBooleanToString(member.isLunchStatus),
        isActive: this.convertBooleanToString(member.isActive),
        maSoCCCD: member.maSoCCCD,
        address: member.address,
        workingYear: member.workingYear,
        contractDuration: member.detailContract.contractDuration,
        contractStartDate: member.detailContract.contractStartDate instanceof Date
          ? member.detailContract.contractStartDate.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
          : new Date(member.detailContract.contractStartDate).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' }),
        contractExpireDate: member.detailContract.contractExpireDate instanceof Date
          ? member.detailContract.contractExpireDate.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' })
          : new Date(member.detailContract.contractStartDate).toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' }),
        // Add more properties if necessary
      });
    });

    // Save the workbook
    workbook.xlsx.writeBuffer().then((buffer) => {
      const blob = new Blob([buffer], { type: "application/octet-stream" });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = "AQMembers.xlsx";
      a.click();
      window.URL.revokeObjectURL(url);
    });
  }

  importFromExcel(excelData: any) {
    // Assuming the first row of the excelData is the header
    const headers = excelData[0]; // Get the headers from the first row
    const members: any[] = []; // Array to hold the transformed AQMember objects

    for (let i = 1; i < excelData.length; i++) {
      //Start from the second row
      const row = excelData[i];

      if (row.length === headers.length) {
        // Ensure the row length matches the headers
        const member: any = {
          id: row[0] || null, // Assuming id is in the first column
          tfsName: row[1] || null, // Assuming tfsName is in the second column
          fullName: row[2] || null, // Assuming fullName is in the third column
          email: row[3] || null, // Assuming email is in the fourth column
          phone: row[4] || null, // Assuming phone is in the fifth column
          avatar: row[5] || null, // Assuming avatar URL is in the sixth column
          birthDate: row[6] ? this.convertToISOString(row[6]) : null, // Assuming birthDate is in the seventh column
          startDate: row[7] ? this.convertToISOString(row[7]) : null, // Assuming startDate is in the eighth column
          nickName: row[8] || null, // Assuming nickName is in the ninth column
          role: row[9] || null, // Assuming role is in the tenth column
          isLeader: this.convertStringToBoolean(row[10]), // Assuming isLeader is in the eleventh column
          isLunchStatus: this.convertStringToBoolean(row[11]), // Assuming isLunchStatus is in the twelfth column
          isActive: this.convertStringToBoolean(row[12]), // Assuming isActive is in the sixteenth column
          maSoCCCD: row[13] || null, // Assuming maSoCCCD is in the seventeenth column
          address: row[14] || null, // Assuming address is in the eighteenth column
          workingYear: row[15] ? Number(row[15]) : null, // Assuming workingYear is in the nineteenth column
          contractType: row[16] ? JSON.parse(row[16]) : null, // Assuming detailContract is in the twentieth column
          contractDuration: row[17] || null, // Assuming detailContract is in the twentieth column
          contractStartDate: row[18] ? this.convertToISOString(row[18]) : null, // Assuming detailContract is in the twentieth column
          contractExpireDate: row[19] ? this.convertToISOString(row[19]) : null, // Assuming detailContract is in the twentieth column
        };

        members.push(member);
      }
    }

    const temptData = [...this.AQmembers, ...members]; // Append new members to the existing array
    this.addImportMember(members);
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
  addImportMember(members: any) {
    const req = []
    members.forEach(member => {
      const detailContract = {
        contractType: member.contractType, // Assuming detailContract is in the twentieth column

        contractDuration: member.contractDuration, // Assuming detailContract is in the twentieth column
        contractStartDate: member.contractStartDate, // Assuming detailContract is in the twentieth column
        contractExpireDate: member.contractExpireDate, // Assuming detailContract is in the twentieth column
      }
      const body = {
        id: member.id, // Assuming id is in the first column
        tfsName: member.tfsName, // Assuming tfsName is in the second column
        fullName: member.fullName, // Assuming fullName is in the third column
        email: member.email, // Assuming email is in the fourth column
        phone: member.phone, // Assuming phone is in the fifth column
        avatar: member.avatar, // Assuming avatar URL is in the sixth column
        birthDate: member.birthDate, // Assuming birthDate is in the seventh column
        startDate: member.startDate, // Assuming startDate is in the eighth column
        nickName: member.nickName, // Assuming nickName is in the ninth column
        role: member.role, // Assuming role is in the tenth column
        isLeader: member.isLeader, // Assuming isLeader is in the eleventh column
        isLunchStatus: member.isLunchStatus, // Assuming isLunchStatus is in the twelfth column
        // detailLunch: member.detailLunch, // Assuming detailLunch is in the thirteenth column
        // detailWFHQuota: member.detailWFHQuota, // Assuming detailWFHQuota is in the fourteenth column
        // detailAbsenceQuota: member.detailAbsenceQuota, // Assuming detailAbsenceQuota is in the fifteenth column
        isActive: member.isActive, // Assuming isActive is in the sixteenth column
        maSoCCCD: member.maSoCCCD, // Assuming maSoCCCD is in the seventeenth column
        address: member.address, // Assuming address is in the eighteenth column
        // workingYear: member.workingYear, // Assuming workingYear is in the nineteenth column
        detailContract: detailContract
      };
      req.push(body)
    });
    this.https.post<any>("/api/ThongTinCaNhan", req).subscribe({
      next: (res: any) => {
        console.log(res);
        this.fetchAQMemberData();

      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        console.log(this.AQmembers);
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

function hexToRgb(hex: string): { r: number; g: number; b: number } {
  const bigint = parseInt(hex, 16);
  const r = (bigint >> 16) & 255;
  const g = (bigint >> 8) & 255;
  const b = bigint & 255;
  return { r, g, b };
}
