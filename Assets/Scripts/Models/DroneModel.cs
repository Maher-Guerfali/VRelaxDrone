using System;
using DroneDispatcher.Core;
using UnityEngine;

namespace DroneDispatcher.Models
{
// Runtime state for a single drone. Not a MonoBehaviour — pure data.
public class DroneModel
{
    public string Id { get; }
    public string DisplayName { get; }
    public DroneState State { get; private set; }
    public string CurrentJobId { get; private set; }
    public Vector3 BasePosition { get; }

    public event Action<DroneModel> OnStateChanged;

    public DroneModel(string id, string displayName, Vector3 basePosition)
    {
        Id = id;
        DisplayName = displayName;
        BasePosition = basePosition;
        State = DroneState.Idle;
    }

    public void SetState(DroneState state)
    {
        State = state;
        OnStateChanged?.Invoke(this);
    }

    public void AssignJob(string jobId)
    {
        CurrentJobId = jobId;
        SetState(DroneState.MovingToPickup);
    }

    public void ClearJob()
    {
        CurrentJobId = null;
        SetState(DroneState.Idle);
    }

    public bool IsAvailable => State == DroneState.Idle;
}
}
