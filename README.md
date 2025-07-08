# HVM - Hunter vs Monster

**HVM** is a prototype survival shooter game built with Unity, designed for scalability, maintainability, and cross-platform support. The project emphasizes clean architecture, modular logic separation, and flexible multi-input control support.

---

## Technologies & Architecture

### Object Pooling
- Efficiently manages and reuses enemies, bullets, and particles.
- Reduces runtime instantiation overhead for better performance.

### UniRx (Reactive Programming)
- Implements reactive flow for UI updates, input handling, cooldowns, fade effects, and more.
- Improves code readability, maintainability, and modularity.

### Modular Architecture
- Clean separation between core systems: `EnemyManager`, `WeaponSystem`, `InputManager`, `UIManager`, `GameManager`, `AudioManager` and `ParticleManager`.
- Easily extensible and unit-testable components.

### Multi-Control Input Support
- Supports Joystick (mobile), Keyboard, and Gamepad via Unity Input System.
- Input abstraction via interface allows for easy expansion and platform adaptation.

### Finite State Machine (FSM)
- Player and enemies operate using a simple FSM: Idle → Chase → Attack → Death.
- Designed for future expansion into more complex behaviors.

### Multiplatform Ready
- Built to run on PC, Android, and iOS.
- UI scales well across various screen sizes using flexible layout and canvas scaler.

---

## Future Development Plans

### FSM Upgrade
- Decouple FSM logic into reusable State Pattern or Scriptable FSM architecture.
- Extend support for Behavior Trees or Utility AI for smarter enemy logic.

### Tooling & Editor Extensions
- Data Import/Export Tool: JSON or Excel import/export for enemies, weapons, and levels.
- MapSetting Tool: editor-friendly map configuration and prefab management.
- LevelSetting Tool: editor interface to define win/loss conditions and spawn patterns.

### Plugin & Service Integration
- Enum-based AudioManager and ParticleManager system.
- Future support for localization, save/load system, and analytics.
