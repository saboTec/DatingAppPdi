import { Component, inject, OnInit } from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { MembersService } from '../../_services/members.service';
import { Member } from '../../_models/member';
import { retry } from 'rxjs';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-member-edit',
  imports: [],
  templateUrl: './member-edit.component.html',
  styleUrl: './member-edit.component.css'
})
export class MemberEditComponent implements OnInit {
  private route = inject(ActivatedRoute);
  member?: Member;

  private accountService = inject(AccountService);
  private memberService = inject(MembersService);

  ngOnInit(): void {
      this.loadMember();
  }

  // loadMember(){
  //   const user = this.accountService.currentUser();
  //   if(!user) return;
  //   console.log("User before:",user);
    
  //   this.memberService.getMember(user.username).subscribe({
  //     next: member => this.member = member,
  //     complete: () => console.log("Member loaded is:",this.member?.userName),
  //     error:error => console.log("No member loaded")
      
  //     })
  //   }

    loadMember(){
      var username = this.route.snapshot.paramMap.get('username');
      if(!username) return;
      this.memberService.getMember(username).subscribe({
        next : member => {
          this.member = member;
        },
        error : error => console.log(error),
        complete : () => console.log('Member loaded sabo',this.member?.age)
      })
    }
}
