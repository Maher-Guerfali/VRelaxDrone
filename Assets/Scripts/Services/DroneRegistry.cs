using System.Collections.Generic;
using DroneDispatcher.Core;
using DroneDispatcher.Models;
using UnityEngine;
using Zenject;

namespace DroneDispatcher.Services
{
// Keeps track of all drones in the scene.
// Each DroneController creates a DroneModel in its Start() method and calls Register() here.
// This service also bridges local DroneModel.OnStateChanged events to the Zenject SignalBus,
// so the UI layer can react to drone state changes without knowing about DroneModel directly.
public class DroneRegistry : IDroneRegistry
{
    readonly SignalBus _signalBus;
    readonly List<DroneModel> _drones = new List<DroneModel>();

    public IReadOnlyList<DroneModel> Drones => _drones;

    public DroneRegistry(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    // Called by DroneController.Start() for each drone in the scene
    public void Register(DroneModel drone)
    {
        // Prevent duplicate registrations (safety check)
        if (_drones.Exists(d => d.Id == drone.Id))
            return;

        // Subscribe to state changes so we can forward them to the SignalBus
        drone.OnStateChanged += HandleDroneStateChanged;
        _drones.Add(drone);
        Debug.Log($"[DroneRegistry] Registered drone: {drone.DisplayName}");
    }

    public DroneModel GetDrone(string droneId)
    {
        return _drones.Find(d => d.Id == droneId);
    }

    // Forward drone state changes to the global SignalBus.
    // DronePanelViewModel subscribes to this signal to refresh the drone list UI.
    void HandleDroneStateChanged(DroneModel drone)
    {
        _signalBus.Fire(new DroneStateChangedSignal { DroneId = drone.Id });
    }
}
}
