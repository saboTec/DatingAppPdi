import { Component, inject, OnInit } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { MemberCardComponent } from "../member-card/member-card.component";
import { AccountService } from '../../_services/account.service';
import { UserParams } from '../../_models/userParams';
import { FormsModule, NgModel } from '@angular/forms';
import { ButtonsModule } from 'ngx-bootstrap/buttons';



@Component({
  selector: 'app-member-list',
  imports: [MemberCardComponent,PaginationModule,FormsModule,ButtonsModule],
  templateUrl: './member-list.component.html',
  styleUrl: './member-list.component.css'
})
export class MemberListComponent implements OnInit {
  private accountService = inject(AccountService)
  memberService = inject(MembersService);
  userParams = new UserParams(this.accountService.currentUser())
  genderList = [{value:'male',display:'Males'},{value:'female',display:'Females'}]

  ngOnInit(): void {
    if(!this.memberService.paginatedResult()) this.loadMembers();
  }
  loadMembers(){
    this.memberService.getMembers(this.userParams)
  }
  resetFilters(){
    this.userParams = new UserParams(this.accountService.currentUser());
    this.loadMembers();
  }

  pageChanged(event: any) {
    if(this.userParams.pageNumber! = event.page){
      this.userParams.pageNumber = event.page;
      this.loadMembers();
    }
  }
}
