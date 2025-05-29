
import { Component, HostListener, inject, OnInit, ViewChild } from '@angular/core';
import { AccountService } from '../../_services/account.service';
import { MembersService } from '../../_services/members.service';
import { Member } from '../../_models/member';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { FormsModule, NgForm, NgModel } from '@angular/forms';
import { Toast, ToastrService } from 'ngx-toastr';
import { PhotoEditorComponent } from "../photo-editor/photo-editor.component";

@Component({
  selector: 'app-member-edit',
  imports: [TabsModule, FormsModule, PhotoEditorComponent],
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  @ViewChild('editForm') editForm?:NgForm;
  @HostListener('window:beforeunload',['$event']) notify($event:any){
    if(this.editForm?.dirty){
      $event.returnValue = true;
    }
  }

  member?: Member;
  username?: string;

  private accountService = inject(AccountService);
  private memberService = inject(MembersService);
  private toastr = inject(ToastrService)

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember() {
    const user = this.accountService.currentUser();
    console.log("Current User Signal:", user); // Debugging

    if (!user) {
      console.error("No logged-in user found.");
      return;
    }
    
    // The User object has 'username', not 'userName'
    this.username = user.username;
    console.log("Loaded Username:", this.username);

    if (!this.username) {
      console.error("No username found for the user.");
      return;
    }

    this.memberService.getMember(this.username).subscribe({
      next: (member) => {
        this.member = member;
        console.log("Member loaded:", this.member);
      },
      error: (error) => console.error("Failed to load member:", error)
    });
    }

    updateMember(){
      this.memberService.updateMember(this.editForm?.value).subscribe({
        next: _ => {
          this.toastr.success("Updated good")
          this.editForm?.reset(this.member);
        }
      });
    }

    onMemberChange(event: Member){
      this.member = event;
    }

  
  
}