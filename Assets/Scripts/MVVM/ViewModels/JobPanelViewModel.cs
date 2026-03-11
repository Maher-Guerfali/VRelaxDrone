using System;
using System.Collections.Generic;
using DroneDispatcher.Core;
using DroneDispatcher.Models;
using DroneDispatcher.Services;
using Zenject;

namespace DroneDispatcher.MVVM.ViewModels
{
// ViewModel for the job list panel.
// Implements IInitializable so Zenject automatically calls Initialize() after all bindings are resolved.
// Implements IDisposable so Zenject calls Dispose() when the container is destroyed (cleans up subscriptions).
// The View (JobPanelView) subscribes to OnJobsUpdated and rebuilds the UI when jobs change.
public class JobPanelViewModel : IInitializable, IDisposable
{
    readonly IJobService _jobService;
    readonly SignalBus _signalBus;

    // The View subscribes to this event to know when to rebuild the job list
    public event Action OnJobsUpdated;

    // Read-only access to all jobs for the View to display
    public IReadOnlyList<JobModel> Jobs => _jobService.Jobs;

    // Which job the user has clicked/selected in the UI
    public string SelectedJobId { get; private set; }

    public JobPanelViewModel(IJobService jobService, SignalBus signalBus)
    {
        _jobService = jobService;
        _signalBus = signalBus;
    }

    // Zenject calls this automatically because we used BindInterfacesAndSelfTo in GameInstaller.
    // This is where we load jobs and start listening for status changes.
    public void Initialize()
    {
        _jobService.Initialize();  // triggers loading ScriptableObjects from Resources/Jobs
        _signalBus.Subscribe<JobStatusChangedSignal>(OnJobStatusChanged);
        OnJobsUpdated?.Invoke();  // tell the View to do its first render
    }

    // Clean up signal subscription when the scene is unloaded
    public void Dispose()
    {
        _signalBus.TryUnsubscribe<JobStatusChangedSignal>(OnJobStatusChanged);
    }

    // Called by JobPanelView when the user clicks on a job row
    public void SelectJob(string jobId)
    {
        SelectedJobId = jobId;
    }

    // When any job status changes (via SignalBus), tell the View to rebuild
    void OnJobStatusChanged(JobStatusChangedSignal signal)
    {
        OnJobsUpdated?.Invoke();
    }
}
}
