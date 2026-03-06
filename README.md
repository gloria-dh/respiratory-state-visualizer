# Respiratory State Visualizer

<p align="center">
  <img src="Respiratory_State_Visualizer_V0/Resources/LOGO.png" alt="Respiratory State Visualizer logo" width="180" />
</p>

A Windows Forms application that renders a real-time, breathing-responsive avatar using contactless physiological sensors and customizable visual profiles.

## Prerequisites

- **Windows OS** with .NET Framework 4.7.2 or later
- **Visual Studio 2022** (or later) with the *.NET desktop development* workload, or **JetBrains Rider**
- **Python 3** with `pyserial` installed (`pip install pyserial`)
- **Texas Instruments mmWave Radar** (e.g. IWR1642BOOST or xWR1843) for live sensor mode
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
5. **Run** (`F5`) — the application starts on the **Setup** tab

## Application Tabs

### Setup
Configure the COM ports, chirp configuration file, and Python sensor script path.

| Field | Description |
|---|---|
| **CLI Port** | Serial port used for sending configuration commands (e.g. `COM3`) |
| **DATA Port** | Serial port used for receiving vitals data frames (e.g. `COM4`) |
| **Config File** | Path to a `.cfg` chirp configuration file from the TI Radar Toolbox |

### Customize
Build a personalised avatar by selecting skin tone, hair type/colour, clothing, and accessories. The preview updates in real time.

### Run
Displays the avatar with real-time breathing animations driven by the sensor. The panel shows live telemetry (heart rate, breath rate, breath deviation, and current state). Manual override buttons (**Neutral**, **Strained**, **Holding Breath**, **Recovering**, **Alert**) are available for testing without hardware.

- **Read Sensor / Stop Sensor** — starts or stops the live radar data stream
- The avatar face expression, chest movement, and breath visualization change based on the detected respiratory state
- Each sensor session is automatically logged to a timestamped CSV file in the `logs/` folder

## Respiratory States

The application classifies the user's breathing into one of five states:

| State | Trigger | Avatar Response |
|---|---|---|
| **Neutral** | Normal breath rate and deviation | Calm face, gentle chest rise/fall |
| **Strained** | Low / shallow breathing | Calm face, chest stays low |
| **HoldingBreath** | Near-zero breath rate detected | Holding-breath face, chest stays low |
| **Recovering** | Returning from Strained or HoldingBreath | Alternating face, chest rise/fall |
| **Alert** | High breath rate (hyperventilating) | Distressed face, rapid chest & breath-out animation |

## Session Logging

Every sensor session is saved as a timestamped CSV in `logs/` next to the executable. Logging is performed by the C# application (`SessionLogger.cs`).

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

## Architecture Overview

```
sensorPipeline.py           → Python sensor script (serial I/O, state machine)
        │  stdout pipe (VITALS|hr|br|dev|state / STATUS|msg / ERROR|msg)
        ▼
Program.cs                  → Entry point
MainForm.cs                 → Shell with tab navigation (Setup / Customize / Run)
├── SetupPage.cs            → COM port, config file, & Python script selection (UserControl)
├── AvatarCustomize.cs      → Avatar appearance editor (UserControl)
└── AvatarRun.cs            → Live avatar display & sensor control (UserControl)
AppTheme.cs                 → Shared color palette constants
AvatarLayerPainter.cs       → Shared image-layer rendering helper
AvatarLayerManager.cs       → Composition helper for avatar image layers
AvatarProfile.cs            → Data model for avatar appearance choices
AvatarState.cs              → Data model for respiratory state
RadarVitalsReader.cs        → Launches Python process, reads stdout pipe, parses received lines into events
SensorSetupSettings.cs      → Static storage for sensor configuration
SessionLogger.cs            → Writes sensor data to CSV
```