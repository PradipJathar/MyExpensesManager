import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface Category {
  id: number;
  categoryName: string;
  isDefault: boolean;
  colorCode: string;
}

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  constructor(private apiService: ApiService) {}

  getAllCategories(): Observable<Category[]> {
    return this.apiService.get<Category[]>('categories');
  }
}
