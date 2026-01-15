# DorfTelemetry

A small telemetry mod for **Dorfromantik** built with **BepInEx**.

This mod scans the current board and generates a **CSV export** with the frequency distribution of placed tiles, based on the tile edge features.

---

## Dependencies

- `BepInEx`

## How to use

Download `DorfTelemetry.dll` and place it inside the plugins dir (`<DorfRomantik_dir>/BepInEx/plugins)

Open a game and press `F8` to generate and export a .csv for that board

## CSV Export Location

Exports are written to `<DorfRomantik_dir>/BepInEx/plugins/DorfTelemetryExports/`

## Tile Edge Codes

Each tile is represented by a 6-character code (one character per edge, clockwise).

Feature codes:

- `G` - Grass
- `A` - Agriculture
- `F` - Forest
- `V` - Village
- `R` - River
- `L` - Lake
- `T` - Train Track
- `S` - Train Station

TIle edge codes are canonically rotated until it reaches the lexicographically smallest rotation to make codes rotation independent. This prevents equal tiles in different rotations to be counted separetely.

e.g. A tile that's half Forest, half Grass could be represented by FFFGGG, or GFFFGG, or GGGFFF, etc. but will always be rotated to FFFGGG.
