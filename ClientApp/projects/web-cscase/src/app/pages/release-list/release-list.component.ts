import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { NgxSpinnerService } from "ngx-spinner";
import { ToastrService } from "ngx-toastr";
import { MessBoxComponent } from "../../service/mess-box/mess-box.component";
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
  public values = {
    loaicase: '', // Loại case
    matruong: '', // Khách hàng (Mã trường)
    phanhe: '', // Phân hệ
    whatnew: ''  // Nội dung xử lý
  };
  public filteredData = [];

  constructor(
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
          // if (this.group && this.group.length > 0) {
          //   this.dataCsCase_Release_Cache = this.group[0].ds_rl;
          // };
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
  private compareVersions(a, b) {
    // Tách chuỗi version thành các phần
    let partsA = a.version.split('.');
    let partsB = b.version.split('.');

    for (let i = 0; i < partsA.length; i++) {
      if (partsA[i] !== partsB[i]) {
        // So sánh số nếu có thể, nếu không thì so sánh như chuỗi
        return isNaN(partsA[i]) || isNaN(partsB[i]) ? partsA[i].localeCompare(partsB[i]) : partsA[i] - partsB[i];
      }
    }

    return 0; // Trả về 0 nếu tất cả các phần đều giống nhau
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
    let keySort = this.Object.keys(uniqueVersions).sort((a, b) => {
      const partsA = a.split('.');
      const partsB = b.split('.');

      // So sánh năm
      const yearComparison = parseInt(partsB[0]) - parseInt(partsA[0]);
      if (yearComparison !== 0) return yearComparison;

      // So sánh chữ cái
      const letterComparison = partsB[1].replace(/[0-9]/g, '').localeCompare(partsA[1].replace(/[0-9]/g, ''));
      if (letterComparison !== 0) return letterComparison;

      // So sánh phần cuối cùng
      return parseInt(partsB[2]) - parseInt(partsA[2]);
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
