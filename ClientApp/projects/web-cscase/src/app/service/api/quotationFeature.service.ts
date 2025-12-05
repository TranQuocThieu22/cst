import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { QuotationFeatureDTO } from "../../pages/tinh-nang-bao-gia/tinh-nang-bao-gia";

@Injectable({ providedIn: 'root' })
export class QuotationFeatureService {
    private baseUrl = '/api/TinhNangBaoGia';

    constructor(private http: HttpClient) { }

    getList(): Observable<QuotationFeatureDTO[]> {
        return this.http.get<QuotationFeatureDTO[]>(this.baseUrl);
    }

    // Dùng chung FormData cho cả Add Group/Module/Feature
    add(payload: FormData): Observable<QuotationFeatureDTO> {
        return this.http.post<QuotationFeatureDTO>(`${this.baseUrl}/Add`, payload);
    }

    update(payload: FormData): Observable<QuotationFeatureDTO> {
        return this.http.put<QuotationFeatureDTO>(`${this.baseUrl}/Update`, payload);
    }

    delete(id: number): Observable<any> {
        return this.http.delete(`${this.baseUrl}?id=${id}`);
    }

    downloadFile(fileId: string): Observable<Blob> {
        return this.http.get(`${this.baseUrl}/DownloadFile?fileId=${fileId}`, {
            responseType: 'blob'
        });
    }
}