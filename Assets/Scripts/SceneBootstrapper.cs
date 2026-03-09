using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
using Unity.AI.Navigation;
#endif

namespace DroneDispatcher
{
// Editor helper — generates placeholder ground, obstacles, and waypoints.
// Drones are NOT created here because you'll use your own 3D drone model.
// After generation: bake NavMesh, then place your drone prefabs manually.
public class SceneBootstrapper : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Generate Scene Layout")]
    void GenerateScene()
    {
        // Ground
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(5, 1, 5);
        var surface = ground.AddComponent<NavMeshSurface>();
        ground.GetComponent<Renderer>().sharedMaterial = CreateMat("Ground", new Color(0.35f, 0.55f, 0.35f));

        // Obstacles
        CreateObstacle("Obstacle_A", new Vector3(5, 1, 3), new Vector3(2, 2, 2));
        CreateObstacle("Obstacle_B", new Vector3(-6, 1, -4), new Vector3(3, 2, 1));
        CreateObstacle("Obstacle_C", new Vector3(0, 1, 8), new Vector3(1, 2, 4));
        CreateObstacle("Obstacle_D", new Vector3(8, 1, -7), new Vector3(2, 2, 2));
        CreateObstacle("Obstacle_E", new Vector3(-3, 1, 5), new Vector3(1.5f, 2, 1.5f));

        // Waypoints with Waypoint component — place your 3D checkpoint model as child
        CreateWaypoint("Hospital",    new Vector3(-15, 0, 12),  Color.red);
        CreateWaypoint("Clinic",      new Vector3(15, 0, 12),   Color.red);
        CreateWaypoint("Warehouse",   new Vector3(-15, 0, 0),   Color.yellow);
        CreateWaypoint("Residential", new Vector3(15, 0, 0),    Color.yellow);
        CreateWaypoint("Restaurant",  new Vector3(-15, 0, -12), Color.magenta);
        CreateWaypoint("Office",      new Vector3(15, 0, -12),  Color.magenta);

        // Drone spawn markers — just empty objects at suggested positions.
        // You replace these with your 3D drone prefab.
        CreateDroneMarker("Drone_01", "Alpha",   new Vector3(-3, 0, -10));
        CreateDroneMarker("Drone_02", "Bravo",   new Vector3(0, 0, -10));
        CreateDroneMarker("Drone_03", "Charlie",  new Vector3(3, 0, -10));

        surface.BuildNavMesh();
        Debug.Log("[SceneBootstrapper] Layout generated. Now place your 3D drone models on the Drone markers and set up UI.");
    }

    void CreateObstacle(string n, Vector3 pos, Vector3 scale)
    {
        var ob = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ob.name = n;
        ob.transform.position = pos;
        ob.transform.localScale = scale;
        ob.AddComponent<NavMeshObstacle>().carving = true;
        ob.GetComponent<Renderer>().sharedMaterial = CreateMat(n, new Color(0.4f, 0.4f, 0.4f));
    }

    void CreateWaypoint(string n, Vector3 pos, Color c)
    {
        var wp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wp.name = n;
        wp.transform.position = pos;
        wp.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
        DestroyImmediate(wp.GetComponent<Collider>());
        wp.GetComponent<Renderer>().sharedMaterial = CreateMat(n, c);

        // the Waypoint script lets WaypointRegistry find this at runtime
        var waypoint = wp.AddComponent<Drone.Waypoint>();
        waypoint.locationId = n;
    }

    void CreateDroneMarker(string id, string displayName, Vector3 pos)
    {
        // Empty GO with NavMeshAgent + DroneController pre-configured.
        // Drag your 3D drone model as a child, assign the Animator field.
        var go = new GameObject(id);
        go.transform.position = pos;

        var agent = go.AddComponent<NavMeshAgent>();
        agent.speed = 8f;
        agent.angularSpeed = 360f;
        agent.acceleration = 12f;
        agent.stoppingDistance = 0.3f;
        agent.radius = 0.5f;

        var ctrl = go.AddComponent<Drone.DroneController>();
        var so = new SerializedObject(ctrl);
        so.FindProperty("droneId").stringValue = id;
        so.FindProperty("displayName").stringValue = displayName;
        so.ApplyModifiedProperties();

        go.AddComponent<Drone.DroneVisuals>();
    }

    Material CreateMat(string n, Color c)
    {
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.name = "Mat_" + n;
        mat.color = c;
        string path = "Assets/Art/Materials/Mat_" + n + ".mat";
        if (!AssetDatabase.LoadAssetAtPath<Material>(path))
            AssetDatabase.CreateAsset(mat, path);
        return AssetDatabase.LoadAssetAtPath<Material>(path);
    }
#endif
}
}
