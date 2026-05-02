import { afterNextRender, DestroyRef, Directive, ElementRef, inject } from '@angular/core';

@Directive({
  selector: '[appAutoSizeAccordionContent]',
})
export class AutoSizeAccordionContent {
  private readonly _host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly _destroyRef = inject(DestroyRef);

  constructor() {
    afterNextRender(() => {
      const host = this._host.nativeElement;
      const inner = host.firstElementChild as HTMLElement | null;
      if (!inner || typeof ResizeObserver === 'undefined') return;

      const observer = new ResizeObserver((entries) => {
        for (const entry of entries) {
          const height = Math.ceil(entry.contentRect.height);
          host.style.setProperty('--brn-accordion-content-height', `${height}px`);
        }
      });
      observer.observe(inner);
      this._destroyRef.onDestroy(() => observer.disconnect());
    });
  }
}
