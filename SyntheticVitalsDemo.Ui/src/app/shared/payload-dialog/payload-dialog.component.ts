import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

export interface PayloadDialogData {
  title: string;
  payload: string;
  fileName: string;
  contentType: string;
}

@Component({
  selector: 'app-payload-dialog',
  imports: [CommonModule, MatButtonModule, MatDialogModule],
  templateUrl: './payload-dialog.component.html'
})
export class PayloadDialogComponent {
  constructor(@Inject(MAT_DIALOG_DATA) public readonly data: PayloadDialogData) {}

  exportPayload(): void {
    const blob = new Blob([this.data.payload], { type: this.data.contentType });
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = this.data.fileName;
    anchor.click();
    URL.revokeObjectURL(url);
  }
}
