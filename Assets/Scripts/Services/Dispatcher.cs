using DroneDispatcher.Core;
using UnityEngine;

namespace DroneDispatcher.Services
{
public class Dispatcher : IDispatcher
{
    readonly IJobService _jobService;
    readonly IDroneRegistry _droneRegistry;

    public Dispatcher(IJobService jobService, IDroneRegistry droneRegistry)
    {
        _jobService = jobService;
        _droneRegistry = droneRegistry;
    }

    // Core business logic: validate assignment and update both models
    public DispatchResult Assign(string jobId, string droneId)
    {
        var job = _jobService.GetJob(jobId);
        if (job == null)
            return DispatchResult.Fail("Job not found.");

        var drone = _droneRegistry.GetDrone(droneId);
        if (drone == null)
            return DispatchResult.Fail("Drone not found.");

        if (job.Status != JobStatus.Pending)
            return DispatchResult.Fail($"Job \"{job.Name}\" is already {job.Status}.");

        if (!drone.IsAvailable)
            return DispatchResult.Fail($"Drone \"{drone.DisplayName}\" is busy.");

        // All checks passed — execute assignment
        job.AssignToDrone(drone.Id);
        drone.AssignJob(job.Id);

        Debug.Log($"[Dispatcher] Assigned \"{job.Name}\" to {drone.DisplayName}");
        return DispatchResult.Ok($"Assigned \"{job.Name}\" to {drone.DisplayName}.");
    }
}
}
