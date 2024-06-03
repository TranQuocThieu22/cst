import { Component, OnInit, OnDestroy, HostListener } from '@angular/core';
import { ServerLinkService } from '@mylibs';
import { NgxSpinnerService } from 'ngx-spinner';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { Router } from '@angular/router';
import { DataServices } from '../../service/dataservices.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.scss'],
})

export class MainComponent implements OnInit, OnDestroy {
  public linkKhaoSat: string = 's'
  public islogin: boolean; public isadmin: boolean = false; public isadministrator: boolean = false;
  public tentheme: string; public showFlag: boolean; public ShowSideBar = false;
  public currentUser: any; public selectedIndex: number; public hideDiv = false;
  public hideDivTK = false; public isShowButtonScroll: boolean; private topShowing = 100;
  public IsData: boolean = false;
  selectedOption: string;
  http: any;
  constructor(private serverLink: ServerLinkService, private https: HttpClient, private spinner: NgxSpinnerService, public router: Router,
    public modalService: BsModalService, public bsModalRefEditor: BsModalRef, private dataServices: DataServices) {

    // serverLink.islogin.subscribe((state) => (this.islogin = state));
    // serverLink.isAdmin.subscribe((state) => (this.isadmin = state));
    // this.currentUser = this.authenticationService.currentUserValue;
    this.https.post<any>("/api/main/view_khao_sat", "").subscribe({
      next: (res: any) => {
        this.linkKhaoSat = res.value
        console.log(res);
        // Your logic for handling the response
        localStorage.setItem("linkhaosat", res.value)
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
      }
    });
    if (localStorage.getItem("dateRange") == null) {
      this.selectedOption = "24-25"
      localStorage.setItem("dateRange", this.selectedOption)
    }
    this.selectedOption = localStorage.getItem("dateRange")
    if (sessionStorage.getItem('current-user')) {
      this.islogin = true;
      this.currentUser = JSON.parse(sessionStorage.getItem('current-user'));
      this.currentUser.pass = '';
      sessionStorage.setItem('current-user', JSON.stringify(this.currentUser));
      if (this.currentUser.roles.toLowerCase() === 'admin') {
        this.isadmin = true;
      }
      else if (this.currentUser.roles.toLowerCase() === 'administrator') {
        this.isadministrator = true;
      }
    }

    this.dataServices.BieuDo.subscribe((res: any) => {
      this.hideDivTK = res;
    });

  }

  public ngOnInit(): void {
    // if (!this.islogin) {
    //   this.router.navigate(['/']);
    // }

    // if (sessionStorage.getItem('db')) {
    //   this.IsData = true;
    //   this.dataServices.IsData.next(true);
    // }
  }

  public ngAfterViewChecked(): void {
    this.dataServices.IsData.subscribe((res: any) => {
      if (res) {
        this.IsData = true;
        return;
      }
    });
  }

  public GuiYeuCau(type: number) {
    this.router.navigate(['main/taocase/' + type]);
  }

  // show hide Imgage cscase
  public showtrangthai() {
    this.hideDiv = !this.hideDiv;
  }

  public closetrangthai() {
    this.hideDiv = false
  }

  public showthongke() {
    // this.hideDivTK = !this.hideDivTK;
    // this.hideDivTK = true;
    // this.dataServices.BieuDo.next(this.hideDivTK);
    // this.dataServices.LoaiBieuDo.next(1);
    this.router.navigate(['main/thongke']);

  }

  public Refesh_page() {
    sessionStorage.setItem('resfesh', '1');
    window.location.reload();
  }

  public onLogout() {
    this.serverLink.setLogin(false);
    sessionStorage.clear();
    this.islogin = false;
    this.isadmin = false;
    this.router.navigate(['/']);
  }

  public ngOnDestroy() {
    this.serverLink.setLogin(false);
    this.islogin = false;
    this.isadmin = false;
    //sessionStorage.clear();
  }

  public Open_usage_manual() {
    window.open("https://shorturl.at/iFHNZ", '_blank');
  }

  public onSubmitLocCase() {
    localStorage.setItem("dateRange", this.selectedOption)
    window.location.reload();
  }

  // get vi tri man hinh hide show button scroll
  @HostListener('window:scroll')
  public checkScroll() {
    const scrollPosition = window.pageYOffset
      || document.documentElement.scrollTop
      || document.body.scrollTop || 0;
    if (scrollPosition >= this.topShowing) {
      this.isShowButtonScroll = true;
    } else {
      this.isShowButtonScroll = false;
    }
  }
  @HostListener('window:beforeunload', ['$event'])
  unloadHandler(event: Event) {
    sessionStorage.setItem('resfesh', '1');
    window.location.reload();
  }

  // click button scroll to top
  public gotoTop() {
    window.scroll({ top: 0, left: 0, behavior: 'smooth' });
  }
}
