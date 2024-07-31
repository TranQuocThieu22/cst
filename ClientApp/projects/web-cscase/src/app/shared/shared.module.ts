import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SafeHtmlPipe } from './safe-html.pipe';
import { HighlightPipe } from './highlight.pipe';
import { OrderByPipe } from './order-by.pipe';

@NgModule({
  declarations: [SafeHtmlPipe, HighlightPipe, OrderByPipe],
  imports: [CommonModule],
  exports: [SafeHtmlPipe, HighlightPipe, OrderByPipe]
})
export class SharedModule { }
