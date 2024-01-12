import { Injectable } from '@angular/core';
// import { ClassicEditor } from '../../assets/ckeditor5/ckeditor';
import { InlineEditor } from '../../assets/ckeditor5/ckeditor';

@Injectable({
  providedIn: 'root',
})

export class CkeditorServer {
  public Editor = InlineEditor;

  constructor() { }

  public ConfigHeader() {
    // let header = [];
    // return header = [
    // "heading",
    // "imageUpload",
    // "mediaEmbed", "insertTable"
    // "undo", "redo", "bold", "italic", "underline", "fontsize", "fontcolor", "fontbackgroundcolor", "horizontalline", 'alignment',
    // "bulletedList", "numberedList", "indent", "outdent", "blockQuote", "link",
    // ];
  }

}
