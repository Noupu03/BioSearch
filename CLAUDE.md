# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

This is a Unity project (no CLI build commands). Open in Unity Editor and use Play mode. There is no test runner or lint step.

Use **Tools → 씬 셋업** in the Unity Editor menu to auto-wire all SerializeField references after code changes. Use **Tools → 씬 정리** to organize scene hierarchy into groups.

## Architecture

### Two-scene structure
- **StartScene**: title/entry screen. `GameSessionManager` (DontDestroyOnLoad) persists across to the game scene.
- **Game scene (scene ssh)**: all gameplay. Never reloaded during a run — the game loop works in-place.

### Game loop (no scene reload)
`GameLoopManager` is the single owner of the loop:
1. `SceneStartManager.Start()` → `GameLoopManager.RequestFirstStage()` → resets all `IStageResettable` → `GameEvents.RaiseStageStarted()`
2. Player inspects files, makes a decision → `SelectPopupManager` → `GameLoopManager.RequestNextStage()`
3. `DoNextStage()`: fade to black → `IStageResettable.ResetForNewStage()` on all resettables → camera return → fade in → `RaiseStageStarted()`
4. Game over → `GameEvents.RaiseGameOver(reason)` → `GameLoopManager.HandleGameOver()` → `SceneManager.LoadScene("StartScene")`

### IStageResettable
Interface with a single `ResetForNewStage()`. Implementors: `SanityManager`, `FileWindow`, `DummyIconSpawner`, `LogWindowManager`, `GameStateManager`, `TimerManager`. The list is stored as `MonoBehaviour[] stageResettables` in `GameLoopManager` (inspector-ordered, order matters).

### GameEvents (static event hub)
Loose coupling between managers. Key events: `OnSceneInitialized`, `OnGameOver(string)`, `OnSanityChanged(float,float)`, `OnScoreChanged`, `OnStageStarted`. Never subscribe in constructors — use `OnEnable`/`OnDisable`.

### Camera system
`HybridCameraController` owns all camera logic:
- Switches between camera1 (room view) and camera2 (monitor view)
- Subscribes to `InputManager.OnWPressed` / `OnSPressed` events (W: switch to monitor after delay, S: return to room)
- Camera2 internal views (view2/leftView/rightView) still use direct `Input.GetKeyDown` in `UpdateCamera2Input()`
- `ReturnToRoomView()` — called by `GameLoopManager.DoNextStage()` to snap camera back
- `InputManager.SimulateSPress()` — fires `OnSPressed` bypassing lock, used after stage reset so `CameraPostFX` also reacts

`CameraPostFX` subscribes to W/S events to lerp URP Volume (Bloom/Vignette/FilmGrain).

### InputManager
Singleton that translates key presses to events. S-press can be locked (`LockSInput`) during W→camera2 transition. `SimulateSPress()` bypasses the lock (for stage reset).

### FileWindow (partial class)
Split across three files: `FileWindow.cs` (core + lifecycle), `FileWindowMethods.cs` (rendering), `FileWindowNavigation.cs` (path/history). Folder tree is rebuilt from scratch each stage via `ResetForNewStage()`. `AbnormalDetector` is a separate static utility.

### ScoreCount / GameConfig
Both are static classes (no MonoBehaviour). `ScoreCount` tracks Success/Fail/Stage counts across stages. `GameConfig` holds magic-number constants (abnormal probabilities, log line limit, folder depth limit).

### SceneSetupTool (Editor only)
`Assets/Editor/SceneSetupTool.cs` — auto-wires all SerializeField references. Run **Tools → 씬 셋업** after structural changes. Also groups scene objects under named parents (**Tools → 씬 정리**).

## Naming conventions
- Managers: `XxxManager` (MonoBehaviour singletons with `public static Instance`)
- Static event hub: `GameEvents`
- Stage reset contract: `IStageResettable`
- UI creation in code (no prefab scene objects): `StageTransitionUI`
