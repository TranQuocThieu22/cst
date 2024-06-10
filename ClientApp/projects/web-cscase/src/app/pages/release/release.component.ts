import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { EduCase } from "@mylibs";
import { BsModalRef, BsModalService } from "ngx-bootstrap/modal";
import { NgxSpinnerService } from "ngx-spinner";

@Component({
  selector: "app-release",
  templateUrl: "./release.component.html",
  styleUrls: ["./release.component.scss"],
})
export class ReleaseComponent implements OnInit {
  public myGroup: FormGroup;
  get f() {
    return this.myGroup.controls;
  }
  public submitted = false;
  public messError = "";
  public messSuccess = "";
  public dataCsCase_Release: EduCase[] = [];
  public countGroup: any;
  public indexGroup: any;

  constructor(
    private formBuilder: FormBuilder,
    private http: HttpClient,
    private modalService: BsModalService,
    public bsModalRefEditor: BsModalRef,
    private spinner: NgxSpinnerService
  ) {
    sessionStorage.setItem("s", "1");
  }

  public ngOnInit(): void {
    this.myGroup = this.formBuilder.group({
      version: ["", Validators.required],
      version_rl: ["", Validators.required],
    });
  }

  public Release() {
    this.messError = this.messSuccess = "";

    if (!this.ValidateFromInput()) {
      this.spinner.hide("spinner_rl");
      return;
    }

    this.spinner.show("spinner_rl");

    const body = {
      filter: {
        version: this.f.version.value,
      },
    };

    this.http.post<any>("/api/main/cscase_rl", body).subscribe(
      (res: any) => {
        if (res && res.code === 200) {
          if (res.data && res.data.data_case) {
            const data_ = res.data.data_case.filter(
              (s) => s.whatnew && s.whatnew.length > 5
            ); // loai bỏ noi dung ko hop le
            const tpm = data_.sort((a, b) => {
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

            tpm.forEach((e) => {
              if (e.loaicase.includes("EX -")) {
                e.groupnum = 1;
              } else if (e.loaicase.includes("BF -")) {
                e.groupnum = 2;
              } else {
                e.groupnum = 3;
              }
            });

            const tpm_ = tpm.sort((a, b) => {
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
          this.spinner.hide("spinner_rl");
        } else {
          this.messError = res?.message
            ? res.message +
            "<br> Phiên làm việc đã hết hạn, Vui lòng đăng nhập lại"
            : "Phiên làm việc đã hết hạn, Vui lòng đăng nhập lại";
          this.spinner.hide("spinner_rl");
        }
      },
      (error) => {
        this.spinner.hide("spinner_rl");
        this.messError =
          "Không tìm thấy dữ liệu. <br> Phiên làm việc đã hết hạn, Vui lòng đăng nhập lại";
      }
    );
  }

  private ValidateFromInput(): boolean {
    this.submitted = true;
    if (this.myGroup.invalid) {
      return false;
    } else {
      return true;
    }
  }

  public CreateRelease() {
    this.spinner.show("spinner_rl");

    let dsrl = [];

    this.dataCsCase_Release.forEach((s) =>
      dsrl.push({
        macase: s.macase,
        vesion: this.f.version.value,
        loaicase: s.loaicase,
        matruong: s.matruong,
        phanhe: s.phanhe,
        chitietyc: s.chitietyc,
        // ngaydukien: s.ngaydukien,
        whatnew: s.whatnew,
        reviewcase: s.reviewcase
      })
    );


    const body = {
      filter: {
        dsrl: dsrl,
        version: this.f.version.value,
        version_rl: this.f.version_rl.value
      },
    };

    const bodyDel = { filter: { version: null, vesion: this.f.version.value } };
    // Delete old rl
    this.http.post<any>("/api/main/delete_case_rl", bodyDel).subscribe(
      (res: any) => { },
      (error) => { }
    );

    this.http.post<any>("/api/main/createRelease", body).subscribe(
      (res: any) => {
        this.spinner.hide("spinner_rl");
        this.messSuccess = "Đã tạo xong Version Release";
      },
      (error) => {
        this.spinner.hide("spinner_rl");
        this.messError =
          "Không tìm thấy dữ liệu. <br> Phiên làm việc đã hết hạn, Vui lòng đăng nhập lại";
      }
    );
  }

  public trackByFn(index: number) {
    return index;
  }
}
