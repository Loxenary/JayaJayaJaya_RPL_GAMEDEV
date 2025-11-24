# Quick Setup Guide - Ghost System

## 🚀 Setup Cepat (5 Menit!)

### Metode 1: Automatic Setup (RECOMMENDED)

1. **Buat Empty GameObject di scene:**
   - Hierarchy → Right Click → Create Empty
   - Nama: "GhostSystemSetup"

2. **Attach script GhostSystemSetup:**
   - Add Component → GhostSystemSetup

3. **Configure (Optional):**
   - Ghost Spawn Position: Dimana ghost muncul pertama kali
   - Number Of Patrol Points: 4 (default)
   - Add Visual Placeholder: ✓ (checked)
   - Add Debug Controller: ✓ (checked)

4. **Run Setup:**
   - Klik kanan component GhostSystemSetup
   - Pilih "Setup Ghost System"
   - DONE! 🎉

### Metode 2: Manual Setup

#### A. Setup Player (5 steps)

1. Select Player GameObject di hierarchy
2. Add Component → PlayerSanity
3. Add Component → PlayerHealth
4. Configure PlayerSanity:
   - Max Sanity: 100
   - Enable Passive Decay: false (untuk testing)
5. Configure PlayerHealth:
   - Max Health: 100

#### B. Setup Ghost (6 steps)

1. **Create Ghost GameObject:**
   - Hierarchy → Create Empty
   - Nama: "Ghost"
   - Posisikan di scene

2. **Add Components (berurutan):**
   - Add Component → GhostAttack
   - Add Component → GhostAI
   - Add Component → GhostVisualPlaceholder (optional)
   - Add Component → GhostDebugController (optional)

3. **Configure GhostAI:**
   - Detection Range: 15
   - Attack Range: 2
   - Chase Speed: 4

4. **Configure GhostAttack:**
   - Base Damage: 15
   - Attack Cooldown: 1.5

#### C. Setup Patrol (Optional)

1. **Create Patrol Points:**
   - Create 3-4 Empty GameObjects
   - Nama: PatrolPoint_1, PatrolPoint_2, dst
   - Spread di area sekitar ghost

2. **Assign ke Ghost:**
   - Select Ghost
   - GhostAI Component → Patrol Points
   - Set size (misal: 4)
   - Drag patrol points ke array

---

## 🎮 Testing

### Test 1: Basic Ghost Behavior
1. Press Play
2. Ghost akan idle atau patrol (jika ada patrol points)
3. Dekati ghost → Ghost akan chase player
4. Ghost akan attack di attack range

### Test 2: Sanity System
1. Press Play
2. Tekan dan tahan `K` → Sanity menurun
3. Observe: Ghost bergerak lebih cepat!
4. Tekan dan tahan `L` → Sanity naik kembali

### Test 3: Debug Controls
- `G` - Toggle ghost active/inactive
- `H` - Stun ghost selama 3 detik
- `J` - Teleport ghost ke player
- `K` - Hold untuk decrease sanity
- `L` - Hold untuk increase sanity

---

## ✅ Checklist

### Player Setup ✓
- [ ] PlayerController exists
- [ ] PlayerSanity added
- [ ] PlayerHealth added
- [ ] Player tagged as "Player"

### Ghost Setup ✓
- [ ] Ghost GameObject created
- [ ] GhostAttack component added
- [ ] GhostAI component added
- [ ] Visual placeholder (optional but recommended)
- [ ] Debug controller (for testing)

### Patrol Setup (Optional) ✓
- [ ] Patrol points created
- [ ] Patrol points positioned
- [ ] Patrol points assigned to GhostAI

---

## 🐛 Common Issues

**Ghost tidak bergerak:**
- Check `Start Active` di GhostAI = true
- Check apakah ada patrol points (atau set Idle)

**Ghost tidak mengejar:**
- Player harus tagged "Player"
- Check Detection Range cukup besar
- Check tidak ada wall menghalangi

**Damage tidak jalan:**
- PlayerHealth harus ada di player
- Check Base Damage > 0

**Sanity tidak berpengaruh:**
- PlayerSanity harus ada di player
- Check `Scale ... With Sanity` = true di GhostAI & GhostAttack

---

## 🎨 Next: Add Visual

Ketika model 3D ready:
1. Import model ke Unity
2. Replace GhostVisualPlaceholder dengan model
3. Add Animator component
4. Connect animations ke state changes

---

## 📝 Quick Reference

### Speed Scaling
```
High Sanity    → 1.0x speed
Medium Sanity  → 1.3x speed
Low Sanity     → 1.6x speed
Critical       → 2.0x speed (FAST!)
```

### Damage Scaling
```
High Sanity    → 15 damage
Medium Sanity  → 18 damage
Low Sanity     → 22.5 damage
Critical       → 30 damage (DEADLY!)
```

---

Setup selesai! Ghost system sudah bisa digunakan 👻🎮
