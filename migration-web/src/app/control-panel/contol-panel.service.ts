import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class ControlPanelService {

  constructor(private http: HttpClient) { }

  getConfig() {
    return this.http.get<any>('https://localhost:44359/api/BR/GetBusinessRules');
  }
}