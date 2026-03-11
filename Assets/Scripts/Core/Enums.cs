namespace DroneDispatcher.Core
{
// Tracks where a delivery job is in its lifecycle.
// Pending = nobody picked it up yet, Completed = package delivered.
public enum JobStatus
{
    Pending,       // waiting to be assigned to a drone
    Assigned,      // a drone accepted it but hasn't started moving yet
    InProgress,    // drone picked up the package and is heading to dropoff
    Completed      // delivery done
}

// The drone's state machine — each state maps to what the drone is physically doing.
// We check this every frame in DroneController.Update() to decide what happens next.
public enum DroneState
{
    Idle,              // parked at base, ready for a new job
    MovingToPickup,    // flying towards the pickup waypoint
    PickingUp,         // arrived at pickup, playing the "grab" animation
    MovingToDropoff,   // carrying package to the delivery location
    DroppingOff,       // at delivery site, playing drop animation
    Returning          // job done, heading back to base position
}
}
