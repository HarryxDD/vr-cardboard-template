# VR Cardboard Template - Virtual Optics Simulation

A Unity VR project for Google Cardboard featuring realistic optical physics simulation with chromatic and spherical aberration effects.

## Features

- **Virtual Optics Physics**: Realistic light ray simulation through convex and concave lenses
- **Chromatic Aberration**: Wavelength-dependent refraction (red, green, blue light separation)
- **Spherical Aberration**: Distance-from-center dependent focusing
- **Interactive Lens System**: Grab and manipulate optical components in VR
- **Google Cardboard XR Support**: Mobile VR experience optimized for Cardboard devices

## Prerequisites

- Unity 2022.3 or later
- Google Cardboard XR Plugin for Unity
- Android SDK (for mobile builds)

## Setup Instructions

### 1. Clone the Repository

```bash
git clone https://github.com/HarryxDD/vr-cardboard-template.git
cd vr-cardboard-template
```

### 2. Install Required Assets

This project excludes large Unity Asset Store packages to keep the repository lightweight. You need to install them manually:

#### **BK_AlchemistHouse** (~1.5 GB)
- Download from Unity Asset Store: [BK AlchemistHouse](https://assetstore.unity.com/packages/3d/environments/fantasy/bk-alchemisthouse-196985)
- Import into `Assets/BK_AlchemistHouse/` folder

#### **Photon Unity Networking** (~36 MB)
- Download from Unity Asset Store: [PUN 2 - FREE](https://assetstore.unity.com/packages/tools/network/pun-2-free-119922)
- Import into `Assets/Photon/` folder
- Configure your Photon App ID in `Resources/PhotonServerSettings`

### 3. Open the Project

1. Open Unity Hub
2. Click "Add" and select the cloned project folder
3. Open with Unity 2022.3 LTS or later

### 4. Configure Build Settings

For Android (Cardboard):
1. File → Build Settings → Switch to Android
2. Player Settings → XR Plug-in Management → Enable Cardboard XR Plugin
3. Set minimum API level to Android 7.0 (API 24) or higher

## Project Structure

```
Assets/
├── Scripts/
│   ├── RayPhysics.cs          # Core optical physics simulation
│   ├── RayEmitter.cs           # Light ray generation
│   └── LensExitSurface.cs      # Ray-lens interaction
├── Prefabs/
│   └── Lens/
│       ├── ConvexLens.prefab
│       └── ConcaveLens.prefab
├── Scenes/
│   └── SampleScene.unity       # Main VR scene
├── Resources/
│   └── LensProperties.cs       # Lens configuration
└── [BK_AlchemistHouse/]        # (excluded - download separately)
└── [Photon/]                   # (excluded - download separately)
```

## Optical Physics System

### Key Components

- **RayPhysics**: Calculates Snell's law with chromatic and spherical aberration
- **RayEmitter**: Generates parallel or diverging light rays
- **LensProperties**: Configures refractive index, focal length, and lens geometry

### Aberration Parameters

```csharp
// Chromatic aberration (wavelength-dependent)
wavelengthShift = 0.08f;  // Adjust color separation

// Spherical aberration (distance-dependent)
sphericalAberration = 0.15f;  // Adjust focal point spread
```

## Controls

- **Gaze + Hold**: Select and grab objects
- **Head Movement**: Look around the VR environment
- **Cardboard Button**: Interact with UI elements

## Performance Optimization

- Ray pooling system to reduce allocations
- Configurable ray count (default: 10 rays per emitter)
- Mobile-optimized rendering

## Known Issues

- Large asset packages excluded from repository to avoid Git LFS quota limits
- Photon networking requires manual configuration after import

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is for educational purposes. Asset Store packages have their own licenses:
- BK_AlchemistHouse: Check Unity Asset Store license
- Photon Unity Networking: [Photon License](https://www.photonengine.com/en-US/Photon/LicenseTerms)

## Credits

- Optical physics implementation: Custom ray tracing with Snell's law
- VR framework: Google Cardboard XR Plugin
- Environment: BK_AlchemistHouse (Unity Asset Store)
- Networking: Photon Unity Networking (PUN 2)

## Support

For issues or questions, please open an issue on GitHub or contact the maintainer.
