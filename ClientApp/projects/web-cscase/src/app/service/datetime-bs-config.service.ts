import { Injectable } from '@angular/core';
import { BsDatepickerConfig } from 'ngx-bootstrap/datepicker';

@Injectable({
  providedIn: 'root'
})
export class DatetimeBsConfigService {

  constructor() { }

  public bsConfig: Partial<BsDatepickerConfig> = {
    dateInputFormat: 'DD/MM/YYYY',
    minDate: new Date('01/01/1970'),
    isAnimated: false,
    showWeekNumbers: false,
    selectFromOtherMonth: true,
    containerClass: 'theme-default',
  };

  public bsConfigHour: Partial<BsDatepickerConfig> = {
    dateInputFormat: 'DD/MM/YYYY HH:mm',
    minDate: new Date('01/01/1970 00:00:00'),
    isAnimated: false,
    showWeekNumbers: false,
    selectFromOtherMonth: true,
    containerClass: 'theme-default',
  };

  public bsConfigYear: Partial<BsDatepickerConfig> = {
    dateInputFormat: 'YYYY',
    minDate: new Date('01/01/1970'),
    minMode: 'year',
    isAnimated: false,
    showWeekNumbers: false,
    selectFromOtherMonth: true,
    containerClass: 'theme-default',
  };
}
