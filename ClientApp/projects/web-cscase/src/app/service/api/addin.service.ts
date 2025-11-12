import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { DanhSachAddinResultDTO } from '../../pages/danh-sach-addin/danh-sach-addin';

@Injectable({
    providedIn: "root"
})

export class DanhSachAddinService {
    private apiAddin = '/api/DanhSachAddin/FetchDanhSachAddin';

    constructor(private http: HttpClient) { }

    fetchDanhSachAddin(input: {}): Observable<DanhSachAddinResultDTO> {
        return this.http.post<DanhSachAddinResultDTO>(this.apiAddin, input)
    }
}