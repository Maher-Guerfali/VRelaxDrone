using UnityEngine;

namespace DroneDispatcher.Data
{
// ScriptableObject = a data container that lives as an asset in the project (not on a GameObject).
// We create these in the Editor via right-click > Create > Drone Dispatcher > Job Definition,
// or automatically with the JobAssetCreator tool.
// Each one defines a delivery job: where to pick up, where to drop off.
// At runtime, JobService loads all of these from Resources/Jobs/ and wraps them in JobModel.
[CreateAssetMenu(fileName = "NewJob", menuName = "Drone Dispatcher/Job Definition")]
public class JobDefinition : ScriptableObject
{
    public string jobId;           // unique identifier, e.g. "job_01"
    public string jobName;         // display name shown in the UI, e.g. "Medical Supply"
    public string description;     // short description for the job panel

    [Header("Locations — match GameObject names in scene")]
    public string pickupLocationName;   // must match a Waypoint's locationId in the scene
    public string dropoffLocationName;  // same — the drone flies from pickup to dropoff
}
}
