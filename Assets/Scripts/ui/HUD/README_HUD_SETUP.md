# 🎮 HUD Setup Guide - Bar System

## ✅ **1. Sanity Display (Bar with Text)**

### UI Structure:

```
Canvas
└── HUD
    └── SanityContainer
        ├── SanityBarBackground (Image)
        ├── SanityBarFill (Image - Type: Filled)
        └── SanityText (TextMeshProUGUI)
```

### Setup Steps:

#### A. Create Container

1. Right-click HUD → Create Empty
2. Rename: `SanityContainer`
3. Anchor: **Top Left** atau **Top Center**
4. Position: Sesuaikan (misal: X=50, Y=-50)
5. Size: Width=200, Height=40

#### B. Create Bar Background

1. Right-click SanityContainer → UI → Image
2. Rename: `SanityBarBackground`
3. Stretch to fill container
4. Color: Dark gray/black dengan alpha 0.5
5. (Optional) Add outline/border

#### C. Create Bar Fill

1. Right-click SanityContainer → UI → Image
2. Rename: `SanityBarFill`
3. Stretch to fill container (same as background)
4. **Image Type**: Change to **Filled** (PENTING!)
5. **Fill Method**: Horizontal
6. **Fill Origin**: Left
7. Initial **Fill Amount**: 1
8. Color: Green (akan berubah otomatis)

#### D. Create Text

1. Right-click SanityContainer → UI → Text - TextMeshPro
2. Rename: `SanityText`
3. Alignment: **Center** (horizontal & vertical)
4. Font Size: 18-24
5. Color: White
6. Stretch to fill container

#### E. Add SanityDisplay Component

1. Select **SanityContainer**
2. Add Component → **SanityDisplay**
3. Configure:

**UI References:**

- Sanity Bar Fill → Drag `SanityBarFill`
- Sanity Text → Drag `SanityText`
- Sanity Icon → (optional)

**Visual Settings:**

- High Sanity Color: Green `#00FF00`
- Medium Sanity Color: Yellow `#FFFF00`
- Low Sanity Color: Red `#FF0000`
- Medium Threshold: `0.5` (50%)
- Low Threshold: `0.25` (25%)

**Animation Settings:**

- Smooth Transition: ✅ Checked
- Transition Speed: `5`

**Blink Settings:**

- Blink When Low: ✅ Checked
- Critical Threshold: `0.15` (15%)
- Blink Speed: `2`
- Blink Min Alpha: `0.6` (fade to 60%, not 0%)

**Display Format:**

- Sanity Text Format: `"{0:0.0}%"` (1 decimal)
- Text Color: White `#FFFFFF`
- Text Outline: ✅ Checked
- Outline Color: Black `#000000`

---

## ✅ **2. Discrete Battery Display (Segment Bars)**

### UI Structure:

```
Canvas
└── HUD
    └── BatteryContainer
        ├── BatterySegments (Empty - auto-generated)
        └── BatteryText (TextMeshProUGUI - Optional)
```

### Setup Steps:

#### A. Create Container

1. Right-click HUD → Create Empty
2. Rename: `BatteryContainer`
3. Anchor: **Bottom Right**
4. Position: X=-50, Y=50
5. Size: Auto (will fit segments)

#### B. Create Segments Container

1. Right-click BatteryContainer → Create Empty
2. Rename: `BatterySegments`
3. Anchor: **Middle-Center**
4. **Width**: `170` (atau lebih - cukup untuk 10 segment)
5. **Height**: `35`

#### C. Create Text (Optional)

1. Right-click BatteryContainer → UI → Text - TextMeshPro
2. Rename: `BatteryText`
3. Position: Below segments (Y=-30)
4. Alignment: Center
5. Font Size: 16-20
6. Color: White

#### D. Add DiscreteBatteryDisplay Component

1. Select **BatteryContainer**
2. Add Component → **DiscreteBatteryDisplay**
3. Configure:

**UI References:**

- Segment Container → Drag `BatterySegments` (empty GameObject)
- Segment Prefab → Leave empty (auto-create)
- Battery Text → Drag `BatteryText` (optional)

**Segment Settings:**

- Segment Count: `10` (each = 10%)
- Segment Width: `15`
- Segment Height: `30`
- Segment Spacing: `2`

**Visual Settings:**

- Full Battery Color: Green `#00FF00`
- Medium Battery Color: Yellow `#FFFF00`
- Low Battery Color: Red `#FF0000`
- Empty Segment Color: Dark gray `#333333` (alpha ~0.3)
- Medium Threshold: `0.5` (50%)
- Low Threshold: `0.25` (25%)

**Blink Settings:**

- Blink When Low: ✅ Checked
- Critical Threshold: `0.15` (15%)
- Blink Speed: `2`
- Blink Min Alpha: `0.6` (fade to 60%)

**Segment Shape:**

- Rounded Corners: ⬜ Unchecked (basic rectangles)
- Corner Radius: `3` (if using custom sprite)

**Display Format:**

- Battery Text Format: `"{0:0}%"` (no decimal)
- Text Color: White `#FFFFFF`
- Text Outline: ✅ Checked
- Outline Color: Black `#000000`

---

## 🔗 **3. Connect to HUDManager**

### Option A: Auto-Find (Recommended)

1. Select **HUDManager** GameObject
2. Check **Auto Find Components** ✅
3. Components will auto-detect on Start

### Option B: Manual Assignment

1. Select **HUDManager**
2. Drag components:
   - Sanity Display → `SanityContainer`
   - Discrete Battery Display → `BatteryContainer`

---

## 🎨 **4. Custom Segment Prefab (Optional)**

For rounded corners or custom shapes:

### Create Prefab:

1. Create → UI → Image
2. Rename: `BatterySegmentPrefab`
3. Add sprite with rounded borders
4. Or use UI shader with roundness
5. Size: Match segment width/height
6. Drag to Project folder (make prefab)
7. Delete from scene

### Assign Prefab:

1. Select BatteryContainer
2. DiscreteBatteryDisplay → Segment Prefab
3. Drag the prefab from Project

---

## 📊 **Expected Results:**

### Sanity Display:

```
[████████████████████] 75.5%
    (Green → Yellow → Red gradient)
    (Blinks 100%→60%→100% when ≤15%)
```

### Battery Display:

```
[■][■][■][■][■][■][■][■][□][□] 80%
    (10 discrete segments)
    (Blinks when ≤15%)
    (No blink at 0%)
```

---

## 🐛 **Troubleshooting:**

### Sanity bar not filling correctly?

- Check Image Type = **Filled** (not Simple/Sliced)
- Check Fill Method = **Horizontal**
- Check Fill Origin = **Left**

### Battery segments stacked/overlapping?

- Check BatterySegments Width = at least `170`
- Verify HorizontalLayoutGroup exists (auto-created)
- Check spacing value

### Text not visible/low contrast?

- Enable Text Outline ✅
- Increase outline width to 0.2-0.3
- Use white text with black outline

### Blinking too fast/slow?

- Adjust Blink Speed (default: 2)
- Adjust Blink Min Alpha (0.6 = 60% fade)

### Not blinking when low?

- Check Blink When Low ✅
- Check Critical Threshold (0.15 = 15%)
- Battery at 0% won't blink (by design)

---

## ⚡ **Quick Test:**

### Test Sanity:

1. Play game
2. Take damage from ghost
3. Watch bar decrease with color change
4. At ≤15%: Should blink (fade 100%→60%)

### Test Battery:

1. Play game
2. Turn flashlight ON
3. Watch segments disappear one by one
4. At ≤15%: Should blink (fade 100%→60%)
5. At 0%: No blink (stays visible)

---

✅ Setup complete! 🎮✨
