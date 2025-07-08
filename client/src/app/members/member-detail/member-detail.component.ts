import { Component, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
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
import { PresenceService } from '../../_services/presence.service';
import { AccountService } from '../../_services/account.service';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';



@Component({
  selector: 'app-member-detail',
  imports: [TabsModule, GalleryModule, TimeagoModule, DatePipe,
    NgIf, TimeagoTextComponent, MemberMessagesComponent],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit, OnDestroy {

  @ViewChild('memberTabs', { static: true }) memberTabs?: TabsetComponent;
  private messageService = inject(MessageService);
  private accountService = inject(AccountService);
  presenceService = inject(PresenceService);
  private route = inject(ActivatedRoute);
  private router = inject(Router)
  member: Member = {} as Member;
  images: GalleryItem[] = [];
  activeTab?: TabDirective;
  // messages: Message[] = [];



  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => {
        this.member = data['member'];
        this.member && this.member.photos.map(p => {
          this.images.push(new ImageItem({ src: p.url, thumb: p.url }))
        });
      }
    })
    this.route.paramMap.subscribe({
      next: _=> this.onRouteParamsChange()
    })
    
    this.route.queryParams.subscribe({
      next: params => {
        params['tab'] && this.selectTab(params['tab'])
      }
    })
  }



  selectTab(tabHeading: string) {
    const tab = this.memberTabs?.tabs.find(t => t.heading === tabHeading);
    if (tab) {
      setTimeout(() => tab.active = true); // needed because view might not be initialized yet
    }
  }

  onRouteParamsChange(){
    const user = this.accountService.currentUser();
    if(!user) return;
    if(this.messageService.hubConnection?.state == HubConnectionState.Connected &&
      this.activeTab?.heading === 'Messages'){

        this.messageService.hubConnection.stop().then(()=> {
          this.messageService.createHubConnection(user,this.member.userName);
        })
      }
    
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    this.router.navigate([],{
      relativeTo: this.route,
      queryParams: {tab: this.activeTab.heading}
    })
    if (this.activeTab.heading === 'Messages' && this.member) {
      const user = this.accountService.currentUser();
      if (!user) return;
      this.messageService.createHubConnection(user, this.member.userName);
    } else {
      this.messageService.stopHubConnection();
    }
  }

  ngOnDestroy(): void {
    this.messageService.stopHubConnection();
  }
}
