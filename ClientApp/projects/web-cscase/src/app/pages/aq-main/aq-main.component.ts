import { HttpClient } from '@angular/common/http';
import { AuthService } from '@mylibs';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MenuItem, PrimeNGConfig } from 'primeng/api';

@Component({
  selector: 'app-aq-main',
  templateUrl: './aq-main.component.html',
  styleUrls: ['./aq-main.component.scss']
})
export class AqMainComponent implements OnInit {
  public leaderChecker;

  sidebarVisible: boolean = false;
  userMenuItems: MenuItem[];
  items: MenuItem[];

  isOpen_EditAvatarDialog: boolean = false;
  isOpen_EditPasswordDialog: boolean = false;

  userInfo: any;
  userAvatar: any;

  oldPassword: any
  newPassword: any
  confirmPassword: any
  isValidConfirmPassword: boolean = true;

  constructor(private router: Router,
    private primengConfig: PrimeNGConfig,
    private https: HttpClient,
    private AuthService: AuthService
  ) {
    this.userMenuItems = [
      {
        label: 'Profile',
        icon: 'pi pi-user',
        command: () => {
          // Handle profile action
        }
      },
      {
        label: 'Settings',
        icon: 'pi pi-cog',
        command: () => {
          // Handle settings action
        }
      },
      {
        separator: true
      },
      {
        label: 'Log Out',
        icon: 'pi pi-sign-out',
        command: () => {
          this.logOut();
        }
      }
    ];
  }
  toggleSidebar() {
    this.sidebarVisible = !this.sidebarVisible;
  }

  showUserMenu(event: Event) {
    // You'll need to use ViewChild to get a reference to the p-menu
    // this.userMenu.toggle(event);
  }


  ngOnInit(): void {
    this.leaderChecker = JSON.parse(sessionStorage.getItem('current-user'));
    this.primengConfig.ripple = true;

    this.AuthService.sessionItem$.subscribe(value => {
      this.userInfo = value;
    });

    // this.userInfo = JSON.parse(sessionStorage.getItem('current-user'));

    this.userAvatar = this.userInfo.avatar;
    // this.items = [
    //   {
    //     label: 'Users',
    //     icon: 'pi pi-fw pi-user',
    //     items: [
    //       {
    //         label: 'Đổi avatar',
    //         icon: 'pi pi-fw pi-instagram',

    //       },
    //       {
    //         label: 'Đổi mật khẩu',
    //         icon: 'pi pi-fw pi-lock',

    //       },
    //       {
    //         label: 'Search',
    //         icon: 'pi pi-fw pi-users',
    //         items: [
    //           {
    //             label: 'Filter',
    //             icon: 'pi pi-fw pi-filter',
    //             items: [
    //               {
    //                 label: 'Print',
    //                 icon: 'pi pi-fw pi-print'
    //               }
    //             ]
    //           },
    //           {
    //             icon: 'pi pi-fw pi-bars',
    //             label: 'List'
    //           }
    //         ]
    //       }
    //     ]
    //   }
    // ];

    this.items = [
      {
        label: 'Đổi avatar',
        icon: 'pi pi-fw pi-instagram',
        command: () => {
          this.isOpen_EditAvatarDialog = true;
          this.userAvatar = this.userInfo.avatar;
        }
      },
      {
        label: 'Đổi mật khẩu',
        icon: 'pi pi-fw pi-lock',
        command: () => {
          this.isOpen_EditPasswordDialog = true;
        }

      },
    ];
  }

  public NavigateNhanSuAq() {
    this.router.navigate(['main/aq-main/nhansuaq']);
  }

  checkIsLeader() {
    const user = sessionStorage.getItem('current-user');
    return JSON.parse(user).isLeader;
  }
  public logOut() {
    sessionStorage.clear();
    sessionStorage.removeItem('current-user');
    this.router.navigate(['']);
  }

  handleUploadAvatar(event) {
    if (event.target.files && event.target.files[0]) {
      var reader = new FileReader();
      reader.readAsDataURL(event.target.files[0]); // read file as data url
      reader.onload = (event) => {
        // called once readAsDataURL is completed
        this.userAvatar = event.target.result as string;
      };
    }
  }

  handleUpdateAvatarAPI() {
    if (this.userAvatar === undefined || this.userAvatar === '') {
      this.userAvatar = null;
    }
    let aqmemberUpdateInfo = {
      avatar: this.userAvatar,
    };

    this.https.put<any>("/api/ThongTinCaNhan/avatar/" + this.userInfo.id, aqmemberUpdateInfo).subscribe({
      next: (res: any) => {
        //todo 
        //sẽ handle Type chuẩn cho currentUserValue trong sessionStorage sau khi trao đổi với cách code cst cũ

        let currentData = this.AuthService.currentUserValue;
        currentData.avatar = res.data;
        this.AuthService.setSessionItem_UserData(currentData);
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)

      }
    });

    this.deleteInputImg();
    this.isOpen_EditAvatarDialog = false
  }

  deleteInputImg() {
    this.userAvatar = null;
  }

  checkConfirmPassword() {
    if (this.newPassword === this.confirmPassword) {
      this.isValidConfirmPassword = true;
    } else {
      this.isValidConfirmPassword = false;
    }
  }

  handleChangePassword() {
    let inputData = {
      id: this.userInfo.id,
      currentpassword: this.oldPassword,
      newPassword: this.newPassword,
      confirmPassword: this.confirmPassword
    }

    this.https.put<any>("api/TFSAccount/changePassword", inputData).subscribe({
      next: (res: any) => {
        //todo 
      },
      error: (error) => {
        console.log(error);
        // Your logic for handling errors
      },
      complete: () => {
        // Your logic for handling the completion event (optional)
        this.isOpen_EditPasswordDialog = false
      }
    });
  }
}
