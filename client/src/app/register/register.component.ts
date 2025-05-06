import { Component, inject, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_service/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  imports: [
    FormsModule,

  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  private accountService = inject(AccountService)
  private toastr = inject(ToastrService)
  cancelRegister = output<boolean>()
  model:any ={}


  register(){
    this.accountService.register(this.model).subscribe({
      next: response => {
        console.log(response)
        this.toastr.success("Welcome aboard")
        this.cancel()
      },
      error: error => 
        this.toastr.error("User is already registered")
        
    });
  }

  cancel(){
    this.cancelRegister.emit(false);
    console.log("Canceled");
  }
}
