using System;
using System.Collections.Generic;
using DroneDispatcher.Core;
using DroneDispatcher.Models;
using DroneDispatcher.Services;
using Zenject;

namespace DroneDispatcher.MVVM.ViewModels
{
// ViewModel for the drone list panel. Similar pattern to JobPanelViewModel.
// Subscribes to DroneStateChangedSignal so the UI refreshes when any drone's state changes.
// Implements IDisposable for cleanup when the container is destroyed.
public class DronePanelViewModel : IDisposable
{
    readonly IDroneRegistry _registry;
    readonly SignalBus _signalBus;

    // View subscribes to OnDronesUpdated to rebuild the drone list
    public event Action OnDronesUpdated;
    // Fired when user selects a different drone (used by camera feed view)
    public event Action<string> OnSelectedDroneChanged;

    public IReadOnlyList<DroneModel> Drones => _registry.Drones;
    public string SelectedDroneId { get; private set; }

    public DronePanelViewModel(IDroneRegistry registry, SignalBus signalBus)
    {
        _registry = registry;
        _signalBus = signalBus;
        // Subscribe immediately — we want to catch state changes from the very beginning
        _signalBus.Subscribe<DroneStateChangedSignal>(OnDroneStateChanged);
    }

    public void Dispose()
    {
        _signalBus.TryUnsubscribe<DroneStateChangedSignal>(OnDroneStateChanged);
    }

    // Called by DronePanelView when user clicks a drone row
    public void SelectDrone(string droneId)
    {
        SelectedDroneId = droneId;
        OnSelectedDroneChanged?.Invoke(droneId);  // camera feed view listens to this
    }

    // Called by DronePanelView after a 1-frame delay (drones register in Start)
    public void Refresh()
    {
        OnDronesUpdated?.Invoke();
    }

    // Any drone state change triggers a full list refresh in the UI
    void OnDroneStateChanged(DroneStateChangedSignal signal)
    {
        OnDronesUpdated?.Invoke();
    }
}
}
