namespace DroneDispatcher.Core
{
// These are Zenject "signals" — basically lightweight event classes.
// When something changes (a job status or drone state), we fire one of these
// through the SignalBus, and any ViewModel that subscribed to it will react.
// This keeps our services and UI completely decoupled — they never reference each other directly.

// Fired by JobService whenever a job's status changes (Pending → Assigned → InProgress → Completed)
public class JobStatusChangedSignal
{
    public string JobId;  // which job changed, so listeners can look it up
}

// Fired by DroneRegistry whenever a drone's state changes (Idle → MovingToPickup → etc.)
public class DroneStateChangedSignal
{
    public string DroneId;  // which drone changed
}
}
