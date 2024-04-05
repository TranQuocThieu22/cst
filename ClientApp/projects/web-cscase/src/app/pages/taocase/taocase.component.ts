import { HttpClient } from '@angular/common/http';
import { ChangeDetectorRef, Component, ElementRef, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CKEditorComponent } from '@ckeditor/ckeditor5-angular/ckeditor.component';
import { DataPhanHe, DataTruong, User } from '@mylibs';
import { BsModalService } from 'ngx-bootstrap/modal';
import { NgxSpinnerService } from 'ngx-spinner';
import { ToastrService } from 'ngx-toastr';
import * as InlineEditor from '../../../assets/ckeditor5/ckeditor.js';
import '../../../assets/ckeditor5/translations/vi';
import { MessBoxComponent } from '../../service/mess-box/mess-box.component';
import { CkeditorServer } from '../../shared/ckEditorLinkServer.module';


interface FileType { ext: string; type: string; }
@Component({
  selector: 'app-taocase',
  templateUrl: './taocase.component.html',
  styleUrls: ['./taocase.component.scss']
})

export class TaocaseComponent {
  public FormPhanAnh: FormGroup; get f() { return this.FormPhanAnh.controls; }
  public FromHieuChinhTuVan: FormGroup; get f1() { return this.FromHieuChinhTuVan.controls; }
  @ViewChild('ckEditor') editorComponent: CKEditorComponent; public Editor = InlineEditor; public config = {};
  @ViewChild('input_hinhanh') public input_hinhanh: ElementRef; public namnefile = ''
  @ViewChild('input_hinhanh_hc') public input_hinhanh_hc: ElementRef;
  @ViewChild('myInput1') public myInput1: ElementRef;
  @ViewChild('myInput2') public myInput2: ElementRef;

  public uploadForm1: FormGroup; public formData1 = new FormData(); public FileName_dinhkem1: string = '';
  public uploadForm2: FormGroup; public formData2 = new FormData(); public FileName_dinhkem2: string = '';

  public cbphanhe: DataPhanHe[]; public cbMaTruong: DataTruong[]; public selectmatruong = []; public disabled = false;
  public messError: string = ''; public submitted: boolean = false; public submitted1: boolean = false;
  public ValidateDate: boolean; public currentUser: User; private MaKhachHang: string;
  public title: string = ''; public base64: string = ''; public ma_case: number;
  public selectedFiles: FileList; public type_chucnang: number;
  public filetype: FileType[]; private filetype_tmp_full: any[];
  private string_ha = '';

  constructor(private http: HttpClient, private spinner: NgxSpinnerService, private toastr: ToastrService,
    private formBuilder: FormBuilder, private router: Router, private route: ActivatedRoute, public addLinkServer: CkeditorServer,
    private modalService: BsModalService, private changeDetector: ChangeDetectorRef) {
    this.config = { language: 'vi', placeholder: 'Copy hình ảnh vào đây' };
    this.uploadForm1 = this.formBuilder.group({ fileupload1: [''] });
    this.uploadForm2 = this.formBuilder.group({ fileupload2: [''] });

    this.filetype_tmp_full = [
      { ext: '.avi', type: 'video/x-msvideo' },
      { ext: '.mpeg', type: 'video/mpeg' },
      { ext: '.bmp', type: 'image/bmp' },
      { ext: '.csv', type: 'text/csv' },
      { ext: '.doc', type: 'application/msword' },
      { ext: '.docx', type: 'application/vnd.openxmlformats-officedocument.wordprocessingml.document' },
      { ext: '.jpeg', type: 'image/jpeg' },
      { ext: '.jpg', type: 'image/jpeg' },
      { ext: '.png', type: 'image/png' },
      { ext: '.pdf', type: 'application/pdf' },
      { ext: '.ppt', type: 'application/vnd.ms-powerpoint' },
      { ext: '.pptx', type: 'application/vnd.openxmlformats-officedocument.presentationml.presentation' },
      { ext: '.txt', type: 'text/plain' },
      { ext: '.xls', type: 'application/vnd.ms-excel' },
      { ext: '.xlsx', type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' }
    ]
    this.filetype = this.filetype_tmp_full.map(s => s.ext);
  }

  private declareForm_PhanAnh() {
    this.FormPhanAnh = this.formBuilder.group({
      tieu_de: ['', Validators.required],
      mo_ta: ['', Validators.required],
      menu_thuc_hien: ['', Validators.required],
      nut_gay_loi: [''],
      nhiem_y: [''],
      hinh_anh: ['', Validators.required],
      thao_tac: [''],
      ket_qua_dung: [''],
      file_dinh_kem: [''],
      noi_dung_khac: [''],
      nguoi_yeu_cau_hoten: [''],
      nguoi_yeu_cau_chucvu: [''],
      nguoi_yeu_cau_phongban: [''],
      nguoi_yeu_cau_dienthoai: [''],
      nguoi_yeu_cau_email: [''],
    });
  }

  private declareForm_HieuChinh() {
    this.FromHieuChinhTuVan = this.formBuilder.group({
      tieu_de_hc: ['', Validators.required],
      muc_dich: ['', Validators.required],
      noi_dung_can_hc: ['', Validators.required],
      vi_du_thuc_te: [''],
      menu_thuc_hien_hc: ['', Validators.required],
      nut_thuc_hien: [''],
      nhiem_y_thuc_hien: [''],
      hinh_anh_hc: ['', Validators.required],
      thao_tac_hc: [''],
      file_dinh_kem_hc: [''],
      noi_dung_khac_hc: [''],
      nguoi_yeu_cau_hc_hoten: [''],
      nguoi_yeu_cau_hc_chucvu: [''],
      nguoi_yeu_cau_hc_phongban: [''],
      nguoi_yeu_cau_hc_dienthoai: [''],
      nguoi_yeu_cau_hc_email: [''],
    });
  }

  public ngOnInit() {

    this.submitted = this.submitted1 = false;
    if (sessionStorage.getItem('current-user')) {
      this.currentUser = JSON.parse(sessionStorage.getItem('current-user'));
      if (this.currentUser.roles.toLowerCase() === 'admin' || this.currentUser.roles.toLowerCase() === 'administrator') {
        this.MaKhachHang = "AQ";
      }
      else {
        this.MaKhachHang = this.currentUser.maTruong;
      }
    }

    this.route.params.subscribe(params => {
      if (params && params.id === '0') {
        this.title = 'Gửi phản ánh lỗi';
        this.type_chucnang = 0;
        this.declareForm_PhanAnh();
        this.FormPhanAnh.reset();
      }
      if (params && params.id === '1') {
        this.title = 'Gửi yêu cầu hiệu chỉnh / Tư vấn';
        this.type_chucnang = 1;
        this.declareForm_HieuChinh();
        this.FromHieuChinhTuVan.reset();
      }
    });

    this.spinner.hide('spinner_tc');
  }

  private Tao_Case() {


    // this.spinner.show('spinner_tc');

    if ((this.type_chucnang === 0 && this.f.hinh_anh.value) || (this.type_chucnang === 1 && this.f1.hinh_anh_hc.value)) {
      let filename_hinhanh = [];
      this.string_ha = `<span style="font-weight:bold;text-decoration-line:underline;"></br>Hình ảnh:</span></br>`;
      let Value_Str_Hinh;
      if (this.type_chucnang === 1) Value_Str_Hinh = this.f1.hinh_anh_hc.value
      else Value_Str_Hinh = this.f.hinh_anh.value
      //  = this.f.hinh_anh.value ? this.f.hinh_anh.value : this.f1.hinh_anh_hc.value;
      const baseUrl = new URL(window.location.href).origin + new URL(window.location.href)?.pathname;
      let tmp_html = this.stringToHTML(Value_Str_Hinh);
      let tpm1 = tmp_html.getElementsByTagName("figure");

      for (let i = 0; i < tpm1.length; i++) {
        let random_filename = Date.now() + Math.random().toString(36).substr(2, 9) + '.png';
        let base64_ = tpm1[i].getElementsByTagName("img")[0].src;
        if (base64_ && base64_.length > 0) {
          filename_hinhanh.push({ filename: random_filename });

          // add hinh lên server
          const body = { filter: { filename: random_filename, base64: base64_ } }
          this.http.post('/api/main/upload_dinhkemfile', body)
            .subscribe((res1_: any) => { }, (error) => { console.log('1: ' + error); });

          tpm1[i].getElementsByTagName("img")[0].src = baseUrl + 'attachment/' + random_filename;
          tpm1[i].setAttribute('width', '1000px')
        }
      }

      this.string_ha = this.string_ha + tmp_html.outerHTML;
    }

    let tmp = this.getdulieutrenform();

    const body = [
      { "op": "add", "path": "/fields/AQ.EstimateTime", "from": null, "value": "1.0" },
      { "op": "add", "path": "/fields/AQ.Priority", "from": null, "value": "1 - Cần phân tích" },
      { "op": "add", "path": "/fields/AQ.Module", "from": null, "value": "PORTAL" },
      { "op": "add", "path": "/fields/AQ.MailTo", "from": null, "value": this.f.nguoi_yeu_cau_email.value ? this.f.nguoi_yeu_cau_email.value : this.f.nguoi_yeu_cau_hc_email.value },
      { "op": "add", "path": "/fields/AQ.CustomerTitle", "from": null, "value": "anh / chị" },
      { "op": "add", "path": "/fields/AQ.CaseType", "from": null, "value": this.type_chucnang === 0 ? "BF - Lỗi cần sửa code" : "NF - Yêu cầu mới cần sửa code" },
      { "op": "add", "path": "/fields/AQ.Customer", "from": null, "value": this.MaKhachHang },
      { "op": "add", "path": "/fields/AQ.StateChangedBy", "from": null, "value": "TiepNhanSup" },
      { "op": "add", "path": "/fields/System.AssignedTo", "from": null, "value": "TiepNhanSup" },
      { "op": "add", "path": "/fields/System.CreatedBy", "from": null, "value": "TiepNhanSup" },
      { "op": "add", "path": "/fields/System.ChangedBy", "from": null, "value": "TiepNhanSup" },
      { "op": "add", "path": "/fields/System.WorkItemType", "from": null, "value": "CS Case" },
      { "op": "add", "path": "/fields/System.State", "from": null, "value": "Mở Case" },
      { "op": "add", "path": "/fields/System.Title", "from": null, "value": this.type_chucnang === 0 ? this.f.tieu_de.value : this.f1.tieu_de_hc.value },
      { "op": "add", "path": "/fields/System.Description", "from": null, "value": tmp.toString() }
    ];

    this.http.post('/api/main/createcase', body)
      .subscribe((res1: any) => {
        if (res1 && res1.code === 200) {
          this.ma_case = res1.data?.newcase[0]?.id;

          //add file dinh kem neu co
          if (this.selectedFiles) {
            let filename_dk = [];
            for (let i = 0; i < this.selectedFiles.length; i++) {
              if (this.selectedFiles[i]?.name && this.base64) {
                filename_dk.push({ filename: this.selectedFiles[i]?.name });
                const body1 = { filter: { filename: this.selectedFiles[i]?.name, base64: this.base64 } }
                this.http.post('/api/main/upload_dinhkemfile', body1) // add hinh lên server
                  .subscribe((res1_: any) => { }, (error) => { console.log('2: ' + error); });
              }
            }
            const bodyupdate = { filter: { macase: this.ma_case.toString(), file: filename_dk } }
            this.http.post('/api/main/updatecase', bodyupdate) // update link đính kèm vào case
              .subscribe((resup: any) => { }, (error) => { console.log('3: ' + error); });
          }

          this.thanhcong();
          this.changeDetector.detectChanges();
          this.router.navigate(['/main/cscase']);
        }
        else {
          console.log("aaaa")
          this.spinner.hide('spinner_tc'); this.toastr.warning('Tạo ticket (Case) không thành công');
        }
      }, (error) => { console.log(error.toString()); this.spinner.hide('spinner_tc'); this.toastr.error('Tạo ticket (Case) không thành công'); });
  }

  public Btn_Create_Case() {

    if (!this.ValidateFromInput()) {
      this.toastr.error('Vui lòng nhập đầy đủ thông tin');
      window.scroll(0, 200);
      return;
    }

    this.Tao_Case();
  }

  private thanhcong() {
    this.spinner.hide('spinner_tc');
    this.toastr.success('Đã tạo ticket (Case) thành công');
    sessionStorage.setItem('resfesh', '1');

    if (this.type_chucnang === 0) {
      this.FormPhanAnh.reset();
      this.uploadForm1.reset();
      this.FileName_dinhkem1 = '';
      this.myInput1.nativeElement.value = '';
    }
    else {
      this.FromHieuChinhTuVan.reset();
      this.uploadForm2.reset();
      this.FileName_dinhkem2 = '';
      this.myInput2.nativeElement.value = '';
    }

    const initialState = {
      icon: 'i', button: 'OK', message:
        `<p style="font-weight:bold;text-decoration-line:underline;">Mã case: ${this.ma_case}</p>Chúng tôi rất cảm ơn và mong tiếp tục được đồng hành, phối hợp cùng Anh/Chị trong thời gian vận hành cũng như phát triển phần mềm.`,
    };

    this.modalService.show(MessBoxComponent, {
      initialState, ignoreBackdropClick: true, animated: false, class: 'modal-dialog-centered',
    });
  }

  private getdulieutrenform() {

    let gop_string = "";

    if (this.type_chucnang === 0) {
      if (this.f.mo_ta.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;">Mô tả lỗi:</span></br>${this.f.mo_ta.value}</br>`;
      }
      if (this.f.menu_thuc_hien.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;"></br>Menu thực hiện:</span></br>${this.f.menu_thuc_hien.value}</br>`;
      }
      if (this.f.nut_gay_loi.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;"></br>Nút thực hiện gây lỗi:</span></br>${this.f.nut_gay_loi.value}</br>`;
      }
      if (this.f.nhiem_y.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;"></br>Nhiệm ý:</span></br>${this.f.nhiem_y.value}</br>`;
      }
      if (this.f.hinh_anh.value) {
        gop_string += `${this.string_ha}`;
      }
      if (this.f.thao_tac.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;"></br>Thao tác các bước:</span></br>${this.f.thao_tac.value}</br>`;
      }
      if (this.f.ket_qua_dung.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;"></br>Kết quả đúng:</span></br>${this.f.ket_qua_dung.value}</br>`;
      }
      if (this.f.noi_dung_khac.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;"></br>Nội dung khác:</span></br>${this.f.noi_dung_khac.value}</br>`;
      }
      if (this.f.nguoi_yeu_cau_hoten.value || this.f.nguoi_yeu_cau_chucvu.value || this.f.nguoi_yeu_cau_phongban.value || this.f.nguoi_yeu_cau_dienthoai.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;"></br>Thông tin người gửi:</span></br>`;
        if (this.f.nguoi_yeu_cau_hoten.value) {
          gop_string += `<span style="font-weight:bold;"> - Họ tên: </span> ${this.f.nguoi_yeu_cau_hoten.value}</br>`;
        }
        if (this.f.nguoi_yeu_cau_chucvu.value) {
          gop_string += `<span style="font-weight:bold;"> - Chức vụ: </span>${this.f.nguoi_yeu_cau_chucvu.value}</br>`;
        }
        if (this.f.nguoi_yeu_cau_phongban.value) {
          gop_string += `<span style="font-weight:bold;"> - Phòng ban: </span>${this.f.nguoi_yeu_cau_phongban.value}</br>`;
        }
        if (this.f.nguoi_yeu_cau_dienthoai.value) {
          gop_string += `<span style="font-weight:bold;"> - Điện thoại: </span> ${this.f.nguoi_yeu_cau_dienthoai.value}</br>`;
        }
      }
    }
    else {
      if (this.f1.muc_dich.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;">* Mục đích và nghiệp vụ thực tế:</span></br>${this.f1.muc_dich.value}</br>`;
      }
      if (this.f1.noi_dung_can_hc.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;"></br>* Nội dung cần hiệu chỉnh/tư vấn:</span></br>${this.f1.noi_dung_can_hc.value}</br>`;
      }
      if (this.f1.vi_du_thuc_te.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;"></br>* Ví dụ dữ liệu thực tế:</span></br>${this.f1.vi_du_thuc_te.value}</br>`;
      }
      if (this.f1.menu_thuc_hien_hc.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;"></br>* Menu thực hiện:</span></br>${this.f1.menu_thuc_hien_hc.value}</br>`;
      }
      if (this.f1.nut_thuc_hien.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;"></br>* Nút thực hiện:</span></br>${this.f1.nut_thuc_hien.value}</br>`;
      }
      if (this.f1.hinh_anh_hc.value) {
        gop_string += `${this.string_ha}`;
      }
      if (this.f1.thao_tac_hc.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;"></br>* Thao tác các bước:</span></br>${this.f1.thao_tac_hc.value}</br>`;
      }
      if (this.f1.noi_dung_khac_hc.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;"></br>* Nội dung khác:</span></br>${this.f1.noi_dung_khac_hc.value}</br>`;
      }
      if (this.f1.noi_dung_khac_hc.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;"></br>* Nội dung khác:</span></br>${this.f1.noi_dung_khac_hc.value}</br>`;
      }
      if (this.f1.nguoi_yeu_cau_hc_hoten.value || this.f1.nguoi_yeu_cau_hc_chucvu.value || this.f1.nguoi_yeu_cau_hc_phongban.value || this.f1.nguoi_yeu_cau_hc_dienthoai.value) {
        gop_string += `<span style="font-weight:bold;text-decoration-line:underline;"></br>Thông tin người gửi:</span></br>`;
        if (this.f1.nguoi_yeu_cau_hc_hoten.value) {
          gop_string += `<span style="font-weight:bold;"> - Họ tên: </span>${this.f1.nguoi_yeu_cau_hc_hoten.value}</br>`;
        }
        if (this.f1.nguoi_yeu_cau_hc_chucvu.value) {
          gop_string += `<span style="font-weight:bold;"> - Chức vụ: </span>${this.f1.nguoi_yeu_cau_hc_chucvu.value}</br>`;
        }
        if (this.f1.nguoi_yeu_cau_hc_phongban.value) {
          gop_string += `<span style="font-weight:bold;"> - Phòng ban: </span>${this.f1.nguoi_yeu_cau_hc_phongban.value}</br>`;
        }
        if (this.f1.nguoi_yeu_cau_hc_dienthoai.value) {
          gop_string += `<span style="font-weight:bold;"> - Điện thoại: </span>${this.f1.nguoi_yeu_cau_hc_dienthoai.value}</br>`;
        }
      }
    }
    return gop_string.toString();
  }

  public getFileUpLoad(event: any, type) {
    if (event) {
      this.selectedFiles = event;
      for (let i = 0; i < this.selectedFiles.length; i++) {
        this.getFile(this.selectedFiles[i], type);
      }
    }
  }

  public getFile(file: File, type) {
    if (file) {
      this.base64 = '';
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onloadend = (e) => {
        const tmp = file.name;
        if (type === 1) {
          this.FileName_dinhkem1 += tmp + ', ';
        }
        else if (type === 2) {
          this.FileName_dinhkem2 += tmp + ', ';
        }
        this.base64 = reader.result.toString();
      }
      reader.onerror = (error) => {
        this.base64 = '';
        if (type === 1) {
          this.myInput1.nativeElement.value = this.FileName_dinhkem1 = '';
        }
        else if (type === 2) {
          this.myInput2.nativeElement.value = this.FileName_dinhkem2 = '';
        }
      };
    };
  }

  private ValidateFromInput(): boolean {
    if (this.type_chucnang === 0) {
      this.submitted = true;
      this.submitted1 = false;
      if (this.FormPhanAnh.invalid) {
        return false;
      }
      else { return true; }
    }
    else {
      this.submitted = false;
      this.submitted1 = true;
      if (this.FromHieuChinhTuVan.invalid) {
        return false;
      }
      else { return true; }
    }
  }

  private stringToHTML = function (str) {
    var dom = document.createElement('div');
    dom.innerHTML = str;
    return dom;
  };

  public trackByFn(index: number) { return index; }
}
