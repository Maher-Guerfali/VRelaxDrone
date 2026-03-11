using System.Collections;
using DroneDispatcher.Drone;
using DroneDispatcher.MVVM.ViewModels;
using DroneDispatcher.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DroneDispatcher.MVVM.Views
{
// The "Assign" button panel. When clicked, it:
// 1. Calls DispatchViewModel.Assign() to validate and link job + drone
// 2. Finds the actual DroneController in the scene and calls StartJob() to begin movement
// 3. Displays success/error feedback from the ViewModel
// This is a View in MVVM — it has zero business logic, just wires UI to ViewModel.
public class DispatchPanelView : MonoBehaviour
{
    [SerializeField] Button assignButton;         // the "Assign" button in the UI
    [SerializeField] TMP_Text feedbackLabel;      // shows success/error messages
    [SerializeField] float feedbackDisplayTime = 3f;

    DispatchViewModel _vm;
    JobPanelViewModel _jobVM;
    DronePanelViewModel _droneVM;
    IJobService _jobService;

    // Zenject injects these — [Inject] on a method works like constructor injection for MonoBehaviours
    [Inject]
    public void Construct(DispatchViewModel vm, JobPanelViewModel jobVM, DronePanelViewModel droneVM, IJobService jobService)
    {
        _vm = vm;
        _jobVM = jobVM;
        _droneVM = droneVM;
        _jobService = jobService;
    }

    // Subscribe to button clicks and ViewModel feedback when the panel is active
    void OnEnable()
    {
        assignButton.onClick.AddListener(OnAssignClicked);
        _vm.OnDispatchFeedback += ShowFeedback;
    }

    // Always unsubscribe to prevent memory leaks
    void OnDisable()
    {
        assignButton.onClick.RemoveListener(OnAssignClicked);
        _vm.OnDispatchFeedback -= ShowFeedback;
    }

    void OnAssignClicked()
    {
        // Step 1: let the ViewModel/Dispatcher handle validation and state updates
        _vm.Assign();

        // Step 2: if assignment succeeded, we need to tell the actual drone to start moving.
        // The ViewModel only updates data models; the physical drone needs a direct call.
        if (_jobVM.SelectedJobId == null || _droneVM.SelectedDroneId == null) return;

        var job = _jobService.GetJob(_jobVM.SelectedJobId);
        if (job == null || job.Status == Core.JobStatus.Pending) return;  // still pending = assignment failed

        // Find the DroneController MonoBehaviour in the scene that matches our selected drone
        var controllers = FindObjectsByType<DroneController>(FindObjectsSortMode.None);
        foreach (var ctrl in controllers)
        {
            if (ctrl.DroneId == _droneVM.SelectedDroneId)
            {
                ctrl.StartJob(job);  // this starts the NavMesh navigation
                break;
            }
        }
    }

    // Show feedback message, then auto-clear after a few seconds
    void ShowFeedback(string message)
    {
        if (feedbackLabel == null) return;
        feedbackLabel.text = message;
        StopAllCoroutines();
        StartCoroutine(ClearFeedback());
    }

    IEnumerator ClearFeedback()
    {
        yield return new WaitForSeconds(feedbackDisplayTime);
        feedbackLabel.text = "";
    }
}
}
