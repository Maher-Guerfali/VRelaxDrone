# Mini Drone Dispatcher

A small dispatch sim where the player assigns delivery drones to pickup/delivery jobs.  
Built as an architecture assignment — focus on code quality, DI, and MVVM, not art.

---

## How to Run

1. Open the project in **Unity 6** (6000.3.x).
2. Open `Assets/Scenes/SampleScene`.
3. Make sure the scene has:
   - A **SceneContext** GameObject with `GameInstaller` added under Mono Installers.
   - A baked **NavMeshSurface** on the ground plane (Component → AI → NavMesh Surface → Bake).
   - 3 drone GameObjects with `DroneController` + `NavMeshAgent` + `DroneVisuals`.
   - 6 waypoint GameObjects named: `Hospital`, `Clinic`, `Warehouse`, `Residential`, `Restaurant`, `Office`.
   - 3 `JobDefinition` ScriptableObject assets in `Assets/Resources/Jobs/`.
   - A Canvas with `JobPanelView`, `DronePanelView`, `DispatchPanelView`.
4. Hit **Play**.

### Creating the Job Definitions

Right-click in Project → **Create → Drone Dispatcher → Job Definition**.  
Create 3 assets in `Assets/Resources/Jobs/`:

| Asset name       | jobId  | jobName          | pickupLocationName | dropoffLocationName |
|------------------|--------|------------------|--------------------|---------------------|
| MedicalSupply    | job_01 | Medical Supply   | Hospital           | Clinic              |
| PackageDelivery  | job_02 | Package Delivery | Warehouse          | Residential         |
| FoodDelivery     | job_03 | Food Delivery    | Restaurant         | Office              |

---

## High-Level Architecture

```
┌──────────────────────────────────────────────────────┐
│ Zenject (SceneContext + GameInstaller)                │
│                                                      │
│  ┌─────────────┐  ┌───────────────┐  ┌───────────┐  │
│  │ IJobService  │  │IDroneRegistry │  │IDispatcher │  │
│  │  (JobService)│  │(DroneRegistry)│  │(Dispatcher)│  │
│  └──────┬──────┘  └──────┬────────┘  └─────┬─────┘  │
│         │                │                  │        │
│         ▼                ▼                  ▼        │
│  ┌─────────────────────────────────────────────────┐ │
│  │              Zenject SignalBus                   │ │
│  │   JobStatusChangedSignal  DroneStateChanged...  │ │
│  └──────────┬──────────────────────┬───────────────┘ │
│             │                      │                 │
│  ┌──────────▼──────────┐  ┌───────▼──────────────┐  │
│  │   JobPanelViewModel │  │ DronePanelViewModel   │  │
│  │  DispatchViewModel  │  │                       │  │
│  └──────────┬──────────┘  └───────┬──────────────┘  │
│             │(C# events)          │(C# events)       │
│  ┌──────────▼──────────┐  ┌───────▼──────────────┐  │
│  │  JobPanelView (UI)  │  │  DronePanelView (UI) │  │
│  │DispatchPanelView(UI)│  │                       │  │
│  └─────────────────────┘  └──────────────────────┘  │
│                                                      │
│  ┌─────────────────┐                                 │
│  │ DroneController  │  NavMeshAgent + state machine  │
│  │ DroneVisuals     │  DOTween hover/bob             │
│  └─────────────────┘                                 │
└──────────────────────────────────────────────────────┘
```

### Layers

| Layer | Responsibility |
|-------|---------------|
| **Core** | Enums, signal types — zero dependencies |
| **Data** | ScriptableObject definitions (JobDefinition) |
| **Models** | Runtime state (JobModel, DroneModel) — plain C#, no MonoBehaviour |
| **Services** | Business logic behind interfaces (JobService, DroneRegistry, Dispatcher) |
| **MVVM/ViewModels** | Presentation logic — subscribes to signals, exposes state + events |
| **MVVM/Views** | MonoBehaviours on UI GameObjects — binds to ViewModel events, zero logic |
| **Drone** | MonoBehaviour controllers — NavMeshAgent movement + DOTween visuals |
| **Installers** | Zenject wiring — single MonoInstaller on SceneContext |

### Key Patterns

- **Dependency Injection**: Zenject wires everything. Services are bound as interfaces so they can be swapped (e.g. for testing or a different navigation backend).
- **MVVM**: Models hold data, ViewModels hold presentation state and subscribe to signals, Views are dumb renderers that subscribe to ViewModel C# events.
- **Signals**: Decouples services from UI. When a job status changes, `JobService` fires a signal → `JobPanelViewModel` reacts → `JobPanelView` rebuilds the list.

---

## What I Prioritised

1. **Clean separation of concerns** — each layer has one job, interfaces everywhere.
2. **DI done properly** — not just constructor injection, but interface-based bindings so you could swap `IJobService` for a mock.
3. **MVVM** — reactive UI via signals and C# events, no polling, no `Update()` in UI code.
4. **Reliable navigation** — Unity NavMesh is battle-tested; DOTween adds visual polish.
5. **Code readability** — short files, clear naming, comments only where non-obvious.

I deprioritised visuals entirely — everything is placeholder cubes/capsules.

---

## What I'd Improve With More Time

- **Object pooling** for UI entries instead of Destroy/Instantiate on every rebuild.
- **Auto-dispatch mode** — an AI scheduler that assigns idle drones to pending jobs automatically.
- **Job creation at runtime** — let the player create custom jobs with a form.
- **Drone priority / battery** — add constraints that affect which drone gets which job.
- **Unit tests** — Zenject makes this easy; mock services and test ViewModels in isolation.
- **Addressables** instead of `Resources.LoadAll` for job definitions.
- **Better visual feedback** — world-space markers, line renderers for paths, minimap.
- **VR interaction** — ray-based UI for assigning jobs in headset.
