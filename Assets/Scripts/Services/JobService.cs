using System.Collections.Generic;
using DroneDispatcher.Core;
using DroneDispatcher.Data;
using DroneDispatcher.Models;
using UnityEngine;
using Zenject;

namespace DroneDispatcher.Services
{
public class JobService : IJobService
{
    readonly SignalBus _signalBus;
    readonly List<JobModel> _jobs = new List<JobModel>();

    public IReadOnlyList<JobModel> Jobs => _jobs;

    public JobService(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    public void Initialize()
    {
        // Load all job definitions from Resources/Jobs
        var definitions = Resources.LoadAll<JobDefinition>("Jobs");

        if (definitions == null || definitions.Length == 0)
        {
            Debug.LogWarning("[JobService] Loaded 0 jobs. Create JobDefinition assets in Assets/Resources/Jobs or use Tools/Drone Dispatcher/Create Default Jobs.");
            return;
        }

        foreach (var def in definitions)
        {
            var model = new JobModel(def);
            model.OnStatusChanged += HandleJobStatusChanged;
            _jobs.Add(model);
        }

        Debug.Log($"[JobService] Loaded {_jobs.Count} jobs");
    }

    public JobModel GetJob(string jobId)
    {
        return _jobs.Find(j => j.Id == jobId);
    }

    void HandleJobStatusChanged(JobModel job)
    {
        _signalBus.Fire(new JobStatusChangedSignal { JobId = job.Id });
    }
}
}
