import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'highlight' })

export class HighlightPipe implements PipeTransform {
  // highlight when search
  transform(value: any, args: any): any {
    if (value === null || value === 'null' || value === '' || !value) {
        return value = '';
    }
    if (value !== null && value !== 'null' && value !== '') {
        const reText = new RegExp(args, 'gi');
        return value.replace(reText, '<mark>$&</mark>');
    }
  }
}
