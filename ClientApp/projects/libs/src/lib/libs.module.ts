import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LibsComponent, } from './libs.component';
import { FormsModule } from '@angular/forms';
import { NgxEditorModule } from 'ngx-editor';
@NgModule({
  providers: [],
  declarations: [LibsComponent],
  imports: [FormsModule, CommonModule, NgxEditorModule],
  exports: [LibsComponent]
})

export class LibsModule {
}

