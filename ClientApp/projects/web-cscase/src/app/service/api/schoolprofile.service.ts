import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
    SchoolDataApiResult, AddinSchoolDataApiResult, AddinSchoolInput,
    SchoolProfileInsertDTO, SchoolProfileInsertResultDTO,
    SchoolProfileResultDTO,
    SchoolProfileUpdateDTO,
    SchoolProfileUpdateResultDTO,
} from '../../pages/danh-sach-truong/SchoolProfile';

@Injectable({
    providedIn: 'root'
})
export class SchoolProfileService {
    private apiSchoolUrl = '/api/DanhSachTruong/GetApiTruong';
    private apiAddinSchoolUrl = '/api/DanhSachTruong/GetApiAddinTruong';
    private schoolProfileUrl = '/api/DanhSachTruong';
    constructor(private http: HttpClient) { }

    getDanhSachTruong(): Observable<SchoolDataApiResult> {
        return this.http.get<SchoolDataApiResult>(this.apiSchoolUrl);
    }

    fetchDanhSachAddinTruong(input: AddinSchoolInput): Observable<AddinSchoolDataApiResult> {
        return this.http.post<AddinSchoolDataApiResult>(this.apiAddinSchoolUrl, input);
    }

    addSchoolProfile(payload: SchoolProfileInsertDTO[]): Observable<SchoolProfileInsertResultDTO> {
        return this.http.post<SchoolProfileInsertResultDTO>(this.schoolProfileUrl, payload);
    }

    getAllSchoolProfile(): Observable<SchoolProfileResultDTO> {
        return this.http.get<SchoolProfileResultDTO>(this.schoolProfileUrl);
    }

    updateOneSchoolProfile(id: string, payload: SchoolProfileUpdateDTO): Observable<SchoolProfileUpdateResultDTO> {
        return this.http.patch<SchoolProfileUpdateResultDTO>(`${this.schoolProfileUrl}/${id}`, payload);
    }

}