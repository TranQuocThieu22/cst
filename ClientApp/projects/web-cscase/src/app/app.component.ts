import { Component, OnInit, OnDestroy, Inject, HostListener } from '@angular/core';
import { ServerLinkService } from '@mylibs';
import { HttpClient } from '@angular/common/http';
import { DOCUMENT } from '@angular/common';
import { NavigationEnd, Router } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})

export class AppComponent implements OnInit, OnDestroy {

  // show-hide ButtonScroll
  public isShowButtonScroll: boolean;
  // vi tri show ButtonScroll
  private topShowing = 500;

  constructor(private serverLink: ServerLinkService, private http: HttpClient,
    public router: Router, @Inject(DOCUMENT) private doc: Document,) {
  }

  public ngOnInit(): void {
    this.router.events.subscribe((evt) => {
      if (!(evt instanceof NavigationEnd)) {
        return;
      }

      if (evt.url === '/home' || evt.url === '/' || evt.url === '') {
        this.ngOnDestroy();
      }
    });

    this.http.get(`/assets/themes/bootstrap-Cerulean.min.css`, { responseType: 'text' })
      .subscribe(data => {
        this.doc.getElementById('dynamic-theme').innerHTML = data || 'Cerulean';
      });
    window.scroll(0, 0);
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

  // click button scroll to top
  public gotoTop() {
    window.scroll({ top: 0, left: 0, behavior: 'smooth' });
  }

  public ngOnDestroy() {
    this.serverLink.setLogin(false);
    //sessionStorage.clear();
  }

}
