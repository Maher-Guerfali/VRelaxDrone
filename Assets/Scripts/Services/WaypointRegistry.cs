using System.Collections.Generic;
using UnityEngine;

namespace DroneDispatcher.Services
{
// Interface so we can inject this anywhere without coupling to the concrete class
public interface IWaypointRegistry
{
    void CollectAll();
    Transform GetLocation(string locationId);
}

// Scans the scene for all Waypoint components and maps their locationId to their Transform.
// This way, when a drone needs to fly to "Hospital", it asks WaypointRegistry.GetLocation("Hospital")
// and gets the actual world position. Called once at startup by GameStartup.Start().
public class WaypointRegistry : IWaypointRegistry
{
    readonly Dictionary<string, Transform> _map = new Dictionary<string, Transform>();

    // Find every Waypoint component in the scene and build the lookup dictionary
    public void CollectAll()
    {
        _map.Clear();
        var all = Object.FindObjectsByType<Drone.Waypoint>(FindObjectsSortMode.None);
        foreach (var wp in all)
        {
            if (string.IsNullOrEmpty(wp.locationId)) continue;
            if (_map.ContainsKey(wp.locationId))
            {
                Debug.LogWarning($"[WaypointRegistry] Duplicate locationId: {wp.locationId}");
                continue;
            }
            _map[wp.locationId] = wp.transform;
        }
        Debug.Log($"[WaypointRegistry] Collected {_map.Count} waypoints");
    }

    // Look up where a location is in the world. Returns null if not found.
    public Transform GetLocation(string locationId)
    {
        _map.TryGetValue(locationId, out var t);
        return t;
    }
}
}
