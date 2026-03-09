using DroneDispatcher.Core;
using DroneDispatcher.MVVM.ViewModels;
using DroneDispatcher.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DroneDispatcher.MVVM.Views
{
// Shows a progress slider for the currently active mission.
// Attach to a panel that has a Slider + label.
public class MissionProgressView : MonoBehaviour
{
    [SerializeField] Slider progressSlider;
    [SerializeField] TMP_Text missionLabel;
    [SerializeField] TMP_Text statusLabel;

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

        // map drone state to progress 0..1
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
