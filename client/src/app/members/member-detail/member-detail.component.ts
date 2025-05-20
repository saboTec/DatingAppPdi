import { Component, inject, OnInit } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Member } from '../../_models/member';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery';
import { TimeagoIntl, TimeagoModule } from 'ngx-timeago';
import { DatePipe, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TimeagoTextComponent } from '../../helpers/timeago-text/timeago-text.component';



@Component({
  selector: 'app-member-detail',
  imports: [TabsModule,GalleryModule,TimeagoModule,DatePipe,NgIf,TimeagoTextComponent],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit{
  
  private memberService = inject(MembersService);
  private route = inject(ActivatedRoute);
  member? :Member;
  images : GalleryItem[] = [];
  
  ngOnInit(): void {
    this.loadMember();
  }

  loadMember(){
    var username = this.route.snapshot.paramMap.get('username');
    if(!username) return;
    this.memberService.getMember(username).subscribe({
      next : member => {
        this.member = member;
        member.photos.map(p => {
          this.images.push(new ImageItem({src:p.url,thumb:p.url}))
        })
      },
      error : error => console.log(error),
      complete : () => console.log('Member loaded',this.member?.age)
    })
  }

}
