import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PrimeNGConfig } from 'primeng/api';

@Component({
  selector: 'app-aq-main',
  templateUrl: './aq-main.component.html',
  styleUrls: ['./aq-main.component.scss']
})
export class AqMainComponent implements OnInit {
  public leaderChecker;
  constructor(private router: Router,
    private primengConfig: PrimeNGConfig,
    private https: HttpClient,

  ) { }

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
  }
}
