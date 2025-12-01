# 🔗 Player-HUD Connection Guide

## ✅ System Sudah Terhubung!

Fear system (PlayerAttributes) sudah otomatis terhubung ke HUD system melalui C# events.

## 📊 Sistem yang Terhubung:

### 1. Fear System → Health Display

- **Fear = 0** → Health bar **penuh** (hijau)
- **Fear = 50** → Health bar **setengah** (kuning)
- **Fear = 100** → Health bar **kosong** (merah) + Player mati

### 2. Battery System → Battery Display

- Battery berkurang saat flashlight menyala
- Battery bertambah dari interactable objects
- UI menampilkan percentage real-time

## 🎮 Setup di Unity Editor

### Quick Setup (Otomatis):

1. **Pastikan Player sudah ada:**

   - Player GameObject dengan tag "Player"
   - Component: `PlayerAttributes`

2. **Setup HUD:**

   - Buat Canvas jika belum ada
   - Add `HealthDisplay` ke UI Image (filled type)
   - Add `BatteryDisplay` ke UI Image (filled type)

3. **Connect (Optional):**
   - Add `PlayerHUDConnector` ke Canvas
   - Klik kanan → "Auto-Find HUD Components"
   - Done!

### Manual Setup:

#### HealthDisplay:

```
1. Create UI → Image
2. Set Image Type = Filled
3. Add Component: HealthDisplay
4. Assign Fill Image ke field "Health Bar Fill"
5. (Optional) Add TextMeshPro untuk text display
```

#### BatteryDisplay:

```
1. Create UI → Image
2. Set Image Type = Filled
3. Add Component: BatteryDisplay
4. Assign Fill Image ke field "Battery Bar Fill"
5. (Optional) Add TextMeshPro untuk percentage text
```

## 🔧 Cara Kerja

### Events Flow:

```
PlayerAttributes
    ↓ (static event)
    ├─→ onFearUpdate (float value)
    │   └─→ HealthDisplay.OnFearUpdated()
    │       └─→ UpdateHealth() - Inverted (100-fear)
    │
    └─→ onBatteryUpdate (float value)
        └─→ BatteryDisplay.OnBatteryUpdated()
            └─→ UpdateBattery()
```

### Auto-Updates Trigger:

1. **Game Start:**

   - PlayerAttributes.Start() memanggil initial events
   - HUD display langsung terupdate

2. **Saat Fear Berubah:**

   - Ghost menyentuh player
   - AddFear() dipanggil
   - onFearUpdate event triggered
   - HealthDisplay update otomatis

3. **Saat Battery Berubah:**
   - Flashlight menyala (drain)
   - Interactable menambah battery
   - onBatteryUpdate event triggered
   - BatteryDisplay update otomatis

## 📝 Testing

### Test Fear/Health:

```csharp
// Tambah fear (kurangi health)
PlayerAttributes player = FindObjectOfType<PlayerAttributes>();
player.Add(AttributesType.Fear, 20); // +20 fear
// Health bar akan berkurang
```

### Test Battery:

```csharp
// Tambah battery
player.Add(AttributesType.Battery, 30); // +30 battery
// Battery bar akan bertambah
```

### Test Ghost Damage:

```
1. Spawn ghost di scene
2. Player menyentuh ghost
3. Fear bertambah
4. Health bar berkurang otomatis
```

## 🎨 Customization

### HealthDisplay Colors:

```
High Health (> 50%) = Green
Medium Health (25-50%) = Yellow
Low Health (< 25%) = Red
```

### BatteryDisplay Features:

```
- Smooth transition animation
- Color based on level
- Blinking when critical (< 15%)
- Percentage text display
```

## ⚙️ Settings

### PlayerAttributes (Inspector):

- `Max Fear` = 100 (default)
- `Initial Battery` = 100 (default)
- Events sudah auto-wired

### HealthDisplay (Inspector):

- `Smooth Transition` = true (recommended)
- `Transition Speed` = 5 (adjust sesuai selera)
- `Show Percentage` = false/true

### BatteryDisplay (Inspector):

- `Smooth Transition` = true
- `Blink When Low` = true (untuk warning effect)
- `Battery Text Format` = "{2:0}%" (shows percentage)

## 🐛 Troubleshooting

### HUD tidak update?

✅ Check Player tag = "Player"
✅ Check PlayerAttributes component ada
✅ Check console untuk error
✅ Check HUD displays ada di scene dan active

### Health bar terbalik?

✅ Fear system sudah inverted otomatis
✅ 0 fear = full health bar
✅ 100 fear = empty bar

### Battery tidak berkurang?

✅ Check flashlight enabled
✅ Check TweeningBattery aktif
✅ Check decrement interval > 0

### Events tidak trigger?

✅ Check OnEnable/OnDisable ada di HUD scripts
✅ Check static events di PlayerAttributes
✅ Restart Unity jika perlu (reload scripts)

## 📚 Script References

**Modified Files:**

- `PlayerAttributes.cs` - Added battery event invocations
- `HealthDisplay.cs` - Subscribe to onFearUpdate
- `BatteryDisplay.cs` - Subscribe to onBatteryUpdate

**New Files:**

- `PlayerHUDConnector.cs` - Helper untuk validation

## 🎯 Quick Test Checklist

- [ ] Player ada di scene dengan PlayerAttributes
- [ ] HealthDisplay ada dan assigned
- [ ] BatteryDisplay ada dan assigned
- [ ] Play mode → Health bar full (green)
- [ ] Play mode → Battery bar full
- [ ] Test ghost damage → Health berkurang
- [ ] Test flashlight → Battery berkurang
- [ ] All working! ✅

---

**Status:** ✅ Fully Connected & Working
**Auto-Update:** ✅ Real-time via C# Events
**No Manual Setup Required:** HUD updates automatically!
