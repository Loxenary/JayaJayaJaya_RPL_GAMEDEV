# ShowHide System Architecture

## System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                        Your Game Code                            │
│  (Buttons, Input Handlers, Game Logic, etc.)                    │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ Publishes Events
                              ▼
        ┌─────────────────────────────────────────┐
        │         EventBus (Mediator)             │
        │  - OpenUI<T>                            │
        │  - CloseUI<T>                           │
        │  - ToggleUI<T>                          │
        └─────────────────────────────────────────┘
                              │
                              │ Notifies Subscribers
                              ▼
        ┌─────────────────────────────────────────┐
        │     ShowHideBase / AutoEventBus         │
        │  - Subscribes to events                 │
        │  - Manages visibility state             │
        │  - Calls Show/Hide/Toggle               │
        └─────────────────────────────────────────┘
                              │
                              │ Executes
                              ▼
        ┌─────────────────────────────────────────┐
        │        Your UI Implementation           │
        │  - ShowInternal() - Your animation      │
        │  - HideInternal() - Your animation      │
        └─────────────────────────────────────────┘
```

## Class Hierarchy

```
MonoBehaviour
    │
    └─── ShowHideBase (Abstract)
            │
            ├─── ShowHideAutoEventBus<T> (Generic Abstract)
            │       │
            │       ├─── SettingsUI
            │       ├─── PauseMenuUI
            │       ├─── InventoryUI
            │       └─── (Your UI Classes...)
            │
            └─── (Or directly inherit ShowHideBase for manual control)
```

## Event Flow Diagram

### Opening a UI Panel

```
User Action (Button Click)
        │
        ▼
UIManager.Open<SettingsUI>()
   or EventBus.Publish(new OpenUI<SettingsUI>())
        │
        ▼
EventBus dispatches to subscribers
        │
        ▼
SettingsUI receives OpenUI<SettingsUI> event
        │
        ▼
OnOpenUIEvent() called
        │
        ▼
ShowUI() called
        │
        ├─── IsVisible = true
        ├─── IsTransitioning = true
        ├─── ShowUIStart() - virtual hook
        │
        ▼
ShowInternal() - YOUR IMPLEMENTATION
        │
        ├─── Your show animation
        │
        ▼
OnShowComplete() - YOU CALL THIS
        │
        ├─── IsTransitioning = false
        └─── ShowUIComplete() - virtual hook
```

### Closing a UI Panel

```
User Action (Close Button)
        │
        ▼
UIManager.Close<SettingsUI>()
   or EventBus.Publish(new CloseUI<SettingsUI>())
        │
        ▼
EventBus dispatches to subscribers
        │
        ▼
SettingsUI receives CloseUI<SettingsUI> event
        │
        ▼
OnCloseUIEvent() called
        │
        ▼
HideUI() called
        │
        ├─── IsTransitioning = true
        ├─── HideUIStart() - virtual hook
        │
        ▼
HideInternal() - YOUR IMPLEMENTATION
        │
        ├─── Your hide animation
        │
        ▼
OnHideComplete() - YOU CALL THIS
        │
        ├─── IsTransitioning = false
        ├─── IsVisible = false
        └─── HideUIComplete() - virtual hook
```

## Component Responsibilities

### ShowHideBase
**Responsibility:** Core UI visibility management
- Maintains visibility state (IsVisible, IsTransitioning)
- Provides lifecycle hooks (Start/Complete callbacks)
- Manages EventBus subscription lifecycle
- Template method pattern for Show/Hide behavior

**Public API:**
- `ShowUI()` - Show the UI
- `HideUI()` - Hide the UI
- `ToggleUI()` - Toggle visibility

**Protected API:**
- `ShowInternal()` - Abstract, implement your animation
- `HideInternal()` - Abstract, implement your animation
- `OnShowComplete()` - Call when show animation finishes
- `OnHideComplete()` - Call when hide animation finishes
- Virtual hooks: ShowUIStart, ShowUIComplete, HideUIStart, HideUIComplete

### ShowHideAutoEventBus<T>
**Responsibility:** Automatic EventBus integration
- Auto-subscribes to OpenUI<T>, CloseUI<T>, ToggleUI<T>
- Reduces boilerplate code
- Uses CRTP (Curiously Recurring Template Pattern)

**Key Feature:** You only need to implement ShowInternal/HideInternal!

### Event Structs (OpenUI<T>, CloseUI<T>, ToggleUI<T>)
**Responsibility:** Type-safe event data
- Generic events for each UI type
- Optional Data field for instance-specific operations
- Constraint: T must be ShowHideBase

### UIManager
**Responsibility:** Convenience API and tracking
- Static helper methods
- Fluent API for chaining operations
- Optional UI state tracking
- Cleaner syntax than raw EventBus calls

## Design Patterns Used

1. **Template Method Pattern**
   - ShowHideBase defines the algorithm skeleton (ShowUI/HideUI)
   - Derived classes implement specific steps (ShowInternal/HideInternal)

2. **Observer Pattern (via EventBus)**
   - UI panels subscribe to events
   - Publishers don't need to know about subscribers

3. **CRTP (Curiously Recurring Template Pattern)**
   - `ShowHideAutoEventBus<T> where T : ShowHideAutoEventBus<T>`
   - Enables type-safe auto-subscription

4. **Facade Pattern**
   - UIManager provides simplified interface to EventBus operations

5. **State Pattern**
   - IsVisible, IsTransitioning track UI state
   - State transitions managed internally

## Benefits of This Architecture

### 1. Loose Coupling
```csharp
// Caller doesn't need reference to UI instance
UIManager.Open<SettingsUI>();  // Works from anywhere!
```

### 2. Type Safety
```csharp
// Compiler checks the type
UIManager.Open<SettingsUI>();      // ✓ Correct
UIManager.Open<NotAUIClass>();     // ✗ Compile error
```

### 3. Extensibility
```csharp
// Easy to add new UI types
public class MyNewUI : ShowHideAutoEventBus<MyNewUI> { ... }
// Immediately works with EventBus and UIManager
```

### 4. Testability
```csharp
// Can test without Unity scene
EventBus.Publish(new OpenUI<SettingsUI>());
// Mock subscriptions for testing
```

### 5. Minimal Boilerplate
```csharp
// Only implement what matters - your animations!
public class MyUI : ShowHideAutoEventBus<MyUI>
{
    protected override void ShowInternal() { /* animation */ }
    protected override void HideInternal() { /* animation */ }
}
```

## Extension Points

You can extend the system by:

1. **Adding Custom Events**
   ```csharp
   public struct RefreshUI<T> where T : ShowHideBase { }
   ```

2. **Creating Specialized Base Classes**
   ```csharp
   public abstract class AnimatedUI<T> : ShowHideAutoEventBus<T>
   {
       // Common animation logic
   }
   ```

3. **Adding UIManager Methods**
   ```csharp
   public static void OpenExclusive<T>()
   {
       CloseAll();
       Open<T>();
   }
   ```

4. **Implementing Custom Hooks**
   ```csharp
   protected override void ShowUIStart()
   {
       // Custom logic before showing
   }
   ```

## Performance Considerations

- EventBus uses dictionary lookups: O(1) for publish/subscribe
- Generic events create separate types: `OpenUI<SettingsUI>` ≠ `OpenUI<PauseMenuUI>`
- Minimal memory overhead: events are structs
- Thread-safe: EventBus uses locking
- No garbage collection pressure from events (structs are value types)

## Comparison with Alternatives

| Approach | Coupling | Type Safety | Boilerplate | Flexibility |
|----------|----------|-------------|-------------|-------------|
| **Direct References** | High | ✓ | Low | Low |
| **String-based Events** | Low | ✗ | Medium | Medium |
| **Generic EventBus** | Low | ✓ | Low | High |
| **Unity Events** | Medium | ✓ | High | Medium |

This system combines the best of all approaches! 🎯
