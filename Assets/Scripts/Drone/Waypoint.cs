using UnityEngine;

namespace DroneDispatcher.Drone
{
// Put this on each pickup/dropoff checkpoint in the scene.
// The locationId must match the pickupLocationName / dropoffLocationName in the JobDefinition.
public class Waypoint : MonoBehaviour
{
    [Tooltip("Must match the location name in the JobDefinition (e.g. Hospital, Clinic, Warehouse)")]
    public string locationId;

    [Header("Optional — place your 3D checkpoint model as a child")]
    [SerializeField] GameObject visualModel;

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
