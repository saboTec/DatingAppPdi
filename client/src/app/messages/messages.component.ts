import { Component, inject, OnInit } from '@angular/core';
import { MembersService } from '../_services/members.service';
import { MessageService } from '../_services/message.service';
import { FormsModule, NgForm } from '@angular/forms';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { TimeagoModule } from 'ngx-timeago';
import { Message } from '../_models/message';
import { RouterLink } from '@angular/router';
import { PaginationModule } from 'ngx-bootstrap/pagination';

@Component({
  selector: 'app-messages',
  imports: [ButtonsModule,FormsModule,TimeagoModule,RouterLink,PaginationModule],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.css'
})
export class MessagesComponent implements OnInit{

  ///Not private because i want to use it in template
  messageService = inject (MessageService);
  container = 'Inbox';
  pageNumber = 1;
  pageSize = 5;
  isOutbox = this.container === 'Outbox'


  ngOnInit(): void {
    this.loadMessages();
  }
  
  loadMessages(){
    this.messageService.getMessages(this.pageNumber,this.pageSize,this.container);
  }

  ///USE this '' will not work but only this ``
  ///Correct Way to Fix getRoute() in messages.component.ts:
  
  getRoute(message: Message) {
  if (this.isOutbox) return `/members/${message.recipientUsername}`;
  else return `/members/${message.senderUsername}`;
}

  ///is called on the event from the template e.g. 
  pageChanged(event:any){
    if(this.pageNumber != event.pageNumber){
      this.pageNumber = event.pageNumber;
      this.loadMessages();
    }
  }
  deleteMessage(id:number){
    this.messageService.deleteMessage(id).subscribe({
      next: _ => {
        this.messageService.paginatedResult.update(prev => {
          if(prev && prev.items){
            prev.items.splice(prev.items.findIndex(m=>m.id === id ),1);
          }
          return prev
        })
      }
    })
  }
}
