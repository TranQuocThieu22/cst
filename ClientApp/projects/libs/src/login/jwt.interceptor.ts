import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) { }

  public intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // add auth header with jwt if user is logged in and request is to api url
    const currentUser = this.authService.currentUserValue;
    const isLoggedIn = currentUser;
    const isApiUrl = request.url.indexOf('/api/') >= 0;
    if (isLoggedIn && isApiUrl) {
      request = request.clone({
        setHeaders: {
          'Cache-Control': 'no-cache',
          'Pragma': 'no-cache',
          Authorization: `Bearer ${currentUser}`
        }
      });
    }

    return next.handle(request);
  }
}
