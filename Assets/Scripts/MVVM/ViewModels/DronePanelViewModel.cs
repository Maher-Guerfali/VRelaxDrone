using System;
using System.Collections.Generic;
using DroneDispatcher.Core;
using DroneDispatcher.Models;
using DroneDispatcher.Services;
using Zenject;

namespace DroneDispatcher.MVVM.ViewModels
{
public class DronePanelViewModel : IDisposable
{
    readonly IDroneRegistry _registry;
    readonly SignalBus _signalBus;

    public event Action OnDronesUpdated;
    public event Action<string> OnSelectedDroneChanged;

    public IReadOnlyList<DroneModel> Drones => _registry.Drones;
    public string SelectedDroneId { get; private set; }

    public DronePanelViewModel(IDroneRegistry registry, SignalBus signalBus)
    {
        _registry = registry;
        _signalBus = signalBus;
        _signalBus.Subscribe<DroneStateChangedSignal>(OnDroneStateChanged);
    }

    public void Dispose()
    {
        _signalBus.TryUnsubscribe<DroneStateChangedSignal>(OnDroneStateChanged);
    }

    public void SelectDrone(string droneId)
    {
        SelectedDroneId = droneId;
        OnSelectedDroneChanged?.Invoke(droneId);
    }

    // Let View call this after drones register themselves in Start()
    public void Refresh()
    {
        OnDronesUpdated?.Invoke();
    }

    void OnDroneStateChanged(DroneStateChangedSignal signal)
    {
        OnDronesUpdated?.Invoke();
    }
}
}
