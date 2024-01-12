import { Injectable } from '@angular/core';
import { HttpClient, HttpRequest, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
    providedIn: 'root'
})

export class UploadFilesService {

    constructor(private http: HttpClient) { }

    public upload(API: string, bodyAPI: any): Observable<HttpEvent<any>> {
        const req = new HttpRequest('POST', API, bodyAPI, {
            reportProgress: true,
            responseType: 'json'
        });
        return this.http.request(req);
    }
}
