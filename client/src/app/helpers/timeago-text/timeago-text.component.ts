import { Component, Input } from '@angular/core';
import { TimeagoModule } from 'ngx-timeago';
import { CommonModule } from '@angular/common';

@Component({
  standalone: true,
  selector: 'app-timeago-text',
  imports: [CommonModule, TimeagoModule],
  template: `<span>{{ value | timeago }}</span>`
})
export class TimeagoTextComponent {
  @Input() value!: string | Date;
}