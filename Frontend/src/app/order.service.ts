import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private apiUrl = 'https://localhost:5001/api';

  constructor(private http: HttpClient) {}

  getValidSymbols(): Observable<any> {
    return this.http.get(`${this.apiUrl}/GetValidSymbols`);
  }
}
