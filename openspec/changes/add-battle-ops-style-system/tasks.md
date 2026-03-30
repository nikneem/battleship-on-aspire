## 1. Repository style guidance

- [ ] 1.1 Create a Battle Ops style skill under `.github/skills` that captures the approved palette, typography, logo rules, PrimeNG guidance, and tactical microcopy
- [ ] 1.2 Reference the style skill and any shared brand assets/conventions so future UI work can apply the same system consistently

## 2. Theme foundation and brand assets

- [ ] 2.1 Replace scaffold-level Angular styling with Battle Ops design tokens and global theme variables in the app
- [ ] 2.2 Add Battle Ops typography and define the approved token mappings for surfaces, borders, accents, warnings, and critical states
- [ ] 2.3 Add the supported Battle Ops logo variants and wire the correct variant usage for hero, minimal, icon, and monochrome surfaces

## 3. Application styling and settings

- [ ] 3.1 Apply the Battle Ops theme to the primary app shell and core UI surfaces so the application no longer renders with default scaffold styling
- [ ] 3.2 Add the Battle Ops styling layer or scaffolding for supported PrimeNG components such as buttons, panels, dialogs, tables, and inputs
- [ ] 3.3 Add persisted style settings for supported Battle Ops-compatible preferences such as reduced motion and any approved presentation options
- [ ] 3.4 Ensure reduced-motion mode preserves the Battle Ops identity while suppressing non-essential animation effects

## 4. Verification

- [ ] 4.1 Add or update automated tests for theme/settings behavior that can be validated in the Angular app
- [ ] 4.2 Validate the OpenSpec change and run the relevant repository checks after implementation
