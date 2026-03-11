using System.Collections;
using DroneDispatcher.Core;
using DroneDispatcher.Models;
using DroneDispatcher.Services;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace DroneDispatcher.Drone
{
// The main brain of each drone in the scene.
// This MonoBehaviour lives on the drone GameObject alongside NavMeshAgent.
// It creates a DroneModel (pure data) in Start(), registers with DroneRegistry,
// and then manages the full delivery state machine:
// Idle → MovingToPickup → PickingUp → MovingToDropoff → DroppingOff → Returning → Idle
//
// Zenject injects our services through the Construct method (field injection alternative).
// The NavMeshAgent handles all the actual pathfinding on the baked NavMesh.
[RequireComponent(typeof(NavMeshAgent))]
public class DroneController : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] string droneId;               // unique ID, e.g. "Drone_01"
    [SerializeField] string displayName = "Drone";  // friendly name for UI, e.g. "Alpha"

    [Header("Movement")]
    [SerializeField] float actionDuration = 1.5f;   // how long pickup/dropoff animations take
    [SerializeField] float arrivedThreshold = 0.5f;  // how close we need to be to count as "arrived"

    [Header("Animation — assign the Animator on your 3D drone model")]
    [SerializeField] Animator animator;
    [SerializeField] string flyingBoolParameter = "IsFlying";

    int _flyingBoolHash;
    bool _hasFlyingBool;
    bool _loggedMissingFlyingBool;

    NavMeshAgent _agent;
    DroneModel _model;        // the pure-data model that tracks our state
    JobModel _currentJob;     // the job we're currently working on

    // These get injected by Zenject — we never create them ourselves
    IJobService _jobService;
    IDroneRegistry _registry;
    IWaypointRegistry _waypoints;

    Transform _pickupTarget;   // where we pick up the package
    Transform _dropoffTarget;  // where we deliver it
    float _totalDistance;
    float _coveredDistance;

    // 0..1 progress for the UI progress bar
    public float MissionProgress { get; private set; }

    // Zenject calls this instead of a regular constructor (MonoBehaviours can't have constructors)
    [Inject]
    public void Construct(IJobService jobService, IDroneRegistry registry, IWaypointRegistry waypoints)
    {
        _jobService = jobService;
        _registry = registry;
        _waypoints = waypoints;
    }

    void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = true;
        _agent.updateUpAxis = false;

        // if animator wasn't manually assigned in the inspector, search children
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        CacheFlyingBoolParameter();
    }

    void Start()
    {
        // Create our data model and register with the global drone registry.
        // We save transform.position as the "base" so the drone knows where to return after a job.
        _model = new DroneModel(droneId, displayName, transform.position);
        _registry.Register(_model);
        _model.OnStateChanged += OnDroneStateChanged;

        SetFlying(false);  // start grounded
    }

    // Runs every frame — we only care about movement states
    void Update()
    {
        if (_model == null) return;

        switch (_model.State)
        {
            case DroneState.MovingToPickup:
            case DroneState.MovingToDropoff:
            case DroneState.Returning:
                UpdateProgress();  // update the 0..1 progress for UI
                CheckArrival();    // did we reach our destination?
                break;
        }
    }

    // Called by DispatchPanelView after the Dispatcher validates the assignment.
    // This is where the drone actually starts moving.
    public void StartJob(JobModel job)
    {
        _currentJob = job;

        // Look up the actual world positions from waypoint names
        _pickupTarget = _waypoints.GetLocation(job.PickupLocationName);
        _dropoffTarget = _waypoints.GetLocation(job.DropoffLocationName);

        if (_pickupTarget == null || _dropoffTarget == null)
        {
            Debug.LogError($"[DroneController] Missing waypoint for job {job.Name}. " +
                $"Make sure GameObjects with Waypoint component exist for '{job.PickupLocationName}' and '{job.DropoffLocationName}'.");
            return;
        }

        SetFlying(true);
        NavigateTo(_pickupTarget.position);  // start heading to pickup
    }

    // Checks if we've arrived at our current destination and decides what to do next.
    // This is the core of the state machine.
    void CheckArrival()
    {
        if (_agent.pathPending) return;                          // NavMesh still calculating path
        if (_agent.remainingDistance > arrivedThreshold) return;  // not close enough yet

        switch (_model.State)
        {
            // Arrived at pickup → play pickup animation, then head to dropoff
            case DroneState.MovingToPickup:
                StartCoroutine(PerformAction(DroneState.PickingUp, () =>
                {
                    _currentJob.SetStatus(JobStatus.InProgress);
                    _model.SetState(DroneState.MovingToDropoff);
                    NavigateTo(_dropoffTarget.position);
                }));
                break;

            // Arrived at dropoff → play drop animation, then head home
            case DroneState.MovingToDropoff:
                StartCoroutine(PerformAction(DroneState.DroppingOff, () =>
                {
                    _currentJob.SetStatus(JobStatus.Completed);
                    _model.SetState(DroneState.Returning);
                    NavigateTo(_model.BasePosition);
                }));
                break;

            // Arrived back at base → mission complete, go idle
            case DroneState.Returning:
                SetFlying(false);
                MissionProgress = 0f;
                _currentJob = null;
                _model.ClearJob();  // this sets state to Idle and clears the job reference
                break;
        }
    }

    // Plays a pickup/dropoff animation using DOTween punch scale effect.
    // Stops the NavMeshAgent during the action, then resumes movement.
    IEnumerator PerformAction(DroneState actionState, System.Action onComplete)
    {
        _model.SetState(actionState);
        _agent.isStopped = true;   // pause navigation
        SetFlying(false);          // land animation

        // Quick "bounce" effect to make it look like the drone is picking up / dropping off
        transform.DOPunchScale(Vector3.one * 0.12f, actionDuration * 0.5f, 2);

        yield return new WaitForSeconds(actionDuration);

        _agent.isStopped = false;  // resume navigation
        SetFlying(true);           // take off again
        onComplete?.Invoke();
    }

    // Calculates a 0..1 overall mission progress for the UI progress bar.
    // The mission is split into legs: pickup (0-35%), dropoff (35-75%), return (75-100%).
    void UpdateProgress()
    {
        if (_totalDistance < 0.1f) return;
        _coveredDistance = _totalDistance - _agent.remainingDistance;

        float leg = _model.State switch
        {
            DroneState.MovingToPickup => 0f,
            DroneState.MovingToDropoff => 0.35f,
            DroneState.Returning => 0.75f,
            _ => 0f
        };

        float legWeight = _model.State == DroneState.Returning ? 0.25f : 0.35f;
        MissionProgress = leg + Mathf.Clamp01(_coveredDistance / _totalDistance) * legWeight;
    }

    // Tell the NavMeshAgent to pathfind to a world position
    void NavigateTo(Vector3 worldPos)
    {
        _agent.SetDestination(worldPos);
        _totalDistance = Vector3.Distance(transform.position, worldPos);
        _coveredDistance = 0f;
    }

    // Toggle the "IsFlying" bool on the Animator (controls idle vs fly animation)
    void SetFlying(bool flying)
    {
        if (animator == null || !_hasFlyingBool)
        {
            if (animator != null && !_hasFlyingBool && !_loggedMissingFlyingBool)
            {
                _loggedMissingFlyingBool = true;
                Debug.LogWarning($"[DroneController] Animator on '{name}' has no bool parameter named '{flyingBoolParameter}' (or fallback names).",
                    this);
            }
            return;
        }

        animator.SetBool(_flyingBoolHash, flying);
    }

    // On Awake, find the Animator bool parameter hash for fast lookups.
    // Checks multiple common naming conventions in case the animator uses a different one.
    void CacheFlyingBoolParameter()
    {
        _hasFlyingBool = false;
        _flyingBoolHash = 0;

        if (animator == null)
            return;

        var candidateNames = new[] { flyingBoolParameter, "IsFlying", "isFlying", "isFLying" };
        for (int i = 0; i < candidateNames.Length; i++)
        {
            if (TryGetBoolParameterHash(candidateNames[i], out var hash))
            {
                _flyingBoolHash = hash;
                _hasFlyingBool = true;
                return;
            }
        }
    }

    bool TryGetBoolParameterHash(string parameterName, out int hash)
    {
        hash = 0;
        if (string.IsNullOrWhiteSpace(parameterName) || animator == null)
            return false;

        var parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            if (parameter.type == AnimatorControllerParameterType.Bool && parameter.name == parameterName)
            {
                hash = parameter.nameHash;
                return true;
            }
        }

        return false;
    }

    void OnDroneStateChanged(DroneModel drone)
    {
        // State change is propagated through DroneRegistry → SignalBus → UI automatically
    }

    public DroneModel Model => _model;
    public string DroneId => droneId;
}
}
}
