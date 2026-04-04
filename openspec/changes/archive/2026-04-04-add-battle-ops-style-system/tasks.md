## 1. Repository style guidance

- [x] 1.1 Create a Battle Ops style skill under `.github/skills` that captures the approved palette, typography, logo rules, PrimeNG guidance, and tactical microcopy
- [x] 1.2 Reference the style skill and any shared brand assets/conventions so future UI work can apply the same system consistently

## 2. Theme foundation and brand assets

- [x] 2.1 Replace scaffold-level Angular styling with Battle Ops design tokens and global theme variables in the app
- [x] 2.2 Add Battle Ops typography and define the approved token mappings for surfaces, borders, accents, warnings, and critical states
- [x] 2.3 Add the supported Battle Ops logo variants and wire the correct variant usage for hero, minimal, icon, and monochrome surfaces

## 3. Application styling and settings

- [x] 3.1 Apply the Battle Ops theme to the primary app shell and core UI surfaces so the application no longer renders with default scaffold styling
- [x] 3.2 Add the Battle Ops styling layer or scaffolding for supported PrimeNG components such as buttons, panels, dialogs, tables, and inputs
- [x] 3.3 Add persisted style settings for supported Battle Ops-compatible preferences such as reduced motion and any approved presentation options
- [x] 3.4 Ensure reduced-motion mode preserves the Battle Ops identity while suppressing non-essential animation effects

## 4. Verification

- [x] 4.1 Add or update automated tests for theme/settings behavior that can be validated in the Angular app
- [x] 4.2 Validate the OpenSpec change and run the relevant repository checks after implementation
