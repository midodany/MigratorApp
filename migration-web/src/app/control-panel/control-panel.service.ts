import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class ControlPanelService {

  constructor(private http: HttpClient) { }

  public EntityName: string = "";
  public PropertyRelationName: string = "";
  public Domain: string = "";
}