import { Directive, inject } from '@angular/core';
import { NgControl } from '@angular/forms';

const NAVIGATION_KEYS = new Set([
  'Backspace',
  'Delete',
  'ArrowLeft',
  'ArrowRight',
  'ArrowUp',
  'ArrowDown',
  'Tab',
  'Home',
  'End',
  'Enter',
  'Escape',
]);

@Directive({
  selector: 'input[appDigitsOnly]',
  host: {
    inputmode: 'numeric',
    '(keydown)': 'onKeyDown($event)',
    '(input)': 'onInput($event)',
    '(paste)': 'onPaste($event)',
  },
})
export class DigitsOnlyDirective {
  private readonly ngControl = inject(NgControl, { optional: true });

  protected onKeyDown(event: KeyboardEvent): void {
    if (NAVIGATION_KEYS.has(event.key)) return;
    if (event.ctrlKey || event.metaKey || event.altKey) return;
    if (event.key.length === 1 && !/^\d$/.test(event.key)) {
      event.preventDefault();
    }
  }

  protected onInput(event: Event): void {
    const el = event.target as HTMLInputElement;
    const sanitized = el.value.replace(/\D/g, '');
    if (sanitized === el.value) return;

    el.value = sanitized;
    if (this.ngControl?.control) {
      this.ngControl.control.setValue(sanitized, { emitEvent: false });
    }
  }

  protected onPaste(event: ClipboardEvent): void {
    const text = event.clipboardData?.getData('text') ?? '';
    if (/^\d*$/.test(text)) return;

    event.preventDefault();
    const el = event.target as HTMLInputElement;
    const start = el.selectionStart ?? el.value.length;
    const end = el.selectionEnd ?? el.value.length;
    const sanitized = text.replace(/\D/g, '');
    const next = el.value.slice(0, start) + sanitized + el.value.slice(end);

    el.value = next;
    if (this.ngControl?.control) {
      this.ngControl.control.setValue(next);
    } else {
      el.dispatchEvent(new Event('input', { bubbles: true }));
    }
  }
}
