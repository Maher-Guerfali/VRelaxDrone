using System;
using DroneDispatcher.Services;

namespace DroneDispatcher.MVVM.ViewModels
{
public class DispatchViewModel
{
    readonly IDispatcher _dispatcher;
    readonly JobPanelViewModel _jobVM;
    readonly DronePanelViewModel _droneVM;

    public event Action<string> OnDispatchFeedback;

    public DispatchViewModel(IDispatcher dispatcher, JobPanelViewModel jobVM, DronePanelViewModel droneVM)
    {
        _dispatcher = dispatcher;
        _jobVM = jobVM;
        _droneVM = droneVM;
    }

    public bool CanAssign => _jobVM.SelectedJobId != null && _droneVM.SelectedDroneId != null;

    public void Assign()
    {
        if (!CanAssign)
        {
            OnDispatchFeedback?.Invoke("Select a job and a drone first.");
            return;
        }

        var result = _dispatcher.Assign(_jobVM.SelectedJobId, _droneVM.SelectedDroneId);
        OnDispatchFeedback?.Invoke(result.Message);
    }
}
}
