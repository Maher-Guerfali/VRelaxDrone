using DroneDispatcher.Core;
using UnityEngine;

namespace DroneDispatcher.Services
{
// The Dispatcher is our core business logic — it decides if a job can be assigned to a drone.
// It validates constraints (is the job still pending? is the drone free?) and then
// updates both models. Think of it like a real dispatch operator at a logistics company.
// Injected via Zenject: it gets IJobService and IDroneRegistry through constructor injection.
public class Dispatcher : IDispatcher
{
    readonly IJobService _jobService;
    readonly IDroneRegistry _droneRegistry;

    // Zenject calls this constructor automatically because we bound IDispatcher → Dispatcher in GameInstaller
    public Dispatcher(IJobService jobService, IDroneRegistry droneRegistry)
    {
        _jobService = jobService;
        _droneRegistry = droneRegistry;
    }

    // Main method — tries to assign a specific job to a specific drone.
    // Returns a result with success/failure message so the UI can show feedback.
    public DispatchResult Assign(string jobId, string droneId)
    {
        // Look up both the job and drone by their IDs
        var job = _jobService.GetJob(jobId);
        if (job == null)
            return DispatchResult.Fail("Job not found.");

        var drone = _droneRegistry.GetDrone(droneId);
        if (drone == null)
            return DispatchResult.Fail("Drone not found.");

        // Validate: the job must be Pending (not already taken)
        if (job.Status != JobStatus.Pending)
            return DispatchResult.Fail($"Job \"{job.Name}\" is already {job.Status}.");

        // Validate: the drone must be Idle (not already on a mission)
        if (!drone.IsAvailable)
            return DispatchResult.Fail($"Drone \"{drone.DisplayName}\" is busy.");

        // All checks passed — link the job and drone together.
        // This triggers the signal chain: Model → Service → SignalBus → ViewModel → View
        job.AssignToDrone(drone.Id);
        drone.AssignJob(job.Id);

        Debug.Log($"[Dispatcher] Assigned \"{job.Name}\" to {drone.DisplayName}");
        return DispatchResult.Ok($"Assigned \"{job.Name}\" to {drone.DisplayName}.");
    }
}
}
