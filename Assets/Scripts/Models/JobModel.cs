using System;
using DroneDispatcher.Core;
using DroneDispatcher.Data;

namespace DroneDispatcher.Models
{
// Runtime representation of a job. Wraps the SO definition and adds mutable state.
public class JobModel
{
    public string Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string PickupLocationName { get; }
    public string DropoffLocationName { get; }

    public JobStatus Status { get; private set; }
    public string AssignedDroneId { get; private set; }

    public event Action<JobModel> OnStatusChanged;

    public JobModel(JobDefinition def)
    {
        Id = def.jobId;
        Name = def.jobName;
        Description = def.description;
        PickupLocationName = def.pickupLocationName;
        DropoffLocationName = def.dropoffLocationName;
        Status = JobStatus.Pending;
    }

    public void SetStatus(JobStatus status)
    {
        if (Status == status)
            return;

        Status = status;
        OnStatusChanged?.Invoke(this);
    }

    public void AssignToDrone(string droneId)
    {
        AssignedDroneId = droneId;
        SetStatus(JobStatus.Assigned);
    }
}
}
