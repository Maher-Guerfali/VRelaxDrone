using DroneDispatcher.Services;
using UnityEngine;
using Zenject;

namespace DroneDispatcher
{
// Put this on the SceneContext GameObject (or any GO in the scene).
// Zenject calls [Inject] after Awake but before Start, so we
// collect waypoints in Start — before DroneControllers get jobs.
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
        _waypoints.CollectAll();
    }
}
}
