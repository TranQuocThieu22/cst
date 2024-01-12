import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LibsComponent, } from './libs.component';
import { FormsModule } from '@angular/forms';

@NgModule({
  providers: [],
  declarations: [LibsComponent],
  imports: [FormsModule, CommonModule],
  exports: [LibsComponent]
})

export class LibsModule {
}

