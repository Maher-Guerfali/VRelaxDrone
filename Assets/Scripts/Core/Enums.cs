namespace DroneDispatcher.Core
{
public enum JobStatus
{
    Pending,
    Assigned,
    InProgress,
    Completed
}

public enum DroneState
{
    Idle,
    MovingToPickup,
    PickingUp,
    MovingToDropoff,
    DroppingOff,
    Returning
}
}
