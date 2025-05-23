import { Component, inject, OnInit } from '@angular/core';
import { MembersService } from '../_services/members.service';
import { MessageService } from '../_services/message.service';
import { FormsModule, NgForm } from '@angular/forms';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { TimeagoModule } from 'ngx-timeago';

@Component({
  selector: 'app-messages',
  imports: [ButtonsModule,FormsModule,TimeagoModule],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.css'
})
export class MessagesComponent implements OnInit{

  ///Not private because i want to use it in template
  messageService = inject (MessageService);
  container = 'Inbox';
  pageNumber = 1;
  pageSize = 5;


  ngOnInit(): void {
    this.loadMessages();
  }
  
  loadMessages(){
    this.messageService.getMessages(this.pageNumber,this.pageSize,this.container);
  }
  ///is called on the event from the template e.g. 
  pageChanged(event:any){
    if(this.pageNumber != event.pageNumber){
      this.pageNumber = event.pageNumber;
      this.loadMessages();
    }
  }

}
