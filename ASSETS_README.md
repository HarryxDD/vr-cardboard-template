# Asset Management Strategy

## Large Assets (NOT in Git)

These assets are stored externally due to size:

### **BK_AlchemistHouse** (1.5 GB)
- **Source:** Unity Asset Store
- **Storage:** OneDrive/Google Drive
- **Setup Instructions:**
  1. Download from: [Your OneDrive Link]
  2. Extract to `Assets/BK_AlchemistHouse/`
  3. Unity will auto-import

**Alternative:** Re-download from Unity Asset Store if you have it purchased

---

## What's Tracked in Git

✅ **Scripts** (~/0.02 MB)
- All C# code
- RayEmitter, RayPhysics, LensProperties, etc.

✅ **Scenes** (~/0.01 MB)
- Scene layouts
- Object hierarchy

✅ **Prefabs** (~/0.18 MB)
- Lens prefabs
- VR rig setup

✅ **Materials** (~/0.01 MB)
- Small material files

✅ **Photon** (36 MB) - Networking (via LFS)

✅ **TextMesh Pro** (3.84 MB) - UI text (via LFS)

---

## Setup Instructions for New Collaborators

1. **Clone repository:**
   ```bash
   git clone https://github.com/HarryxDD/vr-cardboard-template
   cd vr-cardboard-template
   git checkout version/virtual-optics
   ```

2. **Download large assets:**
   - Get `BK_AlchemistHouse` from [OneDrive/Google Drive link]
   - Extract to `Assets/BK_AlchemistHouse/`

3. **Open in Unity:**
   - Unity will import everything
   - Should work immediately

---

## Git LFS Status

Currently tracking via LFS:
- `*.fbx` - 3D models
- `*.png`, `*.jpg` - Textures
- `*.unitypackage` - Unity packages

**LFS Quota:** Check with `git lfs ls-files`

---

## Alternative: Asset Store Link

If you purchased BK_AlchemistHouse from Unity Asset Store:
1. Open Unity Package Manager
2. Go to "My Assets"
3. Download and import BK_AlchemistHouse
4. No external download needed!
