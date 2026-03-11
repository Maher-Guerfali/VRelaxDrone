using DroneDispatcher.Core;
using DroneDispatcher.MVVM.ViewModels;
using DroneDispatcher.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DroneDispatcher.MVVM.Views
{
// Displays a progress bar for the currently active delivery mission.
// Shows the job name, drone name + state, and a slider from 0–1.
// Subscribes directly to Zenject signals (instead of going through a ViewModel)
// because it needs to react to both drone and job changes.
public class MissionProgressView : MonoBehaviour
{
    [SerializeField] Slider progressSlider;  // 0–1 progress bar
    [SerializeField] TMP_Text missionLabel;  // shows job name
    [SerializeField] TMP_Text statusLabel;   // shows drone name + current state

    JobPanelViewModel _jobVM;
    DronePanelViewModel _droneVM;
    SignalBus _signalBus;

    [Inject]
    public void Construct(JobPanelViewModel jobVM, DronePanelViewModel droneVM, SignalBus signalBus)
    {
        _jobVM = jobVM;
        _droneVM = droneVM;
        _signalBus = signalBus;
    }

    void OnEnable()
    {
        _signalBus.Subscribe<DroneStateChangedSignal>(OnDroneChanged);
        _signalBus.Subscribe<JobStatusChangedSignal>(OnJobChanged);

        ConfigureSlider();
        Refresh();
    }

    void OnDisable()
    {
        _signalBus.TryUnsubscribe<DroneStateChangedSignal>(OnDroneChanged);
        _signalBus.TryUnsubscribe<JobStatusChangedSignal>(OnJobChanged);
    }

    void OnDroneChanged(DroneStateChangedSignal s) => Refresh();
    void OnJobChanged(JobStatusChangedSignal s) => Refresh();

    void ConfigureSlider()
    {
        if (progressSlider == null) return;

        progressSlider.minValue = 0f;
        progressSlider.maxValue = 1f;
        progressSlider.wholeNumbers = false;
    }

    void Refresh()
    {
        if (progressSlider == null || missionLabel == null || statusLabel == null)
            return;

        Models.DroneModel activeDrone = FindActiveDrone();

        if (activeDrone == null || string.IsNullOrEmpty(activeDrone.CurrentJobId))
        {
            missionLabel.text = "No active mission";
            statusLabel.text = "";
            progressSlider.value = 0f;
            return;
        }

        // find matching job
        Models.JobModel job = null;
        foreach (var j in _jobVM.Jobs)
        {
            if (j.Id == activeDrone.CurrentJobId) { job = j; break; }
        }

        if (job == null) return;

        missionLabel.text = job.Name;
        statusLabel.text = $"{activeDrone.DisplayName} — {activeDrone.State}";

        // Map each drone state to a rough progress percentage
        float progress = activeDrone.State switch
        {
            DroneState.Idle => 0f,
            DroneState.MovingToPickup => 0.15f,
            DroneState.PickingUp => 0.35f,
            DroneState.MovingToDropoff => 0.55f,
            DroneState.DroppingOff => 0.8f,
            DroneState.Returning => 0.95f,
            _ => 0f,
        };

        if (job.Status == JobStatus.Completed)
            progress = 1f;

        progressSlider.value = progress;
    }

    Models.DroneModel FindActiveDrone()
    {
        // Prefer selected drone if it is currently on a mission.
        if (!string.IsNullOrEmpty(_droneVM.SelectedDroneId))
        {
            foreach (var d in _droneVM.Drones)
            {
                if (d.Id == _droneVM.SelectedDroneId && !d.IsAvailable)
                    return d;
            }
        }

        foreach (var d in _droneVM.Drones)
        {
            if (!d.IsAvailable)
                return d;
        }

        return null;
    }
}
}
