import { ChangeDetectionStrategy, Component, forwardRef, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { provideIcons } from '@ng-icons/core';
import { lucideStar } from '@ng-icons/lucide';
import { HlmIconImports } from '@spartan-ng/helm/icon';

@Component({
  selector: 'app-rating-input',
  imports: [HlmIconImports],
  providers: [
    provideIcons({ lucideStar }),
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => RatingInput),
      multi: true,
    },
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { role: 'radiogroup', 'aria-label': 'Rating' },
  template: `
    <div class="flex items-center gap-1">
      @for (star of stars; track star) {
        <button
          type="button"
          role="radio"
          [attr.aria-checked]="value() === star"
          [attr.aria-label]="star + ' star' + (star > 1 ? 's' : '')"
          class="rounded p-1 transition hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring disabled:cursor-not-allowed disabled:opacity-50"
          [disabled]="disabled()"
          (click)="select(star)"
        >
          <ng-icon
            hlm
            name="lucideStar"
            size="sm"
            [class]="star <= value() ? 'text-amber-400' : 'text-muted-foreground'"
          />
        </button>
      }
    </div>
  `,
})
export class RatingInput implements ControlValueAccessor {
  protected readonly stars = [1, 2, 3, 4, 5];
  protected readonly value = signal<number>(0);
  protected readonly disabled = signal<boolean>(false);

  private _onChange: (value: number) => void = () => {};
  private _onTouched: () => void = () => {};

  writeValue(value: number | null): void {
    this.value.set(value ?? 0);
  }

  registerOnChange(fn: (value: number) => void): void {
    this._onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this._onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  protected select(star: number): void {
    if (this.disabled()) return;
    this.value.set(star);
    this._onChange(star);
    this._onTouched();
  }
}
