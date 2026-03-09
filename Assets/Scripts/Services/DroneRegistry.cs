using System.Collections.Generic;
using DroneDispatcher.Core;
using DroneDispatcher.Models;
using UnityEngine;
using Zenject;

namespace DroneDispatcher.Services
{
public class DroneRegistry : IDroneRegistry
{
    readonly SignalBus _signalBus;
    readonly List<DroneModel> _drones = new List<DroneModel>();

    public IReadOnlyList<DroneModel> Drones => _drones;

    public DroneRegistry(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    public void Register(DroneModel drone)
    {
        if (_drones.Exists(d => d.Id == drone.Id))
            return;

        drone.OnStateChanged += HandleDroneStateChanged;
        _drones.Add(drone);
        Debug.Log($"[DroneRegistry] Registered drone: {drone.DisplayName}");
    }

    public DroneModel GetDrone(string droneId)
    {
        return _drones.Find(d => d.Id == droneId);
    }

    void HandleDroneStateChanged(DroneModel drone)
    {
        _signalBus.Fire(new DroneStateChangedSignal { DroneId = drone.Id });
    }
}
}
