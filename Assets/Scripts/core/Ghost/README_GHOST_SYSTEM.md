# Ghost System Implementation

Implementasi sistem ghost sederhana dengan AI yang responsif terhadap kondisi sanity player. Ghost akan lebih agresif (bergerak lebih cepat dan memberikan damage lebih besar) ketika sanity player menurun.

## 📋 Fitur

### 1. **Player Sanity System** (`PlayerSanity.cs`)

- Sistem sanity yang dapat menurun seiring waktu atau karena event tertentu
- 4 level sanity: High, Medium, Low, Critical
- Passive decay (opsional)
- Recovery system
- Events untuk tracking perubahan sanity

### 2. **Ghost AI** (`GhostAI.cs`)

- State machine dengan 5 state: Idle, Patrol, Chase, Attack, Stunned
- Detection system dengan line of sight check
- Patrol system dengan waypoints
- **Speed scaling berdasarkan sanity player:**
  - High Sanity: 1x speed (normal)
  - Medium Sanity: 1.3x speed
  - Low Sanity: 1.6x speed
  - Critical Sanity: 2x speed (sangat cepat!)

### 3. **Ghost Attack System** (`GhostAttack.cs`)

- Attack cooldown system
- **Damage scaling berdasarkan sanity:**
  - High Sanity: 1x damage
  - Medium Sanity: 1.2x damage
  - Low Sanity: 1.5x damage
  - Critical Sanity: 2x damage
- Sanity drain pada setiap serangan
- Events untuk attack lifecycle

### 4. **Health System**

- `IHealth` interface untuk damage system
- `PlayerHealth.cs` implementasi untuk player
- Auto regeneration (opsional)
- Integration dengan checkpoint system

---

## 🎮 Setup di Unity

### Setup Player

1. **Attach Components ke Player GameObject:**

   ```
   - PlayerController (existing)
   - PlayerSanity (NEW)
   - PlayerHealth (NEW)
   - PlayerRespawn (existing, jika ada)
   ```

2. **Configure PlayerSanity:**

   - Max Sanity: 100
   - Passive Decay Rate: 0.5 (optional, bisa di-disable)
   - Enable Passive Decay: False (aktifkan jika ingin sanity menurun otomatis)
   - Recovery Rate: 1.0

3. **Configure PlayerHealth:**
   - Max Health: 100
   - Can Regenerate: False (optional)
   - Regeneration Rate: 5
   - Regeneration Delay: 3

### Setup Ghost

1. **Buat GameObject baru untuk Ghost:**

   - Buat Empty GameObject, nama: "Ghost"
   - Posisikan di scene

2. **Attach Components:**

   ```
   - GhostAI (NEW)
   - GhostAttack (NEW)
   ```

3. **Configure GhostAI:**

   - **References:**

     - Player: Drag player GameObject (auto-detect jika player tagged "Player")
     - Player Sanity: Auto-detect dari player

   - **Detection:**

     - Detection Range: 15
     - Attack Range: 2
     - Lose Target Range: 25

   - **Movement Base Speed:**

     - Idle Speed: 1
     - Patrol Speed: 2
     - Chase Speed: 4

   - **Sanity Speed Multipliers:**
     - High Sanity: 1.0x
     - Medium Sanity: 1.3x
     - Low Sanity: 1.6x
     - Critical Sanity: 2.0x

4. **Configure GhostAttack:**
   - Base Damage: 15
   - Attack Cooldown: 1.5
   - Attack Duration: 0.5
   - Scale Damage With Sanity: True
   - Reduce Sanity On Attack: True
   - Sanity Drain Per Attack: 5

### Optional: Setup Patrol

1. Buat Empty GameObjects untuk patrol waypoints:

   ```
   - PatrolPoint_1
   - PatrolPoint_2
   - PatrolPoint_3
   ```

2. Posisikan waypoints di scene

3. Pada GhostAI component:
   - Set array size Patrol Points sesuai jumlah waypoint
   - Drag waypoints ke array
   - Set Waypoint Reach Distance: 1
   - Set Patrol Wait Time: 2

---

## 🎯 Cara Kerja

### Ghost Behavior Flow

```
IDLE → PATROL → (Player Detected) → CHASE → ATTACK
       ↑                                ↓
       └────────── (Player Lost) ───────┘
```

1. **Idle State:** Ghost diam di tempat, menunggu
2. **Patrol State:** Ghost berpatroli mengikuti waypoints
3. **Chase State:** Ghost mengejar player yang terdeteksi
4. **Attack State:** Ghost menyerang player di dalam attack range

### Sanity Impact

#### Pada Speed:

```
Player Sanity → Ghost Speed
High (70-100%)   → 1.0x (Normal)
Medium (40-70%)  → 1.3x (Lebih cepat)
Low (20-40%)     → 1.6x (Cepat)
Critical (0-20%) → 2.0x (Sangat cepat!)
```

#### Pada Damage:

```
Player Sanity → Ghost Damage
High (70-100%)   → 1.0x (15 damage)
Medium (40-70%)  → 1.2x (18 damage)
Low (20-40%)     → 1.5x (22.5 damage)
Critical (0-20%) → 2.0x (30 damage!)
```

---

## 🧪 Testing

### Test Sanity System

1. Tekan Play di Unity
2. Buka Inspector untuk melihat PlayerSanity component
3. Untuk testing manual:

   ```csharp
   // Reduce sanity
   playerSanity.DecreaseSanity(20);

   // Observe ghost speed increase
   ```

### Test Ghost AI

1. Pastikan player tagged sebagai "Player"
2. Ghost akan otomatis detect dan chase player
3. Observe perubahan speed saat sanity menurun
4. Check Gizmos di Scene view untuk visualisasi range

### Debug Mode

Enable Gizmos di Scene view untuk melihat:

- Yellow sphere: Detection range
- Red sphere: Attack range
- Blue sphere: Lose target range
- Green spheres: Patrol waypoints
- Red line: Active chase ke player

---

## 📝 Customization

### Membuat Ghost Lebih Agresif

```csharp
// Di Inspector GhostAI:
- Detection Range: 20 (lebih jauh)
- Chase Speed: 6 (lebih cepat)
- Critical Sanity Speed Multiplier: 3 (sangat cepat!)

// Di Inspector GhostAttack:
- Base Damage: 25 (lebih sakit)
- Critical Sanity Damage Multiplier: 3 (very dangerous!)
```

### Membuat Sanity Decay Lebih Intens

```csharp
// Di Inspector PlayerSanity:
- Enable Passive Decay: True
- Passive Decay Rate: 2.0 (cepat menurun)
```

### Menambah Events Custom

```csharp
// Subscribe ke events
ghostAI.OnPlayerDetected += () => {
    Debug.Log("Ghost melihat player!");
    // Play sound effect
};

playerSanity.OnSanityLevelChanged += (level) => {
    Debug.Log($"Sanity level: {level}");
    // Update UI, change lighting, etc
};
```

---

## 🔧 API Reference

### PlayerSanity

```csharp
// Properties
float CurrentSanity { get; }
float SanityPercentage { get; }
SanityLevel CurrentSanityLevel { get; }

// Methods
void DecreaseSanity(float amount)
void IncreaseSanity(float amount)
void ResetSanity()
void SetPassiveDecay(bool enabled)

// Events
event Action<float> OnSanityChanged
event Action<SanityLevel> OnSanityLevelChanged
event Action OnSanityDepleted
```

### GhostAI

```csharp
// Properties
GhostState CurrentState { get; }
float CurrentSpeed { get; }
bool IsActive { get; }

// Methods
void SetActive(bool active)
void Stun(float duration)
void Teleport(Vector3 position)

// Events
event Action<GhostState> OnStateChanged
event Action OnPlayerDetected
event Action OnPlayerLost
```

### GhostAttack

```csharp
// Properties
bool IsAttacking { get; }
bool CanAttack()

// Methods
void Attack(GameObject target)
void SetBaseDamage(float damage)
void SetAttackCooldown(float cooldown)
void ResetCooldown()

// Events
event Action OnAttackStarted
event Action OnAttackCompleted
event Action<GameObject> OnAttackHit
```

---

## 🎨 Untuk Implementasi Model 3D Nanti

Ketika sudah ada model 3D dan animasi:

1. **Tambahkan Animator component ke Ghost**
2. **Hook animations ke state changes:**

   ```csharp
   ghostAI.OnStateChanged += (state) => {
       switch(state) {
           case GhostState.Idle:
               animator.SetTrigger("Idle");
               break;
           case GhostState.Patrol:
               animator.SetTrigger("Walk");
               break;
           case GhostState.Chase:
               animator.SetTrigger("Run");
               break;
           case GhostState.Attack:
               animator.SetTrigger("Attack");
               break;
       }
   };
   ```

3. **Tambahkan visual effects:**
   - Particle effects saat attack
   - Glow effect berdasarkan sanity level
   - Trail renderer untuk movement
   - Sound effects untuk setiap state

---

## 🐛 Troubleshooting

**Ghost tidak bergerak:**

- Pastikan `Start Active` = true di GhostAI
- Check apakah ada patrol points (atau akan idle)
- Pastikan player ada dan tagged "Player"

**Ghost tidak mengejar player:**

- Check Detection Range (apakah cukup jauh?)
- Check Obstruction Mask (apakah ada wall yang menghalangi?)
- Check apakah player di dalam detection range

**Damage tidak berubah dengan sanity:**

- Pastikan `Scale Damage With Sanity` = true
- Check apakah PlayerSanity component attached ke player
- Verify GhostAttack dapat access PlayerSanity

**Player tidak mati:**

- Pastikan PlayerHealth attached ke player
- Check apakah damage > 0
- Verify IHealth interface implemented correctly

---

## 📚 Next Steps

1. **Tambah Visual:**

   - 3D model untuk ghost
   - Animasi (idle, walk, run, attack)
   - Particle effects

2. **Tambah Audio:**

   - Ambient sound untuk ghost
   - Attack sound
   - Footsteps
   - Scare sound saat detection

3. **Expand Mechanics:**

   - Multiple ghost types dengan behavior berbeda
   - Hiding system untuk player
   - Light sources mempengaruhi ghost behavior
   - Items untuk stun/repel ghost

4. **Polish:**
   - UI untuk sanity bar
   - Visual feedback untuk sanity levels
   - Camera effects saat sanity rendah
   - Screen distortion effects

---

Implementasi ghost system sudah selesai dan siap untuk digunakan! 🎮👻
