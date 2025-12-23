# CLAUDE.md

This file provides guidance to Claude Code when working with this repository.

## Project Overview

**BubblePuzzle** - Unity 6 hexagonal bubble shooter (2D/URP)

**Tech Stack**:
- Unity 6000.0.59f2
- URP 17.0.4 (2D renderer)
- New Input System 1.14.2
- Resolution: 1024x1920 (portrait)

**Game Concept**: Match-3 bubble shooter with hexagonal grid, gravity physics, wall reflection

---

## Core Game Specifications

| Category | Details |
|----------|---------|
| **Resolution** | 1024 x 1920 (portrait) |
| **Grid** | Hexagonal (6 neighbors per bubble) |
| **Boundaries** | Left/right walls (bubbles don't attach to walls) |
| **Bubble Colors** | Red, Blue, Green, Yellow, Purple |
| **Match Rule** | 3+ same color connected → destroy all |
| **Gravity** | Bubbles not connected to top fall |
| **Shooter** | Bottom-center, crossbow-style |
| **Aiming** | Mouse hold → guide line with **max 1 reflection** |
| **Reflection** | Angle of incidence = angle of reflection |
| **Preview** | Hexagonal frame at placement point |

**Game Flow**: Aim → Shoot → Place → Match Check → Destroy → Gravity Check → Fall

---

## Project Structure

```
Assets/Scripts/
├── Core/
│   ├── HexGrid.cs              # Coordinate system
│   ├── HexCoordinate.cs        # Axial coords (q, r)
│   └── GameManager.cs          # Main controller
├── Bubble/
│   ├── Bubble.cs               # Entity component
│   ├── BubbleType.cs           # Color enum
│   └── BubblePoolManager.cs    # Object pooling
├── Shooter/
│   ├── BubbleShooter.cs        # Input & shooting
│   ├── TrajectoryCalculator.cs # Reflection (max 1)
│   └── AimGuide.cs             # LineRenderer
├── Grid/
│   ├── BubbleGrid.cs           # Dictionary state
│   ├── GridRenderer.cs         # Visualization
│   └── PlacementValidator.cs   # Valid positions
├── GameLogic/
│   ├── MatchDetector.cs        # BFS (3+)
│   ├── DestructionHandler.cs   # Destroy + effects
│   └── GravityChecker.cs       # DFS from top
└── UI/
    ├── GameUI.cs               # Score, etc.
    └── PreviewFrame.cs         # Hex preview
```

---

## Implementation Phases

### Phase 1: Foundation
- [ ] HexCoordinate struct (axial: q, r)
- [ ] HexGrid coordinate system
- [ ] Bubble prefab (5 colors)
- [ ] Game area (1024x1920) + wall colliders
- [ ] Test: Manual hex placement

### Phase 2: Shooter & Aiming
- [ ] Mouse input (hold/release)
- [ ] AimGuide (LineRenderer)
- [ ] TrajectoryCalculator (max 1 reflection)
- [ ] Hex preview frame
- [ ] Test: Aim + reflection visualization

### Phase 3: Core Loop
- [ ] Shoot animation along trajectory
- [ ] Snap to nearest hex position
- [ ] MatchDetector (BFS, 3+)
- [ ] DestructionHandler (chain destroy)
- [ ] GravityChecker (DFS from top)
- [ ] Fall animation
- [ ] Test: Full cycle

### Phase 4: Polish
- [ ] VFX (pop, sparkle, fall)
- [ ] SFX (shoot, pop, fall)
- [ ] Animations (wobble, destruction)
- [ ] UI (score, combo)
- [ ] Level loading

---

## Key Algorithms

### 1. Hexagonal Coordinates (Axial)
**Structure**: `HexCoordinate { int q, int r }`
- **6 Neighbors**: Right, TopRight, TopLeft, Left, BottomLeft, BottomRight
- **Offset vectors**: `[(1,0), (1,-1), (0,-1), (-1,0), (-1,1), (0,1)]`
- **World position**: `x = size * (3/2 * q)`, `y = size * (√3 * r + √3/2 * q)`

### 2. Grid Management
- **Storage**: `Dictionary<HexCoordinate, Bubble>`
- **Operations**: PlaceBubble, GetBubble, GetNeighbors (6 directions)

### 3. Trajectory with Reflection
- **Input**: Origin, direction
- **Process**:
  1. Raycast → wall hit?
  2. Yes → reflect (Vector2.Reflect), raycast again (NO MORE)
  3. No → hit bubble or max distance
- **Output**: Point array for animation

### 4. Match Detection (BFS)
- **Start**: Placed bubble
- **Process**: Queue-based BFS, check 6 neighbors, same color only
- **Output**: List of bubbles (if count ≥ 3)

### 5. Gravity Check (DFS)
- **Start**: All bubbles in top row (r=0)
- **Process**: DFS to find connected set
- **Output**: All bubbles NOT in connected set → fall

---

## Unity Implementation Notes

### Input System
- Use `Mouse.current.leftButton.isPressed` for hold detection
- Convert screen to world: `Camera.main.ScreenToWorldPoint()`

### Physics2D Layers
| Layer | Name | Collides With |
|-------|------|---------------|
| 8 | Bubble | Bubble, Wall |
| 9 | Wall | Bubble, BubbleShooter |
| 10 | BubbleShooter | None (trigger only) |

### Performance Critical
- **Object Pooling**: Required for bubbles (Queue-based)
- **Dictionary Grid**: Faster than 2D arrays
- **Cache References**: No FindObjectOfType in Update
- **Batch Destruction**: Single frame, not per-bubble
- **Sprite Atlas**: Combine bubble sprites

### Animation Pattern
- Use `Coroutine` for: Launch trajectory, fall, destruction
- Use `Lerp` for smooth movement between points

### Debug Visualization
- `OnDrawGizmos()`: Draw hex grid, neighbor connections
- Debug keys: D (toggle debug), C (clear), R (reload)

---

## Critical Rules

1. **Maximum 1 reflection per shot** (game rule)
2. **Hexagonal coordinate math** (6 neighbors, axial coords)
3. **Object pooling** (performance requirement)
4. **Match check → Destroy → Gravity check → Fall** (execution order)
5. **Dictionary for grid** (not 2D array)
6. **DFS from top row** (gravity algorithm)
7. **BFS for matching** (3+ same color)

---

## Code Conventions

- PascalCase: Classes, methods, public fields
- camelCase: Private fields, locals
- `[SerializeField]`: Inspector-visible private fields
- Explicit types: `List<Bubble>` over `var`
