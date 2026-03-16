import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order } from './order.model';

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private apiUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  getValidSymbols(): Observable<any> {
    return this.http.get(`${this.apiUrl}/GetValidSymbols`);
  }

  createOrder(order: Order): Observable<any> {
    return this.http.post(`${this.apiUrl}/CreateOrder`, order);
  }
}
