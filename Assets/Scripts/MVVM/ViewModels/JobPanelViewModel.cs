using System;
using System.Collections.Generic;
using DroneDispatcher.Core;
using DroneDispatcher.Models;
using DroneDispatcher.Services;
using Zenject;

namespace DroneDispatcher.MVVM.ViewModels
{
public class JobPanelViewModel : IInitializable, IDisposable
{
    readonly IJobService _jobService;
    readonly SignalBus _signalBus;

    // View subscribes to this
    public event Action OnJobsUpdated;

    public IReadOnlyList<JobModel> Jobs => _jobService.Jobs;
    public string SelectedJobId { get; private set; }

    public JobPanelViewModel(IJobService jobService, SignalBus signalBus)
    {
        _jobService = jobService;
        _signalBus = signalBus;
    }

    public void Initialize()
    {
        _jobService.Initialize();
        _signalBus.Subscribe<JobStatusChangedSignal>(OnJobStatusChanged);
        OnJobsUpdated?.Invoke();
    }

    public void Dispose()
    {
        _signalBus.TryUnsubscribe<JobStatusChangedSignal>(OnJobStatusChanged);
    }

    public void SelectJob(string jobId)
    {
        SelectedJobId = jobId;
    }

    void OnJobStatusChanged(JobStatusChangedSignal signal)
    {
        OnJobsUpdated?.Invoke();
    }
}
}
