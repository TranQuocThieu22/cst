import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common'; // <-- PHẢI CÓ CHO 'date' pipe
import { FormsModule } from '@angular/forms';

// --- CÁC MODULE BẠN ĐÃ CÓ ---
import { CheckboxModule } from 'primeng/checkbox';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { DialogModule } from 'primeng/dialog';
import { FieldsetModule } from 'primeng/fieldset';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { ButtonModule } from 'primeng/button';
import { KeyFilterModule } from 'primeng/keyfilter';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { DanhSachTruongComponent } from './danh-sach-truong.component';
import { TableModule } from 'primeng/table';

@NgModule({
    declarations: [
        DanhSachTruongComponent
    ],
    imports: [
        TableModule,
        CommonModule,
        FormsModule,
        CheckboxModule,
        InputTextModule,
        ProgressSpinnerModule,
        DialogModule,
        FieldsetModule,
        DropdownModule,
        CalendarModule,
        ButtonModule,
        InputTextareaModule,
        InputNumberModule,
        KeyFilterModule,
    ],
    exports: [
        DanhSachTruongComponent
    ]
})
export class DanhSachTruongModule { }