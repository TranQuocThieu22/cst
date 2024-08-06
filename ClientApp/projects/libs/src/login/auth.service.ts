import { Injectable } from '@angular/core';
import { HttpClient, HttpParameterCodec } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { User } from './user';

@Injectable({ providedIn: 'root' })

export class AuthService {
  private currentUserSubject: BehaviorSubject<User>;
  public currentUser: Observable<User>;

  constructor(private http: HttpClient) {
    this.currentUserSubject = new BehaviorSubject<User>(JSON.parse(sessionStorage.getItem('current-user')));
    this.currentUser = this.currentUserSubject.asObservable();
  }

  public get currentUserValue(): User {
    return this.currentUserSubject.value;
  }

  public login(username: string, password: string) {
    // const options = {
    //   headers: new HttpHeaders()
    //     .set('Content-Type', 'application/x-www-form-urlencoded')
    // };
    return this.http.post<any>('api/main/login', { username, password })
      .pipe(map((response: User) => {
        console.log(response);

        if (response) {
          // response[0].pass = '';
          sessionStorage.setItem('current-user', JSON.stringify(response));
          this.currentUserSubject.next(response);
        }
        return response;
      }), catchError(error => throwError(error))
      );
  }

  public logout() {
    this.currentUserSubject.next(null);
    sessionStorage.clear();
    sessionStorage.removeItem('current-user');
    return this.http.get('/api/main/logout');
  }
}

// class CustomEncoder implements HttpParameterCodec {
//   encodeKey(key: string): string {
//     return encodeURIComponent(key);
//   }

//   encodeValue(value: string): string {
//     return encodeURIComponent(value);
//   }

//   decodeKey(key: string): string {
//     return decodeURIComponent(key);
//   }

//   decodeValue(value: string): string {
//     return decodeURIComponent(value);
//   }
// }

@Injectable({ providedIn: 'root' })
export class ServerLinkService {
  private user: User;
  public islogin: BehaviorSubject<boolean> = new BehaviorSubject(false);
  public isAdmin: BehaviorSubject<boolean> = new BehaviorSubject(false);

  constructor() {
    this.setLogin(!!sessionStorage.getItem('current-user'));
  }

  public setLogin(state: boolean = false) {
    this.islogin.next(state);
    if (state) {
      this.user = JSON.parse(sessionStorage.getItem('current-user'));
      if (this.user) {
        if (this.user.roles === 'admin') {
          this.isAdmin.next(true);
        }
        else {
          this.isAdmin.next(false);
        }
      }
    }
    else { this.islogin.next(false); this.isAdmin.next(false); }
  }
}
