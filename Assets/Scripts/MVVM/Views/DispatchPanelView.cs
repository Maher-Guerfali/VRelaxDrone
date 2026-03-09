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
public class DispatchPanelView : MonoBehaviour
{
    [SerializeField] Button assignButton;
    [SerializeField] TMP_Text feedbackLabel;
    [SerializeField] float feedbackDisplayTime = 3f;

    DispatchViewModel _vm;
    JobPanelViewModel _jobVM;
    DronePanelViewModel _droneVM;
    IJobService _jobService;

    [Inject]
    public void Construct(DispatchViewModel vm, JobPanelViewModel jobVM, DronePanelViewModel droneVM, IJobService jobService)
    {
        _vm = vm;
        _jobVM = jobVM;
        _droneVM = droneVM;
        _jobService = jobService;
    }

    void OnEnable()
    {
        assignButton.onClick.AddListener(OnAssignClicked);
        _vm.OnDispatchFeedback += ShowFeedback;
    }

    void OnDisable()
    {
        assignButton.onClick.RemoveListener(OnAssignClicked);
        _vm.OnDispatchFeedback -= ShowFeedback;
    }

    void OnAssignClicked()
    {
        _vm.Assign();

        // After successful assignment, find the DroneController and kick off the job
        if (_jobVM.SelectedJobId == null || _droneVM.SelectedDroneId == null) return;

        var job = _jobService.GetJob(_jobVM.SelectedJobId);
        if (job == null || job.Status == Core.JobStatus.Pending) return;

        // find the matching DroneController in the scene
        var controllers = FindObjectsByType<DroneController>(FindObjectsSortMode.None);
        foreach (var ctrl in controllers)
        {
            if (ctrl.DroneId == _droneVM.SelectedDroneId)
            {
                ctrl.StartJob(job);
                break;
            }
        }
    }

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
