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
4. Game over → `GameEvents.RaiseGameOver(reason)` → `GameLoopManager.HandleGameOver()` → `SceneService.Instance.LoadScene(SceneName.StartScene)` (Haare, Addressables-based — see below)

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

### Haare framework (`Assets/Scripts/Haare`)
Adoption of [HaareFramework](https://github.com/kirihara-unity/HaareFramework). The existing `GameLoopManager`/`GameEvents`/singleton-manager architecture above still owns the core loop — Haare pieces are wired into specific integration points (scene transition, popup UI) rather than replacing the whole architecture.

**Core** (`Haare.Client.Core`/`Haare.Client.Routine`): `Processor` (`SingletonMonoBehaviour<Processor>`, centrally drives Update/LateUpdate/FixedUpdate for all registered routines), `MonoRoutine`/`NativeRoutine` (unified lifecycle — MonoRoutine is MonoBehaviour-based, NativeRoutine is plain C#), `Singleton<T>`/`SingletonMonoBehaviour<T>`. `HaareClient.Main()` (`RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`) bootstraps `Processor` on startup, skipping `EventSystem`/`AudioListener` creation since BioSearch's scenes already place both.

**DI**: `GameLifetimeScope` (`Assets/Scripts/Haare/Client/Core/DI/GameLifetimeScope.cs`, global namespace) is the VContainer root scope. **Tools → 씬 셋업** creates it if missing and also creates/wires the `CoreUIManager` prefab (see below) into its `coreUIManagerPrefab` field — idempotent, safe to re-run.

**Scene transition**: `SceneService` (`Haare.Client.Routine.Service.SceneService`, `Singleton<SceneService>` — not VContainer-injected, so plain MonoBehaviour singletons like `GameLoopManager` can call `SceneService.Instance` directly) loads scenes via Addressables. `SceneName { StartScene, SceneSSH }` maps to explicit Addressable addresses (`"StartScene"`, `"SceneSSH"` — **not** filenames; `"scene ssh"` has a space and can't be an enum name) registered by **Tools → Haare Addressables 셋업** (`Assets/Editor/HaareAddressablesSetupTool.cs`). `GameLoopManager.DoGameOver()` calls `SceneService.Instance.LoadScene(SceneName.StartScene).ToCoroutine()` instead of `SceneManager.LoadScene`. The loading-screen variant (`LoadSceneWithLoad`, tied to `CoreUIManager`) was not ported — only the plain `LoadScene()` is wired in.

**UI Panel system** (`Haare.Client.UI`): `ICustomPanel`/`IPanelData`/`PanelAttribute` + `SceneUIManager`/`CoreUIManager` (stack-based panel loader, Addressable-prefab-per-type via `[Panel("address")]`). `CoreUIManager` lives on a generated prefab (`Assets/Prefabs/Haare/CoreUIManager.prefab`, Canvas + `PanelRoot` RectTransform as `safePannelRect`), registered `DontDestroyOnLoad` singleton in `GameLifetimeScope`. `SelectPopupManager`/`SelectPopup` were migrated to this system: `SelectPopup` implements `ICustomPanel` and is data-driven (`SelectPopupData.Question`) so one Addressable prefab (`Assets/Prefabs/UIObject/SelectPopup.prefab`, address `"SelectPopup"`) covers both the accept and release questions — the old two-prefab (`SelectPopupSample 1/2.prefab`) approach didn't fit `[PanelAttribute]`'s one-type-one-address model. **Tools → Haare SelectPopup 셋업** (`Assets/Editor/HaareSelectPopupSetupTool.cs`) does the one-time prefab migration + Addressable registration. `SelectPopup.OpenPanel()` must call `gameObject.SetActive(true)` — `SceneUIManager.Register()` instantiates panels inactive, and only the `LoadPanel<Ttype,Tdata>` overload calls `OpenPanel()` after `SetData()`.

**DataManager** (`Haare.Scripts.Client.Data`): JSON ⇄ model caching, ported but **not connected to anything** — BioSearch has no JSON data source (`ScoreCount`/`GameConfig` are static classes and stay that way). Available for future features that need it.

**AssetLoader** (`Haare.Util.Loader`): Addressables/UniTask wrapper used by `SceneUIManager.Register()` and `DataManager`. `AssetPath.cs` (the demo's hardcoded path constants) was **not** ported — `SceneService` and the Panel system each keep their own explicit address mapping instead.

Not brought in: `GamePresenter`/`IPresenter`/`UIPresenter` (VContainer Presenter layer — no current consumer).

Dependencies added to `Packages/manifest.json`: `com.cysharp.r3` (R3 reactive streams), `com.cysharp.unitask` (UniTask async), `jp.hadashikick.vcontainer` (VContainer DI), `com.github-glitchenzo.nugetforunity`, `com.unity.addressables` — all via git URL/version, so the Editor must resolve them (needs network access) before the project compiles. DOTween binaries live at `Assets/Plugins/Demigiant/DOTween` (no `DOTweenSettings.asset` — DOTween falls back to built-in defaults).

**Important**: `com.cysharp.r3` (the UPM package) only wraps Unity-specific extensions — R3's actual core types (`Observable<>`, `Subject<>`, `Unit`, `ReactiveProperty<>`, etc.) ship as a **NuGet** package, not UPM. `Assets/packages.config` + `Assets/NuGet.config` declare that NuGet dependency (R3 + BCL polyfills: `Microsoft.Bcl.AsyncInterfaces`, `Microsoft.Bcl.TimeProvider`, `System.ComponentModel.Annotations`, `System.Runtime.CompilerServices.Unsafe`, `System.Threading.Channels`), restored via NuGetForUnity into `Assets/Packages` (gitignored — regenerated per machine). If R3 types fail to resolve after opening the Editor, use **NuGet → Restore Packages** from the Unity menu to force it.

**Setup order for a fresh clone / after pulling these changes**: 1) let Package Manager resolve the git packages, 2) **Tools → Haare Addressables 셋업**, 3) **Tools → Haare SelectPopup 셋업**, 4) **Tools → 씬 셋업**. Steps 2-3 only need to run once per machine (results are saved as assets), but re-running is safe (idempotent).

## Naming conventions
- Managers: `XxxManager` (MonoBehaviour singletons with `public static Instance`)
- Static event hub: `GameEvents`
- Stage reset contract: `IStageResettable`
- UI creation in code (no prefab scene objects): `StageTransitionUI`
