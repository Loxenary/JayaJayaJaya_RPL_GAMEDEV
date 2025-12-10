# Enemy Angry System - Dynamic Point Calculations

## Formula Overview

The new system uses a **dynamic calculation** based on player state instead of static points per action.

### Core Formula

```
AngryPoints = MaxPoints × (ItemWeight × ItemFactor^ItemExp + SanityWeight × SanityFactor^SanityExp)
```

Where:
- **ItemFactor** = itemsTaken / totalItems (0-1, normalized)
- **SanityFactor** = (maxSanity - currentSanity) / maxSanity (0-1, inverted so low sanity = high anger)
- **ItemExp** = Exponential curve for items (1.5 recommended for smooth progression)
- **SanityExp** = Exponential curve for sanity (2.0 recommended for aggressive low-sanity punishment)

## Configuration Parameters

### Recommended Values
- **Total Puzzle Items**: 5
- **Max Sanity**: 100
- **Item Weight**: 0.6 (60% importance)
- **Sanity Weight**: 0.4 (40% importance)
- **Item Exponent**: 1.5 (smooth exponential curve)
- **Sanity Exponent**: 2.0 (quadratic, punishes low sanity harder)
- **Max Angry Points**: 100
- **Base Points Per Second**: 0.5

## Calculated Point Baselines for Each Level

Given the formula and parameters above, here are the **balanced point baselines** for each difficulty level:

### Scenario Analysis

Let's analyze key scenarios to find balanced thresholds:

#### Scenario 1: First Item, Max Sanity (Best Case)
- Items: 1/5 = 0.2
- Sanity: 100/100 = 0 (inverted)
- ItemFactor^1.5 = 0.2^1.5 = 0.089
- SanityFactor^2 = 0^2 = 0
- **Points = 100 × (0.6 × 0.089 + 0.4 × 0) = 5.4 points**

#### Scenario 2: First Item, Low Sanity (20)
- Items: 1/5 = 0.2
- Sanity: (100-20)/100 = 0.8
- ItemFactor^1.5 = 0.089
- SanityFactor^2 = 0.8^2 = 0.64
- **Points = 100 × (0.6 × 0.089 + 0.4 × 0.64) = 30.9 points**

#### Scenario 3: Half Items (3/5), Medium Sanity (50)
- Items: 3/5 = 0.6
- Sanity: (100-50)/100 = 0.5
- ItemFactor^1.5 = 0.6^1.5 = 0.465
- SanityFactor^2 = 0.5^2 = 0.25
- **Points = 100 × (0.6 × 0.465 + 0.4 × 0.25) = 37.9 points**

#### Scenario 4: Most Items (4/5), Low Sanity (20)
- Items: 4/5 = 0.8
- Sanity: (100-20)/100 = 0.8
- ItemFactor^1.5 = 0.8^1.5 = 0.716
- SanityFactor^2 = 0.8^2 = 0.64
- **Points = 100 × (0.6 × 0.716 + 0.4 × 0.64) = 68.6 points**

#### Scenario 5: All Items, Minimum Sanity (20)
- Items: 5/5 = 1.0
- Sanity: (100-20)/100 = 0.8
- ItemFactor^1.5 = 1.0^1.5 = 1.0
- SanityFactor^2 = 0.8^2 = 0.64
- **Points = 100 × (0.6 × 1.0 + 0.4 × 0.64) = 85.6 points**

#### Scenario 6: All Items, Zero Sanity (Worst Case)
- Items: 5/5 = 1.0
- Sanity: (100-0)/100 = 1.0
- ItemFactor^1.5 = 1.0
- SanityFactor^2 = 1.0^2 = 1.0
- **Points = 100 × (0.6 × 1.0 + 0.4 × 1.0) = 100 points**

---

## Recommended Level Baselines

Based on the scenarios above, here are the **balanced point thresholds** for each enemy level:

### FIRST Level (Starting)
- **Baseline: 0 points**
- **Trigger**: Game start
- **Behavior**: Passive, slow enemy

### SECOND Level (Early Pressure)
- **Baseline: 25 points**
- **Trigger Examples**:
  - 1 item + sanity at 20
  - 2 items + sanity at 60
- **Behavior**: Slightly aggressive, increased speed

### THIRD Level (Mid-Game Challenge)
- **Baseline: 50 points**
- **Trigger Examples**:
  - 3 items + sanity at 50
  - 4 items + sanity at 80
- **Behavior**: Very aggressive, much faster, better detection

### FOURTH Level (Maximum Threat)
- **Baseline: 75 points**
- **Trigger Examples**:
  - 4 items + sanity at 20
  - 5 items + sanity at 40+
- **Behavior**: Extremely aggressive, maximum speed and detection

---

## Summary Table

| Level | Points | Items (at Sanity 50) | Items (at Sanity 20) | Description |
|-------|--------|---------------------|---------------------|-------------|
| FIRST | 0 | 0 | 0 | Starting level |
| SECOND | 25 | 1-2 items | 1 item | Early pressure |
| THIRD | 50 | 3 items | 2-3 items | Mid-game challenge |
| FOURTH | 75 | 4-5 items | 4 items | Maximum threat |

---

## Configuration File Setup

In Unity, create/modify your `EnemyAngryConfiguration` asset with these values:

### EnemyAngryPointConfiguration
```
Total Puzzle Items: 5
Max Sanity: 100
Item Weight: 0.6
Sanity Weight: 0.4
Item Exponent: 1.5
Sanity Exponent: 2.0
Max Angry Points: 100
Base Points Per Second: 0.5
```

### EnemyAngryConfiguration (Level Baselines)
```
FIRST Level:  angryPointBaseline = 0
SECOND Level: angryPointBaseline = 25
THIRD Level:  angryPointBaseline = 50
FOURTH Level: angryPointBaseline = 75
```

---

## How It Works

1. **Dynamic Calculation**: Points are recalculated whenever items are taken or sanity changes
2. **Progressive Difficulty**: The exponential curves ensure smooth progression
3. **Balanced Scaling**:
   - Items matter more (60%) for strategic progression
   - Sanity matters less (40%) but heavily punishes very low values (quadratic)
4. **Time-Based Accumulation**: Points slowly accumulate over time based on current state
   - Higher item count + lower sanity = faster accumulation

## Testing Tips

- Start with max sanity → Should be FIRST level even with 1 item
- Take all items with max sanity → Should reach ~60 points (THIRD level)
- Lower sanity to 20 with all items → Should reach ~85 points (FOURTH level)
- Lower sanity to 0 with all items → Should hit 100 points (max)

