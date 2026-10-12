# VISUAL.md — appearance teardown

This file describes **appearance** only: what the SmartThings air-conditioner screen looks like
and how to translate it to WinUI 3. It does not set work order — ROADMAP.md does, following the
main path. Reference screenshots, when provided, live in `docs/reference/`.

## Spirit of the SmartThings AC screen

- One vertical scroll. Device name on top, state right below it.
- A hero area dominated by the target temperature in very large thin digits, with round `−`/`+`
  buttons on each side and a big round power button.
- One line with indoor temperature and humidity, small and secondary.
- Below, rounded cards, one per control group, each with a title and a row of icon tiles with a
  caption under each icon. The selected tile is filled with the accent color.
- Color carries state: blue when cooling, other tints per mode, everything grey and muted when
  the unit is off.
- Lots of padding, few borders, soft shadows. Calm and readable at a glance.

## WinUI translation

| SmartThings | WinUI 3 |
|---|---|
| App background | `MicaBackdrop` on the window; content on transparent background |
| Header | custom title bar (`ExtendsContentIntoTitleBar`) with app icon and device label |
| Big temperature | `TextBlock`, `Segoe UI Variable Display`, Light weight, ~96 px, `°C` smaller beside it |
| Round −/+ and power | `Button` with `CornerRadius` = half of size, `FontIcon` glyphs from Segoe Fluent Icons |
| Cards | `Border` with `CardBackgroundFillColorDefaultBrush`, `CardStrokeColorDefaultBrush`, `CornerRadius="8"`, padding 16 |
| Icon tiles | `ItemsView` / `GridView` with single selection, item template = icon + caption; selected uses `AccentFillColorDefaultBrush` |
| Toggles (sound, light, auto clean) | `ToggleSwitch` inside a `SettingsCard`-like row (icon, title, description, control) |
| Errors | `InfoBar` at the top of the scroll |
| Settings | separate page, rows in the Windows 11 Settings style |

## Color by state

| State | Accent |
|---|---|
| cool | blue (`#3B82F6` light / `#60A5FA` dark) |
| dry | teal (`#14B8A6` / `#2DD4BF`) |
| wind | green (`#22C55E` / `#4ADE80`) |
| auto | purple (`#8B5CF6` / `#A78BFA`) |
| off | `TextFillColorDisabledBrush`, tiles disabled |

## Icons (Segoe Fluent Icons, to confirm visually during Phase 2)

Power `E7E8`, minus `E738`, plus `E710`, snowflake for cool (custom path if no glyph), drop for dry,
fan/wind for wind, sync/auto for auto, settings `E713`. Phase 2 picks final glyphs and records them
here.

## Layout order (top to bottom)

1. Title bar: icon, "WindFree Control", settings button.
2. Hero: device label, state text, target temperature with −/+, power button, indoor line.
3. Mode card.
4. Fan speed card.
5. Wind direction card.
6. Special modes card (WindFree first).
7. Energy card.
8. Comfort and options: tropical night, sound, display light, auto clean.
9. Maintenance: filter, self-check, device info.
