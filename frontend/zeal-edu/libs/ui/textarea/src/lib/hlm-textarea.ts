import { Directive, input, linkedSignal } from '@angular/core';
import { BrnFieldControlDescribedBy } from '@spartan-ng/brain/field';
import { BrnTextarea } from '@spartan-ng/brain/textarea';
import { classes } from '@spartan-ng/helm/utils';
import { cva, type VariantProps } from 'class-variance-authority';

export const textareaVariants = cva(
  'placeholder:text-muted-foreground dark:bg-input/30 border-input focus-visible:border-ring focus-visible:ring-ring/50 flex min-h-20 w-full rounded-md border bg-transparent px-3 py-2 text-base shadow-xs transition-[color,box-shadow] outline-none focus-visible:ring-[3px] disabled:pointer-events-none disabled:cursor-not-allowed disabled:opacity-50 md:text-sm',
  {
    variants: {
      error: {
        auto: 'data-[matches-spartan-invalid=true]:border-destructive data-[matches-spartan-invalid=true]:ring-destructive/20 dark:data-[matches-spartan-invalid=true]:ring-destructive/40',
        true: 'border-destructive focus-visible:border-destructive focus-visible:ring-destructive/20 dark:focus-visible:ring-destructive/40',
      },
    },
    defaultVariants: {
      error: 'auto',
    },
  },
);
type TextareaVariants = VariantProps<typeof textareaVariants>;

@Directive({
  selector: '[hlmTextarea]',
  hostDirectives: [{ directive: BrnTextarea, inputs: ['id'] }, BrnFieldControlDescribedBy],
})
export class HlmTextarea {
  public readonly error = input<TextareaVariants['error']>('auto');

  protected readonly _state = linkedSignal(() => ({ error: this.error() }));

  constructor() {
    classes(() => textareaVariants({ error: this._state().error }));
  }
}
