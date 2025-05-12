import { HttpClient } from '@angular/common/http';
import { inject, Injectable, model, signal } from '@angular/core';
import { User } from '../_models/users';
import { map } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  private http = inject(HttpClient);
  baseUrl = environment.apiUrl;

  currentUser = signal <User | null>(null);
  
  login(model:any){
    return this.http.post<User>(this.baseUrl + 'account/login',model).pipe(
      map(user => {
        if(user){
          localStorage.setItem('user',JSON.stringify(user));
          this.currentUser.set(user);
          console.log("User set in signal:", this.currentUser());
        }
        return user;
      })
    )
  }
  register(model:any){
    return this.http.post<User>(this.baseUrl + 'account/register',model).pipe(
      map(user => {
        if(user){
          localStorage.setItem('user',JSON.stringify(user));
          this.currentUser.set(user);
          console.log("The User is:",this.currentUser);
          
        }
        return user;
      })
    )
  }
  logout(){
    localStorage.removeItem('user');
    this.currentUser.set(null);

  }
  
}
