import { ComponentFixture, TestBed } from '@angular/core/testing';

import { KetQuaLamViecCaNhanComponent } from './ket-qua-lam-viec-ca-nhan.component';

describe('KetQuaLamViecCaNhanComponent', () => {
  let component: KetQuaLamViecCaNhanComponent;
  let fixture: ComponentFixture<KetQuaLamViecCaNhanComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ KetQuaLamViecCaNhanComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(KetQuaLamViecCaNhanComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
