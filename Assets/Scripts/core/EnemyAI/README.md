# Enemy AI System Documentation

A scalable, modular Enemy AI system built with **MonsterLove FSM** and **Astar Pathfinding Project**.

## Features

- **State Machine**: Clean FSM using MonsterLove with states: Idle, Patrol, Seen, Chase, Attack
- **Cone Vision**: Realistic cone-based field of view detection (not just sphere colliders)
- **Smart Behavior**: Random idle/patrol switching, vision timeout system
- **Pathfinding**: Integrated Astar pathfinding for intelligent navigation
- **Detection System**: Configurable line-of-sight with raycast obstacle checking
- **Data-Driven**: ScriptableObject-based stats for easy balancing
- **Extensible**: Base class designed for inheritance (Melee, Ranged, Boss, etc.)
- **Event System**: UnityEvents for hooking up animations, VFX, audio

## Quick Start

### 1. Create Enemy Stats Asset

Right-click in Project window → `Create > Enemy AI > Enemy Stats`

Configure the stats:
- Movement (speed, acceleration, rotation)
- Detection (range, FOV angle)
- Combat (attack range, damage, cooldown)
- Health
- Patrol settings

### 2. Setup Enemy GameObject

1. Create a GameObject with a capsule/model
2. Add required components:
   - `AIPath` (from Astar)
   - `Seeker` (from Astar) - added automatically
   - `BaseEnemyAI` or `MeleeEnemyAI` or `RangedEnemyAI`
   - `EnemyDetection`
3. Assign the Enemy Stats asset you created
4. Configure detection layers (e.g., Player layer)

### 3. Setup Pathfinding

Make sure you have an Astar pathfinding graph set up in your scene:
- Window → Pathfinding → Pathfinding
- Scan your scene

### 4. Optional: Add Patrol Points

1. Create empty GameObjects as patrol points
2. Assign them to the `Patrol Points` array in the enemy
3. Enable patrol in Enemy Stats

## Architecture

### Core Components

#### **BaseEnemyAI.cs**
The main AI controller that combines FSM with pathfinding.

**States:**
- `Idle`: Standing still, scanning for targets with cone vision. After random wait time, chooses to go Idle or Patrol
- `Patrol`: Moving between patrol points, scanning for targets with cone vision
- `Seen`: Target spotted! Enemy evaluates if target is within chase radius. If player exits vision cone, 2-second countdown before returning to previous state
- `Chase`: Actively pursuing target. If vision is lost, waits 2 seconds before giving up
- `Attack`: Attacking within range

**Key Methods to Override:**
```csharp
protected virtual void OnAttackPerformed() // Custom attack logic
protected virtual void OnDamageTaken(float damage) // Damage response
protected virtual void OnDeath() // Death behavior
```

#### **EnemyStats.cs**
ScriptableObject for data-driven configuration. Create different stats for each enemy type.

#### **EnemyDetection.cs**
Handles target detection with cone-based vision:
- **Cone Vision**: Uses angle-based FOV, not just sphere colliders
- Range-based detection within the cone
- Line-of-sight raycasting to avoid detecting through walls
- Visual cone gizmos for debugging (cyan cone shows exact vision area)
- **Note**: Detection settings (range, angle, layer) come from EnemyStats and are passed via `Initialize()`. Only the obstacle layer is set directly on this component.

### Example Enemy Types

#### **MeleeEnemyAI.cs**
Extends BaseEnemyAI with close-range combat:
- Lunge attack mechanic
- Close combat behavior

#### **RangedEnemyAI.cs**
Extends BaseEnemyAI with ranged combat:
- Projectile shooting
- Kiting behavior (maintains optimal distance)
- Retreats if target too close

## Creating Custom Enemy Types

```csharp
using UnityEngine;
using EnemyAI;

public class BossEnemyAI : BaseEnemyAI
{
    [Header("Boss Settings")]
    [SerializeField] private float enrageHealthThreshold = 0.3f;
    private bool isEnraged = false;

    protected override void OnDamageTaken(float damage)
    {
        base.OnDamageTaken(damage);

        // Enrage at low health
        if (HealthPercentage <= enrageHealthThreshold && !isEnraged)
        {
            isEnraged = true;
            aiPath.maxSpeed = stats.maxSpeed * 1.5f;
            Log("Boss is enraged!");
        }
    }

    protected override void PerformAttack()
    {
        base.PerformAttack();

        // Boss special attack
        if (isEnraged)
        {
            AreaOfEffectAttack();
        }
    }

    private void AreaOfEffectAttack()
    {
        // Custom AOE attack logic
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        // Drop loot, trigger cutscene, etc.
    }
}
```

## Event System

### EnemyEventBroadcaster
Attach to enemy to expose UnityEvents in inspector:

- `onHealthChanged` - Health updates
- `onStateChanged` - State transitions
- `onTargetDetected` - Target acquired
- `onTargetLost` - Target lost
- `onDamageTaken` - Damage received
- `onDeath` - Enemy died
- `onAttack` - Attack performed

### EnemyEventListener
Example listener for VFX, audio, animations:

```csharp
// Hook up in Unity Inspector or code:
eventBroadcaster.onAttack.AddListener(() => {
    animator.SetTrigger("Attack");
    PlayAttackSound();
});
```

## FSM State Flow

```
     ┌──────────────────────────────┐
     │  Random Choice (50/50)       │
     ▼                              ▼
   Idle ◄──────────────────────► Patrol
     │                              │
     │  Enemy in cone vision        │
     └──────────┬───────────────────┘
                ▼
              Seen ◄─── (Player exits cone: 2s countdown)
                │
                │ (Player within chase radius)
                ▼
              Chase ◄─── (Lost vision: 2s countdown)
                │
                │ (Within attack range)
                ▼
              Attack
                │
                │ (Target moves away)
                └────► Back to Chase or Seen
```

**Transitions:**
- `Idle`: After random wait (2-5s), randomly picks Idle or Patrol (50% chance each)
- `Idle/Patrol → Seen`: Target detected in cone vision
- `Seen → Chase`: Target is within chase radius
- `Seen → Idle/Patrol`: Lost vision for 2 seconds
- `Chase → Attack`: Within attack range
- `Chase → Idle/Patrol`: Lost vision for 2 seconds OR target too far
- `Attack → Chase`: Target moves out of attack range

## Tips & Best Practices

### Performance
- Use appropriate detection range (smaller = better performance)
- Limit FOV angle where possible
- Use layers wisely for detection and obstacles

### Balancing
- Create different stat assets for enemy variants
- Test detection range vs attack range ratios
- Adjust chase distance and lose target time

### Debugging
- Enable `Show Debug Logs` in BaseEnemyAI to see state transitions
- Use Scene view to see detection gizmos:
  - **Cyan cone** = Cone-based vision area (FOV)
  - **Yellow sphere (faded)** = Maximum detection range
  - **Green sphere** = Chase radius (from Seen state)
  - **Red sphere** = Attack range
  - **Magenta line** = Current target tracking
  - **Cyan lines** = Patrol routes

**Important Settings:**
- `Patrol Chance`: 0.0 = always idle, 1.0 = always patrol, 0.5 = 50/50 random
- `Chase Radius`: Distance within which enemy will pursue from Seen state
- `Vision Lost Countdown`: Time (default 2s) before giving up when losing sight

### Integration with Game Systems

#### Damage System
```csharp
// From your weapon/projectile:
enemy.GetComponent<BaseEnemyAI>()?.TakeDamage(damageAmount);
```

#### Animation
```csharp
public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private BaseEnemyAI enemyAI;

    void Start()
    {
        enemyAI = GetComponent<BaseEnemyAI>();
    }

    void Update()
    {
        // Sync animations with AI state
        animator.SetFloat("Speed", enemyAI.aiPath.velocity.magnitude);
        animator.SetBool("IsChasing", enemyAI.CurrentState == BaseEnemyAI.EnemyState.Chase);
    }
}
```

## Troubleshooting

**Enemy not moving:**
- Check if Astar graph is scanned
- Verify AIPath component is enabled
- Ensure stats.maxSpeed > 0

**Enemy not detecting player:**
- Check detection layer mask includes player
- Verify detection range is sufficient
- Check for obstacles blocking line of sight

**Enemy spinning/jittering:**
- Reduce rotationSpeed
- Check for NavMesh issues
- Verify colliders aren't conflicting

## File Structure

```
EnemyAI/
├── BaseEnemyAI.cs              # Core AI controller
├── EnemyStats.cs               # ScriptableObject stats
├── EnemyDetection.cs           # Detection system
├── EnemyEvent.cs               # Event definitions
├── EnemyEventListener.cs       # Event handler example
├── EnemyStateMachine.cs        # Simple wrapper
├── MeleeEnemyAI.cs            # Melee enemy example
├── RangedEnemyAI.cs           # Ranged enemy example
└── README.md                   # This file
```

## Dependencies

- **MonsterLove StateMachine** (Assets/Plugins/MonsterLove)
- **Astar Pathfinding Project** (Assets/AstarPathfindingProject)
- Unity 2020.3+ (recommended)

## Future Enhancements

Possible extensions:
- Behavior trees for more complex AI
- Group coordination (formations, flanking)
- Dynamic difficulty adjustment
- Cover system integration
- Multiple attack patterns
- Ability system

## Support

For issues or questions about:
- **MonsterLove FSM**: Check Assets/Plugins/MonsterLove/Samples
- **Astar Pathfinding**: https://arongranberg.com/astar/docs/
- **This System**: Review code comments and examples

---

**Happy Enemy AI Development!** 🎮
