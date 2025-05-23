import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { AccountService } from './account.service';
import { environment } from '../../environments/environment';
import { PaginatedResult } from '../_models/pagination';
import { Message } from '../_models/message';
import { setPaginatedResponse, setPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  
  private http = inject(HttpClient);
  private accountService = inject(AccountService);
  
  baseUrl = environment.apiUrl

  paginatedResult = signal<PaginatedResult<Message[]> | null>(null);

  getMessages(pageNumber:number,pageSize:number,container : string){
    let params = setPaginationHeaders(pageNumber,pageSize);

    params = params.append('Container',container);

    return this.http.get<Message[]> (this.baseUrl + 'messages',{observe: 'response',params})
      .subscribe({
        next: response => setPaginatedResponse(response,this.paginatedResult)
      })
  }

  
}
