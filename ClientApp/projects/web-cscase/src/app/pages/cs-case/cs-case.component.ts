import { DatePipe } from "@angular/common";
import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormControl } from "@angular/forms";
import { Router } from "@angular/router";
import {
  CSCaseDataDO, DataLoaiCase,
  DataPhanHe, DataState, DataTruong, EduCase, ServerLinkService,
  User
} from "@mylibs";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { NgxPrinterService } from "ngx-printer";
import { NgxSpinnerService } from "ngx-spinner";
import * as XLSX from "xlsx";
import { DataServices } from "../../service/dataservices.service";
import { MessBoxComponent } from "../../service/mess-box/mess-box.component";
import { ChitietcaseComponent } from "../chitietcase/chitietcase.component";
import { NoidungcscaseComponent } from "../noidungcscase/noidungcscase.component";
import { TraodoiComponent } from "../traodoi/traodoi.component";

@Component({
  selector: "app-cs-case",
  templateUrl: "./cs-case.component.html",
  styleUrls: ["./cs-case.component.scss"],
})
export class CsCaseComponent implements OnInit {
  public cbState: DataState[];
  public cbloaicase: DataLoaiCase[];
  public cbphanhe: DataPhanHe[];
  public currentUser: User;
  public ismess = "";
  private rs: any;
  private allSubjects: any;
  public isadmin: boolean = false;
  public isadministrator: boolean = false;
  private data_Goc: CSCaseDataDO;
  private dataCsCase_Goc: EduCase[];
  public dataCsCase_Filter: EduCase[];
  public dataCsCase_Filter_Ad: any = [];
  public IsTFS = false;
  private dataCsCase_Search: EduCase[];
  private dataCsCase_Temp: EduCase[];
  public orderby = "macase";
  public reverseOrderby = false;
  public cbMaTruong: DataTruong[];
  public formControl = new FormControl();
  public selectrangthai = [];
  public selectmatruong = [];
  public selectloaicase = [];
  public selectphanhe = [];
  public searchText = "";

  //  private listmatruong: any; private liststate: any; private listloaicase: any; private listphanhe: any;
  public total = 0;
  public currentPage = 1;
  public pages = 1;
  public pageLimit = 1;
  public disabled = false;
  public isHideMail: boolean = true;
  public value: string;
  public popupTooltip = "";

  constructor(
    private http: HttpClient,
    private modalService: BsModalService,
    private router: Router,
    private spinner: NgxSpinnerService,
    private printerService: NgxPrinterService,
    private serverLink: ServerLinkService,
    private dataServices: DataServices,
    public bsModalRef1: BsModalRef
  ) {
    this.pageLimit = 100;
    this.isHideMail = true;

    if (sessionStorage.getItem("current-user")) {
      this.currentUser = JSON.parse(sessionStorage.getItem("current-user"));
      this.currentUser.pass = "";
      sessionStorage.setItem("current-user", JSON.stringify(this.currentUser));
      if (this.currentUser.roles.toLowerCase() === "admin") {
        this.isadmin = true;
      } else if (this.currentUser.roles.toLowerCase() === "administrator") {
        this.isadministrator = true;
        this.pageLimit = 200;
      }
    }
  }

  public ngOnInit() {
    // Mở case          #008FFB
    // Đang phân tích   #008000
    // Đang xử lý       #6f0080
    // Đang test (Đã xử lý)         #b10754
    // Đóng case        #546E7A

    // BF - Lỗi cần sửa code              #FEB019
    // EX - Lỗi Exception cần sửa code    #FF4560
    // NF - Yêu cầu mới cần sửa code      #008FFB
    // CV - Trao đổi hoặc chưa phân loại  #26a69a

    this.selectmatruong = null;
    sessionStorage.removeItem("matruong");
    this.value = "macase";
    this.dataCsCase_Filter = [];
    // this.liststate = '';
    this.selectrangthai = null;

    if (sessionStorage.getItem("resfesh") !== "1") {
      if (sessionStorage.getItem("db")) {
        sessionStorage.setItem("s", "1");
        this.xulycatch(JSON.parse(sessionStorage.getItem("db")));
        return;
      } else {
        if (sessionStorage.getItem("s") !== "1") {
          this.LoadCSCase();
        }
      }
    } else {
      //tao case xong refesh lại case
      this.LoadCSCase();
    }
  }

  private LoadCSCase() {
    this.ismess = "";
    const body = { filter: { ngaychot: "" }, dateRange: localStorage.getItem("dateRange") };

    this.spinner.show("spinner");
    this.http.post<any>("/api/main/cscase", body).subscribe(
      (res: any) => {
        if (res && res.code === 200) {
          if (res.data) {
            this.data_Goc = res.data;
            sessionStorage.setItem("db", JSON.stringify(this.data_Goc));
            sessionStorage.setItem("s", "1");
            this.dataServices.IsData.next(true);
            sessionStorage.removeItem("resfesh");
            this.xulycatch(this.data_Goc);
          } else {
            this.spinner.hide("spinner");
            this.ismess = "Không tìm thấy dữ liệu";
            this.dataCsCase_Filter = this.dataCsCase_Temp = [];
            this.Disconnect(true);
          }
        } else {
          this.spinner.hide("spinner");
          this.Disconnect();
        }
      },
      (error) => {
        this.spinner.hide("spinner");
        this.Disconnect();
      }
    );

    setTimeout(() => {
      this.spinner.hide("spinner");
    }, 60000);
  }

  private xulycatch(data_goc) {
    this.spinner.show("spinner");

    if (data_goc.data_truong && data_goc.data_truong.length > 0) {
      this.cbMaTruong = data_goc.data_truong;
      // this.dataServices.Is_data_cbMaTruong.next(this.cbMaTruong);

      // console.log(this.cbMaTruong);

      if (this.cbMaTruong.length === 1) {
        this.selectmatruong = [this.currentUser.maTruong];
        this.disabled = true;
      }
    }

    /// IF LOAD BY FILE EXCEL
    if (!data_goc.is_tfs) {
      this.IsTFS = false;

      if (data_goc.data_case) {
        this.dataCsCase_Goc = data_goc.data_case.map((v) => ({
          macase: v.macase,
          matruong: v.matruong,
          tentruong: v.tentruong,
          ngaynhan: v.ngaynhan
            ? new DatePipe("en-US").transform(
              new Date(
                v.ngaynhan.substring(6) +
                "/" +
                v.ngaynhan.substring(3, 5) +
                "/" +
                v.ngaynhan.substring(0, 2)
              ).toDateString(),
              "yyyy-MM-ddTHH:mm:ss"
            )
            : "",
          chitietyc: v.chitietyc,
          trangthai: v.trangthai,
          ngaydukien: v.ngaydukien
            ? new DatePipe("en-US").transform(
              new Date(
                v.ngaydukien.substring(6) +
                "/" +
                v.ngaydukien.substring(3, 5) +
                "/" +
                v.ngaydukien.substring(0, 2)
              ).toDateString(),
              "yyyy-MM-ddTHH:mm:ss"
            )
            : "",
          loaihopdong: v.loaihopdong,
          mucdo: v.mucdo,
          hieuluc: v.hieuluc,
          dabangiao: v.dabangiao,
          ngayemail: v.ngayemail
            ? new DatePipe("en-US").transform(
              new Date(
                v.ngayemail.substring(6) +
                "/" +
                v.ngayemail.substring(3, 5) +
                "/" +
                v.ngayemail.substring(0, 2)
              ).toDateString(),
              "yyyy-MM-ddTHH:mm:ss"
            )
            : "",
          mailto: v.mailto.split(";").join(";<br>"),
          loaicase: v.loaicase,
          phanhe: v.phanhe,
          whatnew: v.whatnew,
          teststate: v.teststate,
        }));
      }
    } else {
      /// IF LOAD BY FILE TFS
      this.IsTFS = true;
      this.dataCsCase_Goc = data_goc.data_case;
    }

    // nếu có dữ liệu dataCsCase_Goc
    if (this.dataCsCase_Goc && this.dataCsCase_Goc.length > 0) {
      this.dataCsCase_Goc.forEach((s) => {
        if (s.mailto) {
          s.mailto = s.mailto.split(";").join("<br>");
        }
        if (s.loaicase.startsWith("CV")) {
          s.ngaydukien = "";
          s.hieuluc = "";
        }
      });

      if (this.isadministrator) {
        this.dataCsCase_Goc = this.dataCsCase_Goc.filter(
          (s) =>
            s.trangthai?.toLowerCase() !== "đóng case" &&
            s.trangthai?.toLowerCase() !== "đang test"
        );
      }

      const combotrangthai_temp = this.dataCsCase_Goc
        .map((x) => x.trangthai)
        .filter((value, index, self) => self.indexOf(value) === index)
        .sort((a, b) => (a > b ? 1 : -1));
      this.cbState = combotrangthai_temp.map((t) => ({ trangthai: t }));

      const comboloaicase_temp = this.dataCsCase_Goc
        .map((x) => x.loaicase)
        .filter((value, index, self) => self.indexOf(value) === index)
        .sort((a, b) => (a > b ? 1 : -1));
      this.cbloaicase = comboloaicase_temp.map((l) => ({ loaicase: l }));

      const combophanhe_temp = this.dataCsCase_Goc
        .map((x) => x.phanhe)
        .filter((value, index, self) => self.indexOf(value) === index)
        .sort((a, b) => (a > b ? 1 : -1));
      this.cbphanhe = combophanhe_temp.map((p) => ({ phanhe: p }));

      // this.dataServices.Is_data_cbPhanHe.next(this.cbphanhe);

      if (this.isadministrator) {
        this.dataCsCase_Filter_Ad = [];

        // count so luong vào data
        this.dataCsCase_Goc.reduce((res, value) => {
          if (!res[value.matruong]) {
            res[value.matruong] = {
              matruong: value.matruong,
              tentruong: value.tentruong,
              soluong: 0,
            };
            this.dataCsCase_Filter_Ad.push(res[value.matruong]);
          }
          res[value.matruong].soluong += 1;
          return res;
        }, {});

        // add ten truong vào data
        this.allSubjects = this.cbMaTruong.reduce((a, b) => {
          a[b.matruong] = b.tentruong;
          return a;
        }, {});
        this.dataCsCase_Filter_Ad.forEach((s) => {
          s.tentruong = this.allSubjects[s.matruong];
        });
        this.dataCsCase_Filter_Ad = this.dataCsCase_Filter_Ad
          .filter((s) => s.tentruong)
          .sort((a, b) =>
            Number(a.soluong) < Number(b.soluong)
              ? 1
              : Number(a.soluong) > Number(b.soluong)
                ? -1
                : 0
          );
        this.spinner.hide("spinner");
      } else {
        this.dataCsCase_Temp = this.dataCsCase_Goc.sort((a, b) =>
          Number(a.macase) < Number(b.macase)
            ? 1
            : Number(a.macase) > Number(b.macase)
              ? -1
              : 0
        );

        this.dataCsCase_Filter = this.dataCsCase_Temp
          .slice((this.currentPage - 1) * this.pageLimit)
          .filter((_u, i) => i < this.pageLimit);
        this.total = this.dataCsCase_Goc.length ?? 0;
        this.pages = Math.ceil(
          (this.total + this.pageLimit - 1) / this.pageLimit
        );
      }

      this.spinner.hide("spinner");
    } else {
      this.ismess = "Không tìm thấy dữ liệu";
      this.dataCsCase_Filter = this.dataCsCase_Temp = [];
      this.spinner.hide("spinner");
    }

    this.spinner.hide("spinner");
  }

  public XemChiTiet(dt: any, type: number = 0) {
    const body = { filter: { ngaychot: "", macase: dt.macase }, dateRange: localStorage.getItem("dateRange") };

    this.spinner.show("spinner");
    this.http.post<any>("/api/main/cscase", body).subscribe(
      (res: any) => {
        if (res && res.code === 200) {
          if (res.data && res.data.data_case) {
            const initialState = { data: res.data.data_case, type };
            this.modalService.show(NoidungcscaseComponent, {
              initialState,
              ignoreBackdropClick: true,
              animated: false,
              class: "model-fullscreen", // 'modal-xl',
            });
            this.spinner.hide("spinner");
          } else {
            this.spinner.hide("spinner");
            this.ismess = "Không tìm thấy dữ liệu";
          }
        } else {
          this.spinner.hide("spinner");
        }
      },
      (error) => {
        this.spinner.hide("spinner");
      }
    );
  }

  public TraoDoi(dt: any) {
    // return;
    if (dt.loaicase.split(" - ")[0].toString() === "CV") {
      const initialState = {
        macase: dt.macase,
        tieu_de: dt.chitietyc,
        da_ban_giao: dt.dabangiao,
      };
      this.bsModalRef1 = this.modalService.show(TraodoiComponent, {
        initialState,
        ignoreBackdropClick: true,
        animated: false,
        class: "model-fullscreen", // 'modal-xl',
      });

      this.bsModalRef1.content.traodoiOut.subscribe((res) => {
        if (res.res === 200 && res.data) {
          dt.comment = res.data;
        }
      });
    }
  }

  private Disconnect(Mess: boolean = false) {
    if (Mess) {
      this.spinner.hide("spinner");
      const initialState = {
        icon: "w",
        button: "OK",
        message: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại",
      };
      this.modalService.show(MessBoxComponent, {
        initialState,
        ignoreBackdropClick: true,
        animated: false,
        class: "modal-dialog-centered",
      });
      this.modalService.onHide.subscribe(() => {
        this.spinner.hide("spinner");
        this.router.navigate(["/"]);
      });
    } else {
      this.spinner.hide("spinner");
      this.router.navigate(["/"]);
    }
  }

  public changeTruong(event: any) {
    // if (!event) {
    //   this.listmatruong = '';
    //   sessionStorage.setItem('matruong', '');
    //   return;
    // }

    // this.listmatruong = event;
    // sessionStorage.setItem('matruong', JSON.stringify(this.selectmatruong));
    this.doSearch();

    // console.log(this.selectmatruong);

    if (!event) {
      this.selectmatruong = [];
      sessionStorage.setItem("matruong", "");
      return;
    }
    sessionStorage.setItem("matruong", JSON.stringify(this.selectmatruong));

    this.doSearch();
  }

  public changState(event: any) {
    // if (!event) { this.liststate = ''; return; }
    // this.liststate = event;

    if (!event) {
      this.selectrangthai = [];
      return;
    }
    this.doSearch();
  }

  public changLoaiCase(event: any) {
    // if (!event) { this.listloaicase = ''; return; }
    // this.listloaicase = event;

    if (!event) {
      this.selectloaicase = [];
      return;
    }
    this.doSearch();
  }

  public changPhanHe(event: any) {
    // if (!event) { this.listphanhe = ''; return; }
    // this.listphanhe = event;

    if (!event) {
      this.selectphanhe = [];
      return;
    }
    this.doSearch();
  }

  public ChangeSearch(searchValue: string): void {
    console.log(searchValue);
    if (!searchValue) {
      this.doSearch();
    }
  }

  public doSearch() {
    this.ismess = "";
    this.currentPage = 1;

    if (!this.dataCsCase_Goc || this.dataCsCase_Goc.length <= 0) {
      this.ismess = "Không tìm thấy dữ liệu";
      return;
    }

    if (
      (this.selectmatruong && this.selectmatruong.length > 0) ||
      (this.selectrangthai && this.selectrangthai.length > 0) ||
      (this.selectloaicase && this.selectloaicase.length > 0) ||
      (this.selectphanhe && this.selectphanhe.length > 0) ||
      this.searchText.length > 0
    ) {
      this.dataCsCase_Search = this.dataCsCase_Goc.filter(
        (s) =>
          s.matruong &&
          (this.selectmatruong && this.selectmatruong.length > 0
            ? this.selectmatruong.find((x) => x === s.matruong)
            : true) &&
          (this.selectrangthai && this.selectrangthai.length > 0
            ? this.selectrangthai.find((x) => x === s.trangthai)
            : true) &&
          (this.selectloaicase && this.selectloaicase.length > 0
            ? this.selectloaicase.find((x) => x === s.loaicase)
            : true) &&
          (this.selectphanhe && this.selectphanhe.length > 0
            ? this.selectphanhe.find((x) => x === s.phanhe)
            : true) &&
          (this.searchText && this.searchText.length > 0
            ? s.chitietyc
              .toLowerCase()
              .includes(this.searchText.toLowerCase()) ||
            s.macase.toLowerCase().includes(this.searchText.toLowerCase())
            : true)
      );

      if (this.dataCsCase_Search && this.dataCsCase_Search.length > 0) {
        this.dataCsCase_Temp = this.dataCsCase_Search;
        this.dataCsCase_Filter = this.dataCsCase_Search
          .slice((this.currentPage - 1) * this.pageLimit)
          .filter((_u, i) => i < this.pageLimit);

        this.total = this.dataCsCase_Search.length ?? 0;
        this.pages = Math.ceil(
          (this.total + this.pageLimit - 1) / this.pageLimit
        );
      } else {
        this.dataCsCase_Filter =
          this.dataCsCase_Search =
          this.dataCsCase_Temp =
          [];
        this.ismess = "Không tìm thấy dữ liệu";
        this.total = 0;
        this.pages = 0;
      }
    } else {
      //  this.listloaicase = ''; this.listmatruong = ''; this.listphanhe = ''; this.liststate = ''; this.searchText = '';
      this.selectrangthai = [];
      this.selectmatruong = [];
      this.selectloaicase = [];
      this.selectphanhe = [];
      this.searchText = "";
      this.dataCsCase_Search = null;
      this.dataCsCase_Temp = this.dataCsCase_Goc;
      this.dataCsCase_Filter = this.dataCsCase_Goc
        .slice((this.currentPage - 1) * this.pageLimit)
        .filter((_u, i) => i < this.pageLimit);
      this.total = this.dataCsCase_Goc.length ?? 0;
      this.pages = Math.ceil(
        (this.total + this.pageLimit - 1) / this.pageLimit
      );
    }

    // if ((this.selectmatruong && this.selectmatruong.length > 0)
    //   || (this.selectrangthai && this.selectrangthai.length > 0)
    //   || (this.selectloaicase && this.selectloaicase.length > 0)
    //   || (this.selectphanhe && this.selectphanhe.length > 0)
    //   || (this.searchText && this.searchText.length > 0)) {

    //   // neu su dung thì chuyển .some thành this.listmatruong.includes(s.matruong)
    //   // this.listmatruong = this.selectmatruong ? this.selectmatruong.join(',') : '';
    //   // this.liststate = this.selectrangthai ? this.selectrangthai.join(',') : '';
    //   this.dataCsCase_Search = this.dataCsCase_Goc.filter(s => s.matruong
    //     && ((this.listmatruong && this.listmatruong.length > 0) ? this.listmatruong.some(x => x.matruong === s.matruong) : true)
    //     && ((this.liststate && this.liststate.length > 0) ? this.liststate.some(x => x.trangthai === s.trangthai) : true)
    //     && ((this.listloaicase && this.listloaicase.length > 0) ? this.listloaicase.some(x => x.loaicase === s.loaicase) : true)
    //     && ((this.listphanhe && this.listphanhe.length > 0) ? this.listphanhe.some(x => x.phanhe === s.phanhe) : true)
    //     && ((this.searchText && this.searchText.length > 0) ?
    //       (s.chitietyc.toLowerCase().includes(this.searchText.toLowerCase())
    //         || s.macase.toLowerCase().includes(this.searchText.toLowerCase())
    //       ) : true
    //     )
    //   );

    //   if (this.dataCsCase_Search && this.dataCsCase_Search.length > 0) {
    //     this.dataCsCase_Temp = this.dataCsCase_Search;
    //     this.dataCsCase_Filter = this.dataCsCase_Search
    //       .slice((this.currentPage - 1) * this.pageLimit)
    //       .filter((_u, i) => i < this.pageLimit);

    //     this.total = this.dataCsCase_Search.length ?? 0;
    //     this.pages = Math.ceil((this.total + this.pageLimit - 1) / this.pageLimit);
    //   }
    //   else {
    //     this.dataCsCase_Filter = this.dataCsCase_Search = this.dataCsCase_Temp = [];
    //     this.ismess = 'Không tìm thấy dữ liệu';
    //     this.total = 0;
    //     this.pages = 0;
    //   }
    // }
    // else // clear all
    // {
    //   this.searchText = ''; this.listloaicase = ''; this.listmatruong = ''; this.listphanhe = ''; this.liststate = '';
    //   this.dataCsCase_Search = null;
    //   this.dataCsCase_Temp = this.dataCsCase_Goc;
    //   this.dataCsCase_Filter = this.dataCsCase_Goc.slice((this.currentPage - 1) * this.pageLimit)
    //     .filter((_u, i) => i < this.pageLimit);
    //   this.total = this.dataCsCase_Goc.length ?? 0;
    //   this.pages = Math.ceil((this.total + this.pageLimit - 1) / this.pageLimit);
    // }
  }

  // page change
  public pageChanged(event: any): void {
    this.currentPage = event.page;
    if (this.dataCsCase_Search && this.dataCsCase_Search.length > 0) {
      this.total = this.dataCsCase_Search.length ?? 0;
      this.pages = Math.ceil(
        (this.total + this.pageLimit - 1) / this.pageLimit
      );
      this.dataCsCase_Filter = this.dataCsCase_Search
        .slice((this.currentPage - 1) * this.pageLimit)
        .filter((_u, i) => i < this.pageLimit);
    } else {
      this.total = this.dataCsCase_Goc.length ?? 0;
      this.pages = Math.ceil(
        (this.total + this.pageLimit - 1) / this.pageLimit
      );
      this.dataCsCase_Filter = this.dataCsCase_Goc
        .slice((this.currentPage - 1) * this.pageLimit)
        .filter((_u, i) => i < this.pageLimit);
    }
    window.scroll({ top: 230, left: 0, behavior: "smooth" });
  }

  public async setOrder(value: string) {
    if (this.dataCsCase_Temp) {
      if (this.orderby === value) {
        this.reverseOrderby = !this.reverseOrderby;
      }
      this.orderby = value;

      if (value === "macase") {
        if (this.reverseOrderby) {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            Number(a.macase) < Number(b.macase)
              ? -1
              : Number(a.macase) > Number(b.macase)
                ? 1
                : 0
          );
        } else {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            Number(a.macase) < Number(b.macase)
              ? 1
              : Number(a.macase) > Number(b.macase)
                ? -1
                : 0
          );
        }
      }

      if (value === "matruong") {
        if (this.reverseOrderby) {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.matruong < b.matruong ? -1 : a.matruong > b.matruong ? 1 : 0
          );
        } else {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.matruong < b.matruong ? 1 : a.matruong > b.matruong ? -1 : 0
          );
        }
      }

      if (value === "ngaynhan") {
        if (this.reverseOrderby) {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            new Date(a.ngaynhan).getTime() > new Date(b.ngaynhan).getTime()
              ? 1
              : -1
          );
        } else {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            new Date(a.ngaynhan).getTime() > new Date(b.ngaynhan).getTime()
              ? -1
              : 1
          );
        }
      }

      if (value === "trangthai") {
        if (this.reverseOrderby) {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.trangthai < b.trangthai ? -1 : a.trangthai > b.trangthai ? 1 : 0
          );
        } else {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.trangthai < b.trangthai ? 1 : a.trangthai > b.trangthai ? -1 : 0
          );
        }
      }

      if (value === "loaihopdong") {
        if (this.reverseOrderby) {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.loaihopdong < b.loaihopdong
              ? -1
              : a.loaihopdong > b.loaihopdong
                ? 1
                : 0
          );
        } else {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.loaihopdong < b.loaihopdong
              ? 1
              : a.loaihopdong > b.loaihopdong
                ? -1
                : 0
          );
        }
      }
      if (value === "mucdo") {
        if (this.reverseOrderby) {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.mucdo < b.mucdo ? -1 : a.mucdo > b.mucdo ? 1 : 0
          );
        } else {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.mucdo < b.mucdo ? 1 : a.mucdo > b.mucdo ? -1 : 0
          );
        }
      }

      if (value === "hieuluc") {
        if (this.reverseOrderby) {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.hieuluc < b.hieuluc ? -1 : a.hieuluc > b.hieuluc ? 1 : 0
          );
        } else {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.hieuluc < b.hieuluc ? 1 : a.hieuluc > b.hieuluc ? -1 : 0
          );
        }
      }

      if (value === "dabangiao") {
        if (this.reverseOrderby) {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.dabangiao < b.dabangiao ? -1 : a.dabangiao > b.dabangiao ? 1 : 0
          );
        } else {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.dabangiao < b.dabangiao ? 1 : a.dabangiao > b.dabangiao ? -1 : 0
          );
        }
      }

      if (value === "ngayemail") {
        if (this.reverseOrderby) {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            new Date(a.ngayemail).getTime() > new Date(b.ngayemail).getTime()
              ? 1
              : -1
          );
        } else {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            new Date(a.ngayemail).getTime() > new Date(b.ngayemail).getTime()
              ? -1
              : 1
          );
        }
      }

      if (value === "loaicase") {
        if (this.reverseOrderby) {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.loaicase.substring(0, 3) < b.loaicase.substring(0, 3)
              ? -1
              : a.loaicase.substring(0, 3) > b.loaicase.substring(0, 3)
                ? 1
                : 0
          );
        } else {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.loaicase.substring(0, 3) < b.loaicase.substring(0, 3)
              ? 1
              : a.loaicase.substring(0, 3) > b.loaicase.substring(0, 3)
                ? -1
                : 0
          );
        }
      }

      if (value === "phanhe") {
        if (this.reverseOrderby) {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.phanhe < b.phanhe ? -1 : a.phanhe > b.phanhe ? 1 : 0
          );
        } else {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.phanhe < b.phanhe ? 1 : a.phanhe > b.phanhe ? -1 : 0
          );
        }
      }
      if (value === "ngaydukien") {
        if (this.reverseOrderby) {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.phanhe < b.phanhe ? -1 : a.phanhe > b.phanhe ? 1 : 0
          );
        } else {
          this.dataCsCase_Temp = this.dataCsCase_Temp.sort((a, b) =>
            a.phanhe < b.phanhe ? 1 : a.phanhe > b.phanhe ? -1 : 0
          );
        }
      }

      this.dataCsCase_Filter = this.dataCsCase_Temp
        .slice((this.currentPage - 1) * this.pageLimit)
        .filter((_u, i) => i < this.pageLimit);
      this.total = this.dataCsCase_Temp.length ?? 0;
      this.pages = Math.ceil(
        (this.total + this.pageLimit - 1) / this.pageLimit
      );
    }
  }

  // only Admin
  public exportexcel() {
    if ((this.isadmin || this.isadministrator) && this.dataCsCase_Filter) {
      const dataCsCase_temp = this.dataCsCase_Filter.map((v) => ({
        "Mã Case": v.macase,
        "Mã trường": v.matruong,
        "Ngày nhận yêu cầu": v.ngaynhan ? new Date(v.ngaynhan) : "",
        "Yêu cầu": v.chitietyc,
        "Trạng thái": v.trangthai,
        "Loại Case": v.loaicase,
        "Phân hệ": v.phanhe,
        "Ngày d.kiến b.giao": v.ngaydukien ? new Date(v.ngaydukien) : "",
        "Loại hợp đồng": v.loaihopdong,
        "Mức độ": v.mucdo,
        "Hiệu lực Release": v.hieuluc,
        "Đã bàn giao": v.dabangiao,
        "Ngày gửi mail": v.ngayemail ? new Date(v.ngayemail) : "",
        Mail: v.mailto.split(";").join(";<br>"),
      }));

      const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(dataCsCase_temp, {
        dateNF: "dd/MM/yyyy",
      });
      const wb: XLSX.WorkBook = XLSX.utils.book_new();
      XLSX.utils.book_append_sheet(wb, ws, "Sheet1");
      XLSX.writeFile(wb, "CSCASE.xlsx");
    }
  }

  // only Admin
  public checkState() {
    if (this.isadmin || this.isadministrator) {
      let initialState = {};
      if (this.IsTFS) {
        initialState = {
          icon: "n",
          button: "OK",
          message: "Đang lấy dữ liệu từ TFS (ONLINE)",
        };
      } else {
        initialState = {
          icon: "e",
          button: "OK",
          message: "Đang lấy dữ liệu từ File Excel (OFFLINE)",
        };
      }
      this.modalService.show(MessBoxComponent, {
        initialState,
        ignoreBackdropClick: true,
        animated: false,
        class: "modal-dialog-centered",
      });
    }
  }

  public customSearchFn(term: string, item: DataTruong) {
    term = term.toLowerCase();
    return (
      item.matruong.toLowerCase().indexOf(term) > -1 ||
      item.tentruong.toLowerCase().indexOf(term) > -1 ||
      item.tentruong.toLowerCase() === term
    );
  }

  public Print() {
    this.printerService.printDiv("printArea");
  }

  public xemcase(matruong: string) {
    const data = this.dataCsCase_Goc.filter((s) => s.matruong === matruong);
    const initialState = { data: data };
    this.modalService.show(ChitietcaseComponent, {
      initialState,
      ignoreBackdropClick: true,
      animated: false,
      class: "modal-dialog-centered modal-xl",
    });
  }

  public HideMail() {
    this.isHideMail = !this.isHideMail;
  }

  public hoverTooltip(hover: number) {
    if (!!hover) {
      if (hover === 1) {
        this.popupTooltip = "Click xem chi tiết";
      }
      if (hover === 2) {
        this.popupTooltip = "Click để trao đổi";
      }
      if (hover === 3) {
        this.popupTooltip = "Click xem đáp ứng Cty";
      }
    } else {
      this.popupTooltip = "";
      hover = null;
    }
  }
  public trackByFn(index: number) {
    return index;
  }
}
