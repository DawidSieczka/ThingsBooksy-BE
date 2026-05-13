# angular-styling.md

## Rule

All styling in ThingsBooksy uses SCSS. Design tokens (colours, spacing, typography, breakpoints) are defined once as CSS custom properties in `frontend/src/styles/_tokens.scss` and consumed everywhere via `var(--token-name)`. Component styles are always scoped to the component's own `.scss` file. Global stylesheets may not contain component-specific rules.

---

## Token file — `_tokens.scss`

All CSS custom properties are declared on `:root` in `_tokens.scss`. The file is imported once in `styles.scss` and is therefore available globally — including inside component `.scss` files.

```scss
// frontend/src/styles/_tokens.scss

:root {
  // Colours
  --color-primary:        #1a6dff;
  --color-primary-hover:  #1458d6;
  --color-danger:         #d93025;
  --color-success:        #1e8a44;
  --color-neutral-50:     #f8f9fa;
  --color-neutral-100:    #f1f3f4;
  --color-neutral-300:    #dadce0;
  --color-neutral-700:    #3c4043;
  --color-neutral-900:    #202124;
  --color-surface:        #ffffff;
  --color-on-surface:     var(--color-neutral-900);

  // Typography
  --font-family-base:     'Inter', system-ui, sans-serif;
  --font-size-sm:         0.875rem;  // 14px
  --font-size-md:         1rem;      // 16px
  --font-size-lg:         1.125rem;  // 18px
  --font-size-xl:         1.25rem;   // 20px
  --font-size-2xl:        1.5rem;    // 24px
  --font-weight-regular:  400;
  --font-weight-medium:   500;
  --font-weight-bold:     700;
  --line-height-base:     1.5;

  // Spacing (4-point scale)
  --space-1:  0.25rem;   // 4px
  --space-2:  0.5rem;    // 8px
  --space-3:  0.75rem;   // 12px
  --space-4:  1rem;      // 16px
  --space-6:  1.5rem;    // 24px
  --space-8:  2rem;      // 32px
  --space-12: 3rem;      // 48px
  --space-16: 4rem;      // 64px

  // Border radius
  --radius-sm: 0.25rem;   // 4px
  --radius-md: 0.5rem;    // 8px
  --radius-lg: 0.75rem;   // 12px
  --radius-full: 9999px;

  // Shadows
  --shadow-sm: 0 1px 2px rgba(0, 0, 0, 0.08);
  --shadow-md: 0 2px 8px rgba(0, 0, 0, 0.12);
  --shadow-lg: 0 4px 16px rgba(0, 0, 0, 0.16);

  // Transitions
  --transition-fast:   150ms ease;
  --transition-base:   250ms ease;
  --transition-slow:   400ms ease;

  // Breakpoints (read-only reference — use in SCSS @media, not in CSS var())
  // --breakpoint-sm: 576px
  // --breakpoint-md: 768px
  // --breakpoint-lg: 1024px
  // --breakpoint-xl: 1280px
}
```

Rules:
- Every colour, spacing value, font size, and transition used in the project must have a token. Never hard-code raw values like `#1a6dff` or `16px` in component stylesheets.
- Token names follow `--{category}-{variant}` kebab-case. Do not abbreviate categories (`--clr-` is forbidden; use `--color-`).
- Breakpoints cannot be referenced via `var()` inside `@media` rules (browser limitation). Define them as SCSS variables in `_tokens.scss` and use those:
  ```scss
  $breakpoint-md: 768px;
  @media (min-width: $breakpoint-md) { ... }
  ```
- Adding a new token requires updating `_tokens.scss` only — all consumers update automatically.

---

## Global base file — `_base.scss`

`_base.scss` contains only application-wide resets and `body` defaults. It must not reference component-level classes.

```scss
// frontend/src/styles/_base.scss

*,
*::before,
*::after {
  box-sizing: border-box;
  margin: 0;
  padding: 0;
}

body {
  font-family: var(--font-family-base);
  font-size: var(--font-size-md);
  line-height: var(--line-height-base);
  color: var(--color-on-surface);
  background-color: var(--color-surface);
}

a {
  color: var(--color-primary);
  text-decoration: none;

  &:hover {
    text-decoration: underline;
  }
}
```

---

## Entry point — `styles.scss`

```scss
// frontend/src/styles.scss

@use 'styles/tokens' as *;
@use 'styles/base' as *;
```

Rules:
- Import `_tokens.scss` before `_base.scss` so base styles can use token variables.
- Do not import component-specific styles here — Angular handles that through component metadata.

---

## Component styles — scoping

Every component has its own `.scss` file referenced via `styleUrl` in `@Component`. Angular's default view encapsulation (`ViewEncapsulation.Emulated`) is always used — never `None` or `ShadowDom`.

```typescript
@Component({
  selector: 'tb-resource-card',
  standalone: true,
  templateUrl: './resource-card.component.html',
  styleUrl: './resource-card.component.scss',
})
export class ResourceCardComponent { ... }
```

Component stylesheet — use tokens, not raw values:

```scss
// features/resources/list/resource-card/resource-card.component.scss

.resource-card {
  background-color: var(--color-surface);
  border: 1px solid var(--color-neutral-300);
  border-radius: var(--radius-md);
  padding: var(--space-4);
  box-shadow: var(--shadow-sm);
  transition: box-shadow var(--transition-fast);

  &:hover {
    box-shadow: var(--shadow-md);
  }

  &__title {
    font-size: var(--font-size-lg);
    font-weight: var(--font-weight-medium);
    color: var(--color-neutral-900);
    margin-bottom: var(--space-2);
  }

  &__description {
    font-size: var(--font-size-sm);
    color: var(--color-neutral-700);
    line-height: var(--line-height-base);
  }
}
```

Rules:
- Use BEM-style class naming (`.block__element--modifier`) for component internals. Nesting in SCSS is allowed for `&__element` and `&--modifier` references.
- Never use `:host` overrides to escape component scope. Style the root element using the host selector only for layout concerns (e.g., `display`, `width`) that the parent needs to control:
  ```scss
  :host {
    display: block;      // layout hint for parent
  }
  ```
- Never use `::ng-deep`. If a third-party component must be styled, wrap it in a dedicated shared component.
- Do not use Angular-specific attribute selectors (`[_ngcontent-...]`) — they are generated and unstable.

---

## Animations

Use CSS transitions for simple state changes (hover, active, visibility). Use `@angular/animations` only for multi-step or route-level animations.

### CSS transitions (preferred for simple cases)

```scss
.button {
  background-color: var(--color-primary);
  transition: background-color var(--transition-fast), box-shadow var(--transition-fast);

  &:hover {
    background-color: var(--color-primary-hover);
    box-shadow: var(--shadow-sm);
  }
}
```

### Angular animations (multi-step or route transitions)

```typescript
import { trigger, transition, style, animate, state } from '@angular/animations';

export const fadeIn = trigger('fadeIn', [
  transition(':enter', [
    style({ opacity: 0, transform: 'translateY(-4px)' }),
    animate('{{duration}} ease', style({ opacity: 1, transform: 'translateY(0)' })),
  ], { params: { duration: '250ms' } }),
  transition(':leave', [
    animate('150ms ease', style({ opacity: 0 })),
  ]),
]);
```

Rules:
- Always use token-defined durations in CSS transitions (`var(--transition-fast)`, `var(--transition-base)`, `var(--transition-slow)`).
- Reusable Angular animation definitions live in `shared/animations/`.
- Never animate `width`, `height`, or `top`/`left` directly — prefer `transform` and `opacity` for GPU-composited performance.
- Respect `prefers-reduced-motion`:
  ```scss
  @media (prefers-reduced-motion: reduce) {
    *,
    *::before,
    *::after {
      animation-duration: 0.01ms !important;
      transition-duration: 0.01ms !important;
    }
  }
  ```
  Add this rule to `_base.scss`.

---

## Responsiveness

Use mobile-first SCSS with `min-width` breakpoints. Breakpoints are defined as SCSS variables in `_tokens.scss`.

```scss
// _tokens.scss (SCSS variable section, below the :root block)
$breakpoint-sm: 576px;
$breakpoint-md: 768px;
$breakpoint-lg: 1024px;
$breakpoint-xl: 1280px;
```

Usage in a component:

```scss
@use 'styles/tokens' as *;

.resource-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--space-4);

  @media (min-width: $breakpoint-md) {
    grid-template-columns: repeat(2, 1fr);
  }

  @media (min-width: $breakpoint-lg) {
    grid-template-columns: repeat(3, 1fr);
  }
}
```

Rules:
- Always write mobile styles first, then override with `min-width` media queries.
- Never use `max-width` media queries — they conflict with the mobile-first approach.
- Never hard-code pixel breakpoint values in component stylesheets — always use SCSS variables.
- Component stylesheets that need breakpoint variables must `@use 'styles/tokens' as *` at the top.

---

## Utility classes

Utility classes are permitted only for layout primitives that are too generic to belong to any component. Define them in `_base.scss` under a clearly delimited section.

```scss
// _base.scss — utility classes (keep this section minimal)

.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

.sr-only { @extend .visually-hidden; }
```

Rules:
- Utility classes are the exception, not the rule. Prefer component-scoped classes.
- No Tailwind-style atomic class proliferation (`mt-4`, `text-lg`, etc.) — use tokens in component stylesheets.
- Any utility class added to `_base.scss` must have a comment explaining its purpose.

---

## Zakazy — zestawienie

| Zakaz | Powód |
|---|---|
| Raw wartości kolorów/spacing w komponentach (`#1a6dff`, `16px`) | Używaj tokenów `var(--color-*)`, `var(--space-*)` |
| `::ng-deep` | Przebija enkapsulację; użyj shared komponentu opakowującego |
| `ViewEncapsulation.None` lub `ViewEncapsulation.ShadowDom` | Zawsze `Emulated` (domyślne) |
| Reguły komponentowe w `_base.scss` lub `styles.scss` | Style komponentu należą do `.component.scss` |
| Hard-kodowane breakpointy w komponentach (`@media (min-width: 768px)`) | Używaj `$breakpoint-md` z `_tokens.scss` |
| `max-width` media queries | Mobile-first — tylko `min-width` |
| Animacja `width`/`height`/`top`/`left` | Używaj `transform` + `opacity` (GPU compositing) |
| Brakujący `prefers-reduced-motion` w globalnych animacjach | Dostępność — dodaj do `_base.scss` |
| Nowe tokeny zdefiniowane w plikach komponentów | Tokeny tylko w `_tokens.scss` |
| Klas narzędziowych w stylu Tailwind | Używaj tokenów w stylach komponentów |
