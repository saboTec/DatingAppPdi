import { Component, inject, input, OnInit, output, ViewChild } from '@angular/core';
import { MessageService } from '../../_services/message.service';
import { FormsModule, NgForm } from '@angular/forms';
import { TimeagoModule } from 'ngx-timeago';

@Component({
  selector: 'app-member-messages',
  imports: [FormsModule,TimeagoModule],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.css'
})
export class MemberMessagesComponent {
  @ViewChild('messageForm') messageForm?:NgForm;
  messageService = inject(MessageService);
  username = input.required<string>();
  messageContent = '';
  
  sendMessage(){
    this.messageService.sendMessage(this.username(),this.messageContent).then(()=>{
      this.messageForm?.reset();
    });
  }

}
