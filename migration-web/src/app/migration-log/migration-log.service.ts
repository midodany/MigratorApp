import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class MigrationLogService {

  constructor(private http: HttpClient) { }

  getBatches() {
    const url = 'https://localhost:44359/api/Log/GetBatches';
    return this.http.get<any>(url);
  }

  GetLogObjects(batchId: string) {
    const url = 'https://localhost:44359/api/Log/GetLogObjects';
    let queryParams = new HttpParams();
    queryParams = queryParams.append("batchId",batchId);
    return this.http.get<any>(url,{params:queryParams});
  }
}
