import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ActiveRuleService {

  constructor(private http: HttpClient) { }

  getConfig(origin: string) {
    const url = 'https://localhost:44359/api/BR/GetBusinessRules';
    let queryParams = new HttpParams();
    queryParams = queryParams.append("origin",origin);
    return this.http.get<any>(url,{params:queryParams});
  }
  saveData(data) {
    return this.http.post<any>('https://localhost:44359/api/BR/SaveBusinessRules',data);
  }
}
