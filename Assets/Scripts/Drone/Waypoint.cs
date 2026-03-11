using UnityEngine;

namespace DroneDispatcher.Drone
{
// Place this component on any pickup/dropoff location in the scene.
// The locationId MUST match what's in the JobDefinition ScriptableObject.
// For example, if a job says pickupLocationName = "Hospital",
// then there must be a GameObject with this Waypoint component where locationId = "Hospital".
// WaypointRegistry scans for all of these at startup and builds a name→position lookup.
public class Waypoint : MonoBehaviour
{
    [Tooltip("Must match the location name in the JobDefinition (e.g. Hospital, Clinic, Warehouse)")]
    public string locationId;

    [Header("Optional — place your 3D checkpoint model as a child")]
    [SerializeField] GameObject visualModel;

    // Editor-only: draw a visual marker so we can see waypoints in the Scene view
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 1f);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, locationId);
#endif
    }
}
}
