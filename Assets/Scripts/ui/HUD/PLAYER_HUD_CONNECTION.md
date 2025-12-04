# 🔗 Player-HUD Connection Guide

## ✅ System Sudah Terhubung!

Sanity system (PlayerAttributes) sudah otomatis terhubung ke HUD system melalui C# events.

## 📊 Sistem yang Terhubung:

### 1. Sanity System → Sanity Display

- **Sanity = 100** → Sanity bar **penuh** (hijau) - Player sehat
- **Sanity = 50** → Sanity bar **setengah** (kuning)
- **Sanity = 0** → Sanity bar **kosong** (merah) + Player mati

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
   - Add `SanityDisplay` ke UI Image (filled type)
   - Add `BatteryDisplay` ke UI Image (filled type)

3. **Connect (Optional):**
   - Add `PlayerHUDConnector` ke Canvas
   - Klik kanan → "Auto-Find HUD Components"
   - Done!

### Manual Setup:

#### SanityDisplay:

```
1. Create UI → Image
2. Set Image Type = Filled
3. Add Component: SanityDisplay
4. Assign Fill Image ke field "Sanity Bar Fill"
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
    ├─→ onSanityUpdate (float normalizedValue 0-1)
    │   └─→ SanityDisplay.OnSanityUpdated()
    │       └─→ UpdateSanityNormalized() - 1=full, 0=dead
    │
    └─→ onBatteryUpdate (float value)
        └─→ BatteryDisplay.OnBatteryUpdated()
            └─→ UpdateBattery()
```

### Auto-Updates Trigger:

1. **Game Start:**

   - PlayerAttributes.Start() memanggil initial events
   - HUD display langsung terupdate

2. **Saat Sanity Berubah:**

   - Ghost menyentuh player
   - TakeDamage() dipanggil
   - Sanity berkurang
   - onSanityUpdate event triggered
   - SanityDisplay update otomatis

3. **Saat Battery Berubah:**
   - Flashlight menyala (drain)
   - Interactable menambah battery
   - onBatteryUpdate event triggered
   - BatteryDisplay update otomatis

## 📝 Testing

### Test Sanity:

```csharp
// Kurangi sanity (damage)
PlayerAttributes player = FindObjectOfType<PlayerAttributes>();
player.TakeDamage(DamageType.Sanity, 20); // -20 sanity
// Sanity bar akan berkurang
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
3. Sanity berkurang
4. Sanity bar berkurang otomatis
```

## 🎨 Customization

### SanityDisplay Colors:

```
High Sanity (> 50%) = Green
Medium Sanity (25-50%) = Yellow
Low Sanity (< 25%) = Red
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

- `Max Sanity` = 100 (default, player starts with full sanity)
- `Initial Battery` = 100 (default)
- Events sudah auto-wired

### SanityDisplay (Inspector):

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

### Sanity system:

✅ Sanity 100 = healthy (full bar)
✅ Sanity 0 = dead (empty bar)
✅ Ghost damage mengurangi sanity

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

- `PlayerAttributes.cs` - Manages sanity and battery with events
- `SanityDisplay.cs` - Subscribe to onSanityUpdate
- `BatteryDisplay.cs` - Subscribe to onBatteryUpdate

**New Files:**

- `PlayerHUDConnector.cs` - Helper untuk validation

## 🎯 Quick Test Checklist

- [ ] Player ada di scene dengan PlayerAttributes
- [ ] SanityDisplay ada dan assigned
- [ ] BatteryDisplay ada dan assigned
- [ ] Play mode → Sanity bar full (green)
- [ ] Play mode → Battery bar full
- [ ] Test ghost damage → Sanity berkurang
- [ ] Test flashlight → Battery berkurang
- [ ] All working! ✅

---

**Status:** ✅ Fully Connected & Working
**Auto-Update:** ✅ Real-time via C# Events
**No Manual Setup Required:** HUD updates automatically!
