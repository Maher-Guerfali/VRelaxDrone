using System;
using DroneDispatcher.Core;
using UnityEngine;

namespace DroneDispatcher.Models
{
// Runtime state for a single drone. Plain C# class, NOT a MonoBehaviour.
// DroneController (the MonoBehaviour on the actual GameObject) creates this in Start()
// and registers it with DroneRegistry so the rest of the app can track drone state.
public class DroneModel
{
    public string Id { get; }                      // unique id like "Drone_01"
    public string DisplayName { get; }             // friendly name like "Alpha"
    public DroneState State { get; private set; }  // current state in the state machine
    public string CurrentJobId { get; private set; } // which job this drone is working on (null if idle)
    public Vector3 BasePosition { get; }           // where the drone was spawned — it returns here after a job

    // Observer pattern — DroneRegistry subscribes here and fires a Zenject signal
    public event Action<DroneModel> OnStateChanged;

    public DroneModel(string id, string displayName, Vector3 basePosition)
    {
        Id = id;
        DisplayName = displayName;
        BasePosition = basePosition;
        State = DroneState.Idle;  // drones start idle, waiting for assignments
    }

    // Called whenever the drone transitions to a new state
    public void SetState(DroneState state)
    {
        State = state;
        OnStateChanged?.Invoke(this);  // notify DroneRegistry → SignalBus → UI
    }

    // Called by Dispatcher when this drone gets a job
    public void AssignJob(string jobId)
    {
        CurrentJobId = jobId;
        SetState(DroneState.MovingToPickup);  // immediately start heading to pickup
    }

    // Called when the drone finishes a job and returns to base
    public void ClearJob()
    {
        CurrentJobId = null;
        SetState(DroneState.Idle);  // back to idle, ready for next assignment
    }

    // Quick check used by Dispatcher to see if this drone can take a new job
    public bool IsAvailable => State == DroneState.Idle;
}
}
