using UnityEngine;

namespace DroneDispatcher.Data
{
[CreateAssetMenu(fileName = "NewJob", menuName = "Drone Dispatcher/Job Definition")]
public class JobDefinition : ScriptableObject
{
    public string jobId;
    public string jobName;
    public string description;

    [Header("Locations — match GameObject names in scene")]
    public string pickupLocationName;
    public string dropoffLocationName;
}
}
