namespace DroneDispatcher.Core
{
// Zenject signals — fired when job or drone state changes.
// ViewModels subscribe to these to keep UI in sync.
public class JobStatusChangedSignal
{
    public string JobId;
}

public class DroneStateChangedSignal
{
    public string DroneId;
}
}
