import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SafeHtmlPipe } from './safe-html.pipe';
import { HighlightPipe } from './highlight.pipe';

@NgModule({
  declarations: [SafeHtmlPipe, HighlightPipe],
  imports: [CommonModule],
  exports: [SafeHtmlPipe, HighlightPipe]
})
export class SharedModule { }
