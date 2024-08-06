import { Component, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ServerLinkService, User, AuthService } from '@mylibs';
import { NgxSpinnerService } from 'ngx-spinner';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})

export class LoginComponent implements OnInit, AfterViewInit {
  // @ViewChild('myfocus') myFocusField: ElementRef;
  @ViewChild('myFocusField') myFocusField: ElementRef<HTMLInputElement>;
  public loginForm: FormGroup;
  public submitted = false;
  public islogin: boolean;
  public currentUser: User;

  public SHOW_BUTTON: boolean;
  public SHOW_EYE: boolean;
  public messError = '';

  constructor(private serverLink: ServerLinkService, public router: Router, private spinner: NgxSpinnerService,
    private authenticationService: AuthService, private formBuilder: FormBuilder) { }

  get f() { return this.loginForm.controls; }

  public ngOnInit() {
    this.islogin = false;
    this.messError = '';
    this.serverLink.setLogin(false);
    //sessionStorage.clear();
    this.loginForm = this.formBuilder.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  public ngAfterViewInit() {
    if (this.myFocusField !== undefined) {
      this.myFocusField.nativeElement.focus();
    }
  }

  public onLogin() {
    this.messError = '';
    this.submitted = true;

    if (!this.ValidateFromInput()) {
      return;
    }
    else {
      this.spinner.show();
      this.submitted = true;
      this.authenticationService.login(this.f.username.value.trim().toUpperCase(), this.f.password.value.trim())
        .subscribe((data: User) => {
          if (data) {
            if (data.maTruong) {
              this.spinner.hide();
              this.serverLink.setLogin(true);
              this.islogin = true;
              this.router.navigate(['/main/cscase']);
              if (data.roles === "TFS") {
                this.router.navigate(['/main/aq-main']);
              }

            }
            else {
              this.messError = 'Đăng nhập không thành công';
              this.spinner.hide();
              this.islogin = false;
              sessionStorage.clear();
              this.serverLink.setLogin(false);
            }
          }
          else {
            this.spinner.hide();
            this.islogin = false;
            this.serverLink.setLogin(false);
            sessionStorage.clear();
            this.messError = 'Đăng nhập không thành công';
          }
        }, error => {
          this.spinner.hide();
          this.islogin = false;
          this.serverLink.setLogin(false);
          sessionStorage.clear();
          this.messError = 'Đăng nhập không thành công';
        });

      setTimeout(() => { this.spinner.hide(); }, 20000);
    }
  }

  public showPassword() {
    this.SHOW_BUTTON = !this.SHOW_BUTTON;
    this.SHOW_EYE = !this.SHOW_EYE;
  }

  public ValidateFromInput(): boolean {
    this.submitted = true;
    if (this.loginForm.invalid) {
      return false;
    }
    else { return true; }
  }

}
