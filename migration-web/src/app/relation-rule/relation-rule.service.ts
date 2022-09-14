import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class RelationRuleService {

  constructor(private http: HttpClient) { }

  getConfig() {
    const url = 'https://localhost:44359/api/BR/GetRelationRules';
    return this.http.get<any>(url);
  }

  saveData(data) {
    return this.http.post<any>('https://localhost:44359/api/BR/SaveRelationRules',data);
  }

}
