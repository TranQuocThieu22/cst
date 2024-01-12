import { Injectable } from '@angular/core';
import { DataPhanHe, DataTruong } from '@mylibs';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})

export class DataServices {
  public BieuDo: BehaviorSubject<boolean> = new BehaviorSubject(false);
  public LoaiBieuDo: BehaviorSubject<number> = new BehaviorSubject(1);

  public IsData: BehaviorSubject<boolean> = new BehaviorSubject(false);
}

