import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NhanSuAqComponent } from './nhan-su-aq.component';

describe('NhanSuAqComponent', () => {
  let component: NhanSuAqComponent;
  let fixture: ComponentFixture<NhanSuAqComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ NhanSuAqComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NhanSuAqComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
