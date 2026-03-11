using DroneDispatcher.Services;
using UnityEngine;
using Zenject;

namespace DroneDispatcher
{
// Runs once at scene load to collect all waypoint locations.
// Lives on the SceneContext GameObject (or any GO in the scene).
// Zenject calls [Inject] after Awake but before Start, so by the time Start() runs
// we have our WaypointRegistry ready. We collect waypoints here (before DroneControllers
// try to use them in their Start methods).
public class GameStartup : MonoBehaviour
{
    IWaypointRegistry _waypoints;

    [Inject]
    public void Construct(IWaypointRegistry waypoints)
    {
        _waypoints = waypoints;
    }

    void Start()
    {
        // Scan the scene for all Waypoint components and build the name→transform lookup
        _waypoints.CollectAll();
    }
}
}
