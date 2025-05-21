import { Component, inject, NgModule, OnDestroy, OnInit } from '@angular/core';
import { LikesService } from '../_services/likes.service';
import { Member } from '../_models/member';
import { FormsModule } from '@angular/forms';
import { ButtonsModule } from 'ngx-bootstrap/buttons';
import { MemberCardComponent } from '../members/member-card/member-card.component';
import { PaginationModule } from 'ngx-bootstrap/pagination';

@Component({
  selector: 'app-lists',
  imports: [FormsModule,ButtonsModule,MemberCardComponent,PaginationModule],
  templateUrl: './lists.component.html',
  styleUrl: './lists.component.css'
})
export class ListsComponent  implements OnInit,OnDestroy {
  likesService = inject (LikesService)
  predicate='likes';
  pageNumber = 1;
  pageSize = 5;

  ngOnInit(): void {
    this.loadLikes();
  }

  getTitle(){
    switch(this.predicate){
      case 'liked' : return 'Members you like';
      case 'likedBy' : return 'Mmbers that like you';
      default: return 'Mutual'
    }
  }
  loadLikes(){
    this.likesService.getLikes(this.predicate,this.pageNumber,this.pageSize);
  }

  pageChanged(event:any){
    if(this.pageNumber != event.pageNumber){
      this.pageNumber = event.pageNumber
      this.loadLikes();
    }

  }

  ngOnDestroy(): void {
    this.likesService.paginatedResult.set(null);
  }
}
