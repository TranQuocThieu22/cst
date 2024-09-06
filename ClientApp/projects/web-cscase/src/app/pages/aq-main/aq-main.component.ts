import { HttpClient } from '@angular/common/http';
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
  constructor(private router: Router,
    private primengConfig: PrimeNGConfig,
    private https: HttpClient,

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
    console.log('Logging out...');

  }
}
