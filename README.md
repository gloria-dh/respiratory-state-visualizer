# Respiratory State Visualizer

<p align="center">
  <img src="Respiratory_State_Visualizer_V0/Resources/LOGO.png" alt="Respiratory State Visualizer logo" width="180" />
</p>

A Windows Forms application that renders a real-time, breathing-responsive avatar using contactless physiological sensors and customizable visual profiles.

## Prerequisites

- **Windows OS** with .NET Framework 4.7.2 or later
- **Visual Studio 2022** (or later) with the *.NET desktop development* workload, or **JetBrains Rider**
- **Python 3** with `pyserial` installed (`pip install pyserial`)
- **Texas Instruments mmWave Radar** (e.g. IWR6843AOPEVM) for live sensor mode
- **TI Radar Toolbox** — provides the vital-signs chirp configuration files (`.cfg`)

## Getting Started

1. **Clone the repository**
   ```
   git clone https://github.com/gloria-dh/respiratory-state-visualizer.git
   ```
2. **Install Python dependency**
   ```
   pip install pyserial
   ```
3. **Open the solution** in Visual Studio / Rider
   `Respiratory_State_Visualizer_V0.sln`
4. **Build** the project (`Ctrl+Shift+B`) — `sensorPipeline.py` is automatically copied to the output directory
5. **Run** (`F5`) — a boot screen plays, then the application opens on the **Setup** tab

## Application Tabs

### Setup
Configure the COM ports and chirp configuration file.

| Field | Description |
|---|---|
| **CLI Port** | Serial port used for sending configuration commands (e.g. `COM3`) |
| **DATA Port** | Serial port used for receiving vitals data frames (e.g. `COM4`) |
| **Config File** | Path to a `.cfg` chirp configuration file |

### Customize
Build a personalised avatar by selecting skin tone, hair type/colour, clothing, and accessories. The preview updates in real time.

### Run
Displays the avatar with real-time breathing animations driven by the sensor. The panel shows the current respiratory state, heart rate, and breath rate range.

- **Read Sensor / Stop Sensor** — starts or stops the live radar data stream
- The avatar face expression, chest movement, and breath visualisation change based on the detected respiratory state
- A calibration phase runs for the **first 8 sensor frames** before displaying data
- Each sensor session is automatically logged to a timestamped CSV file in the `logs/` folder

### History
Browse and review past sensor sessions. The left panel lists all saved CSV log files, and selecting one displays its contents in a data grid. Sessions can be deleted individually, and the logs folder can be opened directly in Explorer.

## Respiratory States

The application classifies the user's breathing into one of five states based on breath-waveform deviation and breath rate:

| State | Trigger | Breath-Rate Range | Avatar Response |
|---|---|---|---|
| **Neutral** | Deviation ≥ 0.02, breath rate ≤ 20, and not transitioning from HoldingBreath | 10 – 20 BPM | Calm face, gentle chest rise/fall |
| **Strained** | Deviation < 0.02 (first low-deviation reading from Neutral/Alert) | 5 – 10 BPM | Strained face, progressive cheek reddening, chest stays low |
| **HoldingBreath** | Deviation < 0.02, sustained from Strained/HoldingBreath/Recovering | < 5 BPM | Holding-breath face, chest stays low |
| **Recovering** | Deviation returns to normal within 1 grace packet after HoldingBreath | 5 – 10 BPM | Alternating face, flushed cheeks, chest rise/fall |
| **Alert** | Breath rate > 20 BPM (overrides deviation check) | > 20 BPM | Alternating face, rapid chest & breath-out animation |

## Session Logging

Every sensor session is saved as a timestamped CSV in `logs/` next to the executable. These logs can be reviewed from the **History** tab or opened externally.

| Column | Description |
|---|---|
| `Timestamp` | ISO 8601 timestamp of the reading |
| `PacketNumber` | Sequential packet counter (resets each session) |
| `HeartRate` | Heart rate in bpm |
| `BreathRate` | Breathing rate in bpm |
| `BreathDeviation` | Breath waveform deviation (unitless) |
| `State` | Current respiratory state (`Neutral`, `Strained`, `HoldingBreath`, `Recovering`, `Alert`) |

Example:
```
Timestamp,PacketNumber,HeartRate,BreathRate,BreathDeviation,State
2026-03-01T16:30:00.000000,1,72.50,14.20,0.0032,Neutral
2026-03-01T16:30:00.500000,2,73.10,15.80,0.0041,Neutral
```

## Utility Scripts

The `scripts/` folder contains standalone Python utilities for development and maintenance. They are independent of the application and run directly from the command line.

### `sensorRawDump.py` — Raw sensor dump

Connects to the radar, streams live vital-sign packets, and prints them in a tabular format — **without** any state classification. Useful for verifying sensor output and tuning thresholds.

```
python scripts/sensorRawDump.py --cli-port COM3 --data-port COM4 --config-file path/to/config.cfg
```

| Flag | Default | Description |
|---|---|---|
| `--cli-port` | `COM3` | Serial port for sending chirp config |
| `--data-port` | `COM4` | Serial port for receiving data frames |
| `--config-file` | *(required)* | Path to `.cfg` chirp configuration file |
| `--verbose` / `-v` | off | Also print all 35 raw TLV fields per packet |

### `wipeLogs.py` — Delete session logs

Deletes all `session_*.csv` files from `Respiratory_State_Visualizer_V0/bin/Debug/logs/`. Equivalent to manually clearing the History tab, but useful for bulk cleanup without opening the application.

```
python scripts/wipeLogs.py
```

## Architecture Overview

```
scripts/
├── sensorPipeline.py       → Python sensor script (serial I/O, state machine)
│       │  stdout pipe (VITALS|hr|br|dev|state / STATUS|msg / ERROR|msg)
│       ▼  (launched by RadarVitalsReader.cs)
├── sensorRawDump.py        → Standalone raw sensor dump (no state mapping, CLI only)
└── wipeLogs.py             → Bulk-delete all session CSVs from the debug logs folder

Respiratory_State_Visualizer_V0/
Program.cs                  → Entry point
MainForm.cs                 → Shell with tab navigation (Setup / Customize / Run / History)
├── SetupPage.cs            → COM port and config file selection (UserControl)
├── AvatarCustomize.cs      → Avatar appearance editor (UserControl)
└── AvatarRun.cs            → Live avatar display & sensor control (UserControl)
HistoryPage.cs              → Session log browser (UserControl)
SplashForm.cs               → Startup splash screen
AppTheme.cs                 → Shared colour palette constants
AvatarLayerPainter.cs       → Image-layer rendering
AvatarLayerManager.cs       → Avatar image layer composition
AvatarProfile.cs            → Data model for avatar appearance choices
AvatarState.cs              → Data model for respiratory state
RadarVitalsReader.cs        → Launches Python process, reads stdout pipe, fires events
SensorSetupSettings.cs      → Static storage for sensor configuration
SessionLogger.cs            → Writes sensor data to CSV
```