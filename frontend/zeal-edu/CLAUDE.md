```
 ============================================================
 Project   : EProject 3 - Zeal Education
 Author    : Lê Chính Trực - Student1557161 (C2403L0751)
 Email     : letruc.work@gmail.com - truc.lc.2427@aptechlearning.edu.vn
 Created   : 2026-19-04
 Course    : ADSE - Aptech Vietnam (https://aptechvietnam.com.vn)
 License   : All rights reserved. Unauthorized use prohibited.
 ============================================================
```

You are an expert in TypeScript, Angular, and scalable web application development. You write functional, maintainable, performant, and accessible code following Angular and TypeScript best practices.

## Language for UI text

**ALL user-facing text in the frontend MUST be in English** — including:

- Labels, placeholders, button text, headings, table column titles
- Toast messages, dialog titles/content, validation/error messages
- Empty-state copy, tooltips, aria-labels
- Any string rendered into the DOM or shown via `toast.*`, `MatSnackBar`, etc.

Do NOT use Vietnamese (with or without diacritics) in user-facing text. Code comments and internal docs (CLAUDE.md...) may still use Vietnamese.

```html
<!-- OK -->
<button hlmBtn>Save</button>
<p>No records found.</p>

<!-- NOT OK -->
<button hlmBtn>Luu</button>
<p>Khong co du lieu.</p>
```

## TypeScript Best Practices

- Use strict type checking
- Prefer type inference when the type is obvious
- Avoid the `any` type; use `unknown` when type is uncertain

## Import Path Aliases

Use path aliases instead of relative `../` paths. Aliases are configured in `tsconfig.json`:

- `@app/*` → `src/app/*`
- `@core/*` → `src/app/core/*`
- `@features/*` → `src/app/features/*`
- `@shared/*` → `src/app/shared/*`
- `@environments/*` → `src/environments/*`
- `@/*` → `src/*` (fallback for paths outside the above)

Rules:

- Prefer the most specific alias (e.g. use `@core/auth/auth-service`, not `@app/core/auth/auth-service`).
- Use aliases for any cross-folder import (anything that would otherwise need `../`).
- Keep `./` for sibling files in the same folder or local subfolders within the same feature module (e.g. `./login/login` inside `auth.routes.ts`).
- Aliases also work in dynamic imports: `import('@features/auth/auth.routes')`.

## Angular Best Practices

- Always use standalone components over NgModules
- Must NOT set `standalone: true` inside Angular decorators. It's the default in Angular v20+.
- Use signals for state management
- Implement lazy loading for feature routes
- Do NOT use the `@HostBinding` and `@HostListener` decorators. Put host bindings inside the `host` object of the `@Component` or `@Directive` decorator instead
- Use `NgOptimizedImage` for all static images.
  - `NgOptimizedImage` does not work for inline base64 images.

## Accessibility Requirements

- It MUST pass all AXE checks.
- It MUST follow all WCAG AA minimums, including focus management, color contrast, and ARIA attributes.

### Components

- Keep components small and focused on a single responsibility
- Use `input()` and `output()` functions instead of decorators
- Use `computed()` for derived state
- Set `changeDetection: ChangeDetectionStrategy.OnPush` in `@Component` decorator
- Prefer inline templates for small components
- Prefer Reactive forms instead of Template-driven ones
- Do NOT use `ngClass`, use `class` bindings instead
- Do NOT use `ngStyle`, use `style` bindings instead
- When using external templates/styles, use paths relative to the component TS file.

## State Management

- Use signals for local component state
- Use `computed()` for derived state
- Keep state transformations pure and predictable
- Do NOT use `mutate` on signals, use `update` or `set` instead

## Templates

- Keep templates simple and avoid complex logic
- Use native control flow (`@if`, `@for`, `@switch`) instead of `*ngIf`, `*ngFor`, `*ngSwitch`
- Use the async pipe to handle observables
- Do not assume globals like (`new Date()`) are available.

## Shared Pipes

- For VND currency formatting, use the `vnd` pipe from `@shared/pipes/vnd-pipe` — do NOT repeat `currency: 'VND' : 'symbol' : '1.0-0' : 'vi-VN'`.
  - Template: `{{ value | vnd }}`
  - Component: `import { VndPipe } from '@shared/pipes/vnd-pipe';` and add `VndPipe` to `imports`.
  - To change the default VND format (digits, symbol, locale), edit `src/app/shared/pipes/vnd-pipe.ts` only.

## Services

- Design services around a single responsibility
- Use the `providedIn: 'root'` option for singleton services
- Use the `inject()` function instead of constructor injection

## Form Dialogs

Dialogs that emit a `submitted` event for the parent to mutate MUST surface the parent's pending state — never own a local `submitting` signal that nothing writes to.

- Declare `readonly submitting = input<boolean>(false)` on the dialog component (do NOT use `signal(false)` for this — the parent controls it).
- Parent binds the matching mutation: `<app-x-dialog [submitting]="someMutation.isPending()" (submitted)="..." />`. For multi-mutation dialogs, OR the relevant `isPending()` flags.
- Submit button MUST be disabled and show a spinner while pending. Import `HlmSpinnerImports` from `@spartan-ng/helm/spinner` and follow this pattern:
  ```html
  <button hlmBtn type="submit" [disabled]="form.invalid || submitting()">
    @if (submitting()) {
    <hlm-spinner class="mr-2" />
    } {{ submitting() ? 'Saving...' : 'Save' }}
  </button>
  ```
- Guard the submit handler too: `if (this.submitting()) return;` at the top of `submit()` to prevent re-entry from rapid clicks.
