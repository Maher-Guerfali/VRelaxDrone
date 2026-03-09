using System.Collections.Generic;
using UnityEngine;

namespace DroneDispatcher.Services
{
public interface IWaypointRegistry
{
    void CollectAll();
    Transform GetLocation(string locationId);
}

public class WaypointRegistry : IWaypointRegistry
{
    readonly Dictionary<string, Transform> _map = new Dictionary<string, Transform>();

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

    public Transform GetLocation(string locationId)
    {
        _map.TryGetValue(locationId, out var t);
        return t;
    }
}
}
