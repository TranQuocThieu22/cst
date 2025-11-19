import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { DanhSachAddinResultDTO, InputUpdateAddin, UpdateAddinResultDTO } from '../../pages/danh-sach-addin/danh-sach-addin';

@Injectable({
    providedIn: "root"
})

export class DanhSachAddinService {
    private apiBase = '/api/DanhSachAddin';

    constructor(private http: HttpClient) { }

    fetchDanhSachAddin(input: {}): Observable<DanhSachAddinResultDTO> {
        return this.http.post<DanhSachAddinResultDTO>(`${this.apiBase}/FetchDanhSachAddin`, input);
    }

    updateAddin(input: InputUpdateAddin): Observable<UpdateAddinResultDTO> {
        return this.http.post<UpdateAddinResultDTO>(`${this.apiBase}/UpdateAddin`, input);
    }

    updateAddinFiles(data: FormData): Observable<any> {
        return this.http.post<any>(`${this.apiBase}/UpdateAddinFiles`, data);
    }

    getFileMeta(idAddin: string): Observable<any> {
        return this.http.get<any>(`${this.apiBase}/GetFileMeta/${idAddin}`);
    }

    downloadFile(idAddin: string, fileType: 'word' | 'pdf'): Observable<Blob> {
        return this.http.get(`${this.apiBase}/DownloadFile?idAddin=${idAddin}&fileType=${fileType}`, {
            responseType: 'blob'
        });
    }

}