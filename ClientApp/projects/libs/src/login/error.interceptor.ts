import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor() { }

  public intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Thêm header 'Cache-Control' vào mỗi yêu cầu
    const modifiedRequest = request.clone({
      setHeaders: {
        'Cache-Control': 'no-cache',
        'Pragma': 'no-cache',
      }
    });

    return next.handle(modifiedRequest).pipe(catchError(err => {
      const error = err.error.message || err.statusText;
      return throwError(error);
      // if ([401, 403].indexOf(err.status) !== -1) {
      // auto logout if 401 Unauthorized or 403 Forbidden response returned from api
      // this.authenticationService.logout();
      // location.reload(true);
      // }
    }));
  }
}
