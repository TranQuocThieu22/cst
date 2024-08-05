import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-aq-main',
  templateUrl: './aq-main.component.html',
  styleUrls: ['./aq-main.component.scss']
})
export class AqMainComponent implements OnInit {

  constructor(private router: Router) { }

  ngOnInit(): void {
  }
  public NavigateNhanSuAq() {
    this.router.navigate(['main/aq-main/nhansuaq']);
  }

}
