# Enemy AI Animation System Setup

This guide explains how to set up and use the animation system for enemy AI.

## Overview

The Enemy AI uses an **EventBus-based animation system** that decouples animation logic from the AI state machine. This makes it easy to add, modify, and manage animations without touching the core AI code.

## Components

### 1. **EnemyAIAnimationListener** (Component)
- Attach this to your enemy GameObject (the one with the Animator)
- Listens to animation events from EventBus
- Automatically plays animations on the Animator
- Auto-assigns the Animator component

### 2. **EnemyAIAnimationEvent** (Events)
Contains all animation event types:
- `EnemyAnimationPlay` - Play/CrossFade animations
- `EnemyAnimationTrigger` - Set triggers
- `EnemyAnimationBool` - Set bool parameters
- `EnemyAnimationFloat` - Set float parameters
- `EnemyAnimationInt` - Set int parameters

### 3. **BaseEnemyAI** (Built-in Animation Methods)
The AI already has these animation methods built-in:
- `PlayWalkAnimation()` - Plays walk animation
- `PlayAttackAnimation()` - Plays attack animation
- `SetAnimationBool(name, value)` - Set bool parameter
- `SetAnimationFloat(name, value)` - Set float parameter
- `TriggerAnimation(name)` - Trigger an animation

## Quick Setup (3 Steps)

### Step 1: Add the Listener Component
1. Select your enemy GameObject in the scene
2. Add the `EnemyAIAnimationListener` component
3. The Animator will be auto-assigned

### Step 2: Configure Animation Names in BaseEnemyAI Inspector
In the **Animation Settings** section:
- **Walk Animation Name**: `"Walk"` (or your walk state name)
- **Attack Animation Name**: `"Attack"` (or your attack state name)
- **Use Animation CrossFade**: ✓ (recommended for smooth transitions)
- **Animation CrossFade Duration**: `0.15` (adjust for your preference)

### Step 3: Done!
The AI will automatically:
- Play **Walk** animation when patrolling, chasing, or moving
- Play **Attack** animation when attacking
- Use smooth crossfades between animations

## Animation State Flow

```
Idle State → (no animation change)
   ↓
Patrol State → Plays WALK animation
   ↓
Seen State → Plays WALK animation (slow movement)
   ↓
Chase State → Plays WALK animation (fast movement)
   ↓
Attack State → Plays ATTACK animation
   ↓
Flee State → Plays WALK animation
```

## Advanced Usage

### Custom Animation Events (from any script)

```csharp
// Play a specific animation
EventBus.Publish(EnemyAnimationPlay.Simple("Run"));

// Play with crossfade
EventBus.Publish(EnemyAnimationPlay.WithCrossFade("Jump", 0.2f));

// Set animator trigger
EventBus.Publish(EnemyAnimationTrigger.Create("Hit"));

// Set bool parameter
EventBus.Publish(EnemyAnimationBool.Create("IsRunning", true));

// Set float parameter (e.g., speed)
EventBus.Publish(EnemyAnimationFloat.Create("Speed", 2.5f));

// Set int parameter
EventBus.Publish(EnemyAnimationInt.Create("AttackType", 1));
```

### Override Animation Methods in Custom Enemy

```csharp
public class CustomEnemy : BaseEnemyAI
{
    protected override void PlayWalkAnimation()
    {
        // Custom walk logic
        if (isAngry)
        {
            EventBus.Publish(EnemyAnimationPlay.Simple("AngryWalk"));
        }
        else
        {
            base.PlayWalkAnimation(); // Use default
        }
    }

    protected override void PlayAttackAnimation()
    {
        // Play different attack based on level
        string attackAnim = angryLevel switch
        {
            EnemyLevel.FIRST => "Attack_Light",
            EnemyLevel.FOURTH => "Attack_Heavy",
            _ => attackAnimationName
        };

        EventBus.Publish(EnemyAnimationPlay.WithCrossFade(attackAnim, 0.1f));
    }
}
```

### Add More Animations to States

```csharp
protected override void Idle_Enter()
{
    base.Idle_Enter();
    // Play idle animation
    EventBus.Publish(EnemyAnimationPlay.Simple("Idle"));
}

protected override void Flee_Enter()
{
    base.Flee_Enter();
    // Play flee animation
    EventBus.Publish(EnemyAnimationPlay.WithCrossFade("Flee", 0.2f));
}
```

## Troubleshooting

### Animations not playing?
1. ✓ Check that `EnemyAIAnimationListener` is attached to the GameObject
2. ✓ Verify the Animator component is assigned in the listener
3. ✓ Make sure animation names match your Animator state names exactly (case-sensitive!)
4. ✓ Enable debug logs in the listener to see what's happening

### Wrong animation playing?
1. Check the animation names in the Inspector match your Animator states
2. Verify the Animator Controller has the states you're referencing

### Animations are choppy?
1. Enable "Use Animation CrossFade" in Inspector
2. Increase the "Animation CrossFade Duration" to 0.2-0.3 seconds

## Benefits of This System

✓ **Decoupled** - Animation logic separate from AI logic
✓ **Flexible** - Easy to add new animations without modifying AI code
✓ **Reusable** - Trigger animations from anywhere using EventBus
✓ **Override-friendly** - Easy to customize in derived classes
✓ **Debug-friendly** - Optional logging shows exactly what's happening
