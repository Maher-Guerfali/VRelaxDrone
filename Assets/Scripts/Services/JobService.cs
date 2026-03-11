using System.Collections.Generic;
using DroneDispatcher.Core;
using DroneDispatcher.Data;
using DroneDispatcher.Models;
using UnityEngine;
using Zenject;

namespace DroneDispatcher.Services
{
// Manages all delivery jobs at runtime.
// On Initialize(), it loads JobDefinition ScriptableObjects from Resources/Jobs/
// and wraps each one in a JobModel (runtime state).
// It also bridges the gap between local C# events and Zenject's SignalBus:
// when a JobModel fires OnStatusChanged, this service catches it and fires
// a JobStatusChangedSignal through the SignalBus so ViewModels can react.
public class JobService : IJobService
{
    readonly SignalBus _signalBus;
    readonly List<JobModel> _jobs = new List<JobModel>();

    // Exposed as read-only so the UI can display but not modify the list
    public IReadOnlyList<JobModel> Jobs => _jobs;

    // Constructor injection — Zenject provides the SignalBus
    public JobService(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    // Called by JobPanelViewModel.Initialize() at startup.
    // Loads all job assets from the Resources/Jobs folder.
    public void Initialize()
    {
        var definitions = Resources.LoadAll<JobDefinition>("Jobs");

        if (definitions == null || definitions.Length == 0)
        {
            Debug.LogWarning("[JobService] Loaded 0 jobs. Create JobDefinition assets in Assets/Resources/Jobs or use Tools/Drone Dispatcher/Create Default Jobs.");
            return;
        }

        // Create a runtime JobModel for each ScriptableObject definition
        foreach (var def in definitions)
        {
            var model = new JobModel(def);
            model.OnStatusChanged += HandleJobStatusChanged;  // subscribe to local event
            _jobs.Add(model);
        }

        Debug.Log($"[JobService] Loaded {_jobs.Count} jobs");
    }

    public JobModel GetJob(string jobId)
    {
        return _jobs.Find(j => j.Id == jobId);
    }

    // When any job's status changes, forward it to the global SignalBus.
    // This is how the UI knows to refresh — the ViewModel subscribes to this signal.
    void HandleJobStatusChanged(JobModel job)
    {
        _signalBus.Fire(new JobStatusChangedSignal { JobId = job.Id });
    }
}
}
