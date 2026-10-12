# DEVICE.md — what this unit supports

Source: `tools/dump-device.ps1` run on 2026-10-12 (status + capability definitions).
Model: Samsung Room Air Conditioner, `ARA-WW-TP1-22-COMMON`, firmware `11260327`,
presentation `DA-AC-RAC-01001`, release year 2021.

Re-run the script after a firmware change and update this file.

## Controls (enabled, with values)

| Feature | Capability | Values on this unit | Now |
|---|---|---|---|
| Power | `switch` | on, off | on |
| Target temperature | `thermostatCoolingSetpoint` | 16-30 °C, step 1 | 25 |
| Mode | `airConditionerMode` | auto, cool, dry, wind | cool |
| Fan speed | `airConditionerFanMode` | auto, low, medium, high, turbo | auto |
| Wind direction | `fanOscillationMode` | fixed, vertical, horizontal, all | vertical |
| Special mode | `custom.airConditionerOptionalMode` | off, sleep, quiet, smart, speed, windFree, windFreeSleep | off |
| Tropical night | `custom.airConditionerTropicalNightMode` | 0-35 h | 0 |
| Auto clean | `custom.autoCleaningMode` | on, off | off |
| Sound | `audioVolume` | 0-100 (0/100 in practice) | 0 |
| Filter reset | `custom.dustFilter.resetDustFilter` | — | — |
| Self-check | `samsungce.selfCheck` | start, cancel | ready |
| Refresh | `refresh` | — | — |

## Readings

| Reading | Attribute | Sample |
|---|---|---|
| Room temperature | `temperatureMeasurement.temperature` | 25 °C |
| Humidity | `relativeHumidityMeasurement.humidity` | 68 % |
| Power | `powerConsumption.power` | 216 W |
| Energy total | `powerConsumption.energy` | 2 808 106 Wh |
| Filter | `custom.dustFilter` | usage 100 %, status `wash`, capacity 500 h |

## Listed as disabled but reporting values (experimental)

- `samsungce.airConditionerLighting` — `lighting: on`, supported `on/off`.
- `samsungce.airConditionerBeep` — `beep: off`. The sound is controlled through `audioVolume`.

## Disabled and null (hidden)

air quality, dust, very fine dust, odor sensors; deodor, HEPA and very fine dust filters;
do-not-disturb; welcome cooling; odor controller; individual lock; quick control; SPi mode
(`setSpiMode` listed in `unavailableCommands`); audio notification; demand response; OpenADR.

## Not exposed by the API on this unit

On/off timers. No timer capability exists.
