import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { PrimeNGConfig } from 'primeng/api';

@Component({
  selector: 'app-aq-main',
  templateUrl: './aq-main.component.html',
  styleUrls: ['./aq-main.component.scss']
})
export class AqMainComponent implements OnInit {
  visibleSidebar1;
  constructor(private router: Router,
    private primengConfig: PrimeNGConfig
  ) { }

  ngOnInit(): void {
    this.primengConfig.ripple = true;
  }
  public NavigateNhanSuAq() {
    this.router.navigate(['main/aq-main/nhansuaq']);
  }

}
