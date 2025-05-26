import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Member } from '../../_models/member';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TimeagoIntl, TimeagoModule } from 'ngx-timeago';
import { DatePipe, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TimeagoTextComponent } from '../../helpers/timeago-text/timeago-text.component';
import { MemberMessagesComponent } from "../member-messages/member-messages.component";
import { Message } from '../../_models/message';
import { MessageService } from '../../_services/message.service';



@Component({
  selector: 'app-member-detail',
  imports: [TabsModule, GalleryModule, TimeagoModule, DatePipe,
    NgIf, TimeagoTextComponent, MemberMessagesComponent],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit {
  @ViewChild('memberTabs', { static: true }) memberTabs?: TabsetComponent;
  private messageService = inject(MessageService);
  private memberService = inject(MembersService);
  private route = inject(ActivatedRoute);
  member: Member = {} as Member;
  images: GalleryItem[] = [];
  activeTab?: TabDirective;
  messages: Message[] = [];



  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => {
        this.member = data['member'];
        this.member && this.member.photos.map(p => {
          this.images.push(new ImageItem({ src: p.url, thumb: p.url }))
        });
      }
    })
    this.route.queryParams.subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tab'])
      }
    })
  }

  onUpdateMessage(event:Message){
    this.messages.push(event);
  }

  // selectTab(heading:string){
  //   if(this.activeTab){
  //     const messageTab = this.memberTabs?.tabs.find(x=>x.heading ===heading);
  //     if(messageTab) {
  //       messageTab.active = true;
  //       this.onTabActivated(messageTab);
  //     }
  //   }
  // }
  selectTab(tabHeading: string) {
    const tab = this.memberTabs?.tabs.find(t => t.heading === tabHeading);
    if (tab) {
      setTimeout(() => tab.active = true); // needed because view might not be initialized yet
    }
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if (this.activeTab.heading === 'Messages' && this.messages.length === 0 && this.member) {
      this.messageService.getMessageThred(this.member.userName).subscribe({
        next: messages => this.messages = messages
      })
    }
  }
}
