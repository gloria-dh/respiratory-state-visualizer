# Respiratory State Visualizer

A Windows Forms application that renders a real-time, breathing-responsive avatar using contactless physiological sensors and customizable visual profiles.

## Prerequisites

- **Windows OS** with .NET Framework 4.7.2 or later
- **Visual Studio 2022** (or later) with the *.NET desktop development* workload, or **JetBrains Rider**
- **Texas Instruments mmWave Radar** (e.g. IWR1642BOOST or xWR1843) for live sensor mode
- **TI Radar Toolbox** — provides the vital-signs chirp configuration files (`.cfg`)

## Getting Started

1. **Clone the repository**
   ```
   git clone https://github.com/gloria-dh/respiratory-state-visualizer.git
   ```
2. **Open the solution** in Visual Studio / Rider
   `Respiratory_State_Visualizer_V0.sln`
3. **Build** the project (`Ctrl+Shift+B`)
4. **Run** (`F5`) — the application starts on the **Setup** tab

## Application Tabs

### Setup
Configure the COM ports and chirp configuration file for the TI mmWave radar sensor.

| Field | Description |
|---|---|
| **CLI Port** | Serial port used for sending configuration commands (e.g. `COM5`) |
| **DATA Port** | Serial port used for receiving vitals data frames (e.g. `COM6`) |
| **Config File** | Path to a `.cfg` chirp configuration file from the TI Radar Toolbox |

### Customize
Build a personalised avatar by selecting skin tone, hair type/colour, clothing, and accessories. The preview updates in real time.

### Run
Displays the avatar with real-time breathing animations driven by the sensor. Manual override buttons (**Calm**, **Holding Breath**, **Hyperventilating**) are available for testing without hardware.

- **Read Sensor** — starts/stops the live radar data stream
- The avatar face expression, chest movement, and breath visualization change based on the detected breathing rate

## Architecture Overview

```
Program.cs              → Entry point
MainForm.cs             → Shell with tab navigation (Setup / Customize / Run)
├── SetupPage.cs        → COM port & config file selection (UserControl)
├── AvatarCustomize.cs  → Avatar appearance editor (UserControl)
└── AvatarRun.cs        → Live avatar display & sensor control (UserControl)
AppTheme.cs             → Shared color palette constants
AvatarLayerPainter.cs   → Shared image-layer rendering helper
AvatarLayerManager.cs   → Composition helper for avatar image layers
AvatarProfile.cs        → Data model for avatar appearance choices
AvatarState.cs          → Data model for current display state
RadarVitalsReader.cs    → Serial port reader for TI mmWave vitals frames
SensorSetupSettings.cs  → Static storage for sensor configuration
```