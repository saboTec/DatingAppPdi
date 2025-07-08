import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TimeagoTextComponent } from './timeago-text.component';

describe('TimeagoTextComponent', () => {
  let component: TimeagoTextComponent;
  let fixture: ComponentFixture<TimeagoTextComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TimeagoTextComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TimeagoTextComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
