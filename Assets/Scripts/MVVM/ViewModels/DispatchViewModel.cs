using System;
using DroneDispatcher.Services;

namespace DroneDispatcher.MVVM.ViewModels
{
// Coordinates the "Assign" action between the job panel and drone panel.
// When the user clicks the Assign button, this ViewModel checks if both
// a job and drone are selected, then calls the Dispatcher service to do the actual assignment.
// It fires OnDispatchFeedback with a success/error message for the View to display.
// Note: this is a plain C# class, NOT a MonoBehaviour. Zenject creates it for us.
public class DispatchViewModel
{
    readonly IDispatcher _dispatcher;
    readonly JobPanelViewModel _jobVM;    // need this to know which job is selected
    readonly DronePanelViewModel _droneVM; // need this to know which drone is selected

    // The View subscribes to this to show feedback messages to the user
    public event Action<string> OnDispatchFeedback;

    // Constructor injection — Zenject provides all three dependencies
    public DispatchViewModel(IDispatcher dispatcher, JobPanelViewModel jobVM, DronePanelViewModel droneVM)
    {
        _dispatcher = dispatcher;
        _jobVM = jobVM;
        _droneVM = droneVM;
    }

    // Quick check: both a job and drone must be selected before we can assign
    public bool CanAssign => _jobVM.SelectedJobId != null && _droneVM.SelectedDroneId != null;

    // Called when the user clicks the Assign button
    public void Assign()
    {
        if (!CanAssign)
        {
            OnDispatchFeedback?.Invoke("Select a job and a drone first.");
            return;
        }

        // Delegate to the Dispatcher service which does all the validation
        var result = _dispatcher.Assign(_jobVM.SelectedJobId, _droneVM.SelectedDroneId);
        OnDispatchFeedback?.Invoke(result.Message);  // show result in the UI
    }
}
}
