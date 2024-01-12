import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})

export class TransTextService {

    constructor() { }

    public transtext(value: string): string {
        const ext: string = value.substring(value.lastIndexOf('.') + 1, value.length).toLowerCase();
        let newvalue: string = value.replace('.' + ext, '');
        if (value.length <= 30) {
          return value;
        }
        newvalue = newvalue.substring(0, 30) + (value.length > 30 ? '...' : '');
        return newvalue + '.' + ext;
      }
}
