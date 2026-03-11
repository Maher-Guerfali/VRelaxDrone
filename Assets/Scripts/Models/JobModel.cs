using System;
using DroneDispatcher.Core;
using DroneDispatcher.Data;

namespace DroneDispatcher.Models
{
// Runtime representation of a delivery job.
// This is NOT a MonoBehaviour — it's a plain C# class that holds mutable state.
// We create one of these for each JobDefinition (ScriptableObject) at startup.
// The key idea: ScriptableObject = static data on disk, JobModel = live state at runtime.
public class JobModel
{
    // These are read-only because they come from the ScriptableObject and never change
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string PickupLocationName { get; }
    public string DropoffLocationName { get; }

    // These change during gameplay
    public JobStatus Status { get; private set; }
    public string AssignedDroneId { get; private set; }

    // Observer pattern: anyone can subscribe to get notified when status changes.
    // JobService subscribes here and forwards it to the Zenject SignalBus.
    public event Action<JobModel> OnStatusChanged;

    // Constructor takes the SO definition and copies its data into this runtime model
    public JobModel(JobDefinition def)
    {
        Id = def.jobId;
        Name = def.jobName;
        Description = def.description;
        PickupLocationName = def.pickupLocationName;
        DropoffLocationName = def.dropoffLocationName;
        Status = JobStatus.Pending;  // every job starts as Pending
    }

    // Update the status and notify subscribers (which eventually updates the UI)
    public void SetStatus(JobStatus status)
    {
        if (Status == status)
            return;  // avoid unnecessary event firing

        Status = status;
        OnStatusChanged?.Invoke(this);
    }

    // Called by Dispatcher when a drone is assigned to this job
    public void AssignToDrone(string droneId)
    {
        AssignedDroneId = droneId;
        SetStatus(JobStatus.Assigned);  // this triggers the signal chain
    }
}
}
