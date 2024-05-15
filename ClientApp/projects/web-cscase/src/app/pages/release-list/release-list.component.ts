import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { NgxSpinnerService } from "ngx-spinner";
import { ToastrService } from "ngx-toastr";
import { MessBoxComponent } from "../../service/mess-box/mess-box.component";
import { NoidungcscaseComponent } from "../noidungcscase/noidungcscase.component";
import { EditRlcaseComponent } from "./edit-rlcase/edit-rlcase.component";

@Component({
  selector: "app-release-list",
  templateUrl: "./release-list.component.html",
  styleUrls: ["./release-list.component.scss"],
})
export class ReleaseListComponent implements OnInit {
  public group: any;
  public Object = Object;
  public hide = [];
  public hideChucNangMoi = []
  public hideChinhSuaChoTungDonVi = []
  public countGroup: any;
  public indexGroup: any;
  public messError: string = "";
  public dataCsCase_Release: any;
  public listChucNangMoi = []
  private RL_Full: any;
  public index_old: number = 0;
  public currentUser: any;
  public isadmin: boolean = false;
  public isshow: boolean = false;
  public dataCsCase_Release_Cache: any;
  public ismess = "";
  public values = {
    loaicase: '', // Loại case
    matruong: '', // Khách hàng (Mã trường)
    phanhe: '', // Phân hệ
    whatnew: ''  // Nội dung xử lý
  };
  public filteredData = [];

  constructor(
    private router: Router,
    private http: HttpClient,
    private spinner: NgxSpinnerService,
    public bsModalRef: BsModalRef,
    private modalService: BsModalService,
    private toastr: ToastrService
  ) { }

  public ngOnInit(): void {
    if (sessionStorage.getItem("current-user")) {
      this.currentUser = JSON.parse(sessionStorage.getItem("current-user"));
      if (this.currentUser.roles.toLowerCase() === "admin") {
        this.isadmin = true;
      }
    }

    this.spinner.show("spinner_rlv");
    this.http.post<any>("/api/main/ds_case_rl", "").subscribe(
      (res: any) => {
        if (res && res.length > 0) {
          this.RL_Full = res.sort((a, b) => (a.version > b.version ? -1 : 1));
          this.group = this.groupArry(this.RL_Full);
          this.dataCsCase_Release_Cache = this.RL_Full
        }
        this.spinner.hide("spinner_rlv");
      },
      (error) => {
        this.spinner.hide("spinner_rlv");
        this.messError =
          "Không tìm thấy dữ liệu. <br> Phiên làm việc đã hết hạn, Vui lòng đăng nhập lại";
      }
    );
  }

  public selectNgay(version: string) {
    // this.listChucNangMoi = [] Fix list chức năng không đi theo phiên bản
    const rl_version = this.RL_Full.filter((s) => s.vesion === version);
    const tpm = rl_version.sort((a, b) => {
      if (a.loaicase > b.loaicase) {
        return 1;
      } else if (a.loaicase < b.loaicase) {
        return -1;
      } else if (a.macase > b.macase) {
        return 1;
      } else if (a.macase < b.macase) {
        return -1;
      }
      return 0;
    });
    this.group_filter(tpm);
  }

  private group_filter(data: any) {
    data.forEach((e) => {
      if (e.loaicase.includes("EX -")) {
        e.groupnum = 1;
      } else if (e.loaicase.includes("BF -")) {
        e.groupnum = 2;
      } else if (e.loaicase.includes("QA -")) {
        e.groupnum = 4;
      } else {
        e.groupnum = 3;
      }
    });
    data.forEach(e => {
      if (e.reviewcase == 'NEW') {
        if (!this.listChucNangMoi.includes(e)) this.listChucNangMoi.push(e)
      }
    });
    console.log(this.listChucNangMoi);


    this.listChucNangMoi.sort((a, b) => {
      if (a.phanhe < b.phanhe) {
        return -1;
      }
      if (a.phanhe > b.phanhe) {
        return 1;
      }
      return 0;
    });

    const tpm_ = data.sort((a, b) => {
      if (a.groupnum > b.groupnum) {
        return 1;
      } else if (a.groupnum < b.groupnum) {
        return -1;
      }
      return 0;
    });

    this.dataCsCase_Release = tpm_;


    this.countGroup = this.dataCsCase_Release.reduce((a, b) => {
      a[b.loaicase] = (a[b.loaicase] || 0) + 1;
      return a;
    }, {});
    this.indexGroup = this.dataCsCase_Release.reduce((a, b, i) => {
      a[b.loaicase] = a[b.loaicase] === undefined ? i : a[b.loaicase];
      return a;
    }, {});
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
          this.Disconnect(true);
        }
      });
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
  public Sua(gr) {
    if (this.isadmin) {
      const initialState = { version: gr.version };
      this.bsModalRef = this.modalService.show(EditRlcaseComponent, {
        initialState,
        backdrop: "static",
        class: "modal-dialog-centered",
        ignoreBackdropClick: true,
        animated: false,
      });

      this.bsModalRef.content.dtOut.subscribe((res) => {
        if (res.res === 200 && res.data) {
          this.RL_Full = res.data.forEach((s) => {
            const dateParts4 = s.version.split("/");
            s.version_ = new Date(
              +dateParts4[2],
              +dateParts4[1] - 1,
              +dateParts4[0]
            );
          });

          this.RL_Full = res.data.sort((a, b) =>
            a.version_ > b.version_ ? -1 : 1
          );
          this.group = this.groupArry(this.RL_Full);
        }
      });
    }
  }

  public Xoa(gr) {
    if (this.isadmin) {
      const initialState = {
        icon: "q",
        message: "Xác nhận xóa dữ liệu?",
        onConfirm: () => {
          const body = { filter: { version: gr.version, vesion: gr.vesion } };

          this.spinner.show("spinner_rlv");

          this.http.post<any>("/api/main/delete_case_rl", body).subscribe(
            (res: any) => {
              if (res) {
                this.RL_Full = res.forEach((s) => {
                  s.version = gr.version;
                });

                this.RL_Full = res.sort((a, b) =>
                  a.version > b.version ? -1 : 1
                );
                this.group = this.groupArry(this.RL_Full);
                this.spinner.hide("spinner_rlv");
                this.toastr.success("Dữ liệu đã được xóa thành công");
              } else {
                this.spinner.hide("spinner_rlv");
                this.toastr.error("Dữ liệu không thể xóa");
              }
            },
            (error) => {
              this.spinner.hide("spinner_rlv");
              this.toastr.error("Dữ liệu không thể xóa");
            }
          );
        },
      };
      this.modalService.show(MessBoxComponent, {
        initialState,
        backdrop: "static",
        keyboard: false,
        ignoreBackdropClick: true,
        animated: false,
        class: "modal-dialog-centered",
      });
    }
  }

  private groupArry(arr: any) {
    const uniqueVersions = [];
    arr.forEach(obj => {
      const { vesion, version_rl } = obj;
      const key = `${vesion}`;

      // Kiểm tra xem phiên bản đã tồn tại trong đối tượng chưa
      if (!uniqueVersions[key]) {
        uniqueVersions[key] = { vesion, version_rl };
      }
    });
    let keySort = this.Object.keys(uniqueVersions).sort((b, a) => {
      const aParts = a.split('.');
      const bParts = b.split('.');

      // So sánh năm
      const aYear = parseInt(aParts[0]);
      const bYear = parseInt(bParts[0]);
      if (aYear !== bYear) {
        return aYear - bYear;
      }

      // So sánh chữ cái
      const aChar = aParts[1];
      const bChar = bParts[1];
      if (aChar !== bChar) {
        return aChar.localeCompare(bChar);
      }

      // So sánh số chữ cái
      const aNumChars = parseInt(aParts[2]);
      const bNumChars = parseInt(bParts[2]);
      if (aNumChars !== bNumChars) {
        return aNumChars - bNumChars;
      }

      // So sánh number
      const aNumber = parseInt(aParts[3]);
      const bNumber = parseInt(bParts[3]);
      return aNumber - bNumber;
    });

    let newArr = []
    keySort.forEach(element => {
      newArr[element] = { vesion: element, version_rl: uniqueVersions[element].version_rl }
    });

    return Object.values(newArr);
  }



  public showDataFormDg(index: number) {
    this.hide[index] = !this.hide[index];

    if (index != this.index_old) {
      this.hide[this.index_old] = false;
    }
    this.index_old = index;
  }

  public showChucNangMoi(index: number) {
    this.hideChucNangMoi[index] = !this.hideChucNangMoi[index];

    if (index != this.index_old) {
      this.hideChucNangMoi[this.index_old] = false;
    }
    this.index_old = index;
  }
  public showChinhSuaChoTungDonVi(index: number) {
    this.hideChinhSuaChoTungDonVi[index] = !this.hideChinhSuaChoTungDonVi[index];

    if (index != this.index_old) {
      this.hideChinhSuaChoTungDonVi[this.index_old] = false;
    }
    this.index_old = index;
  }

  public trackByFn(index: number) {
    return index;
  }

  public onSearchChange(event, version, colName) {
    // Update input value when change
    this.values[`${colName}`] = event.target.value;
    // Check  change value will back data default (Get from cache)
    this.selectNgay(version)
    // Filter data
    const dataFilter = this.dataCsCase_Release.filter((c) =>
      (!this.values.loaicase || c.loaicase.toLowerCase().includes(this.values.loaicase.toLowerCase())) &&
      (!this.values.matruong || c.matruong.toLowerCase().includes(this.values.matruong.toLowerCase())) &&
      (!this.values.phanhe || c.phanhe.toLowerCase().includes(this.values.phanhe.toLowerCase())) &&
      (!this.values.whatnew || c.whatnew.toLowerCase().includes(this.values.whatnew.toLowerCase()))
    );
    this.group_filter(dataFilter);
  }
  public clearInputValues() {
    this.values = {
      loaicase: '',
      matruong: '',
      phanhe: '',
      whatnew: ''
    };
  }
}
