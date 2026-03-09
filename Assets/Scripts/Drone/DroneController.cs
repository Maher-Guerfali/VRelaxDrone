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
[RequireComponent(typeof(NavMeshAgent))]
public class DroneController : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] string droneId;
    [SerializeField] string displayName = "Drone";

    [Header("Movement")]
    [SerializeField] float actionDuration = 1.5f;
    [SerializeField] float arrivedThreshold = 0.5f;

    [Header("Animation — assign the Animator on your 3D drone model")]
    [SerializeField] Animator animator;
    [SerializeField] string flyingBoolParameter = "IsFlying";

    int _flyingBoolHash;
    bool _hasFlyingBool;
    bool _loggedMissingFlyingBool;

    NavMeshAgent _agent;
    DroneModel _model;
    JobModel _currentJob;

    IJobService _jobService;
    IDroneRegistry _registry;
    IWaypointRegistry _waypoints;

    Transform _pickupTarget;
    Transform _dropoffTarget;
    float _totalDistance;
    float _coveredDistance;

    // 0..1 how far along the current leg the drone is
    public float MissionProgress { get; private set; }

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

        // if animator wasn't assigned, try to find it in children
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        CacheFlyingBoolParameter();
    }

    void Start()
    {
        _model = new DroneModel(droneId, displayName, transform.position);
        _registry.Register(_model);
        _model.OnStateChanged += OnDroneStateChanged;

        SetFlying(false);
    }

    void Update()
    {
        if (_model == null) return;

        switch (_model.State)
        {
            case DroneState.MovingToPickup:
            case DroneState.MovingToDropoff:
            case DroneState.Returning:
                UpdateProgress();
                CheckArrival();
                break;
        }
    }

    // Called after Dispatcher validates the assignment.
    // Called by DispatchPanelView after user assigns this drone to a job
    public void StartJob(JobModel job)
    {
        _currentJob = job;

        // Find the actual waypoint transforms in the scene by name
        _pickupTarget = _waypoints.GetLocation(job.PickupLocationName);
        _dropoffTarget = _waypoints.GetLocation(job.DropoffLocationName);

        if (_pickupTarget == null || _dropoffTarget == null)
        {
            Debug.LogError($"[DroneController] Missing waypoint for job {job.Name}. " +
                $"Make sure GameObjects with Waypoint component exist for '{job.PickupLocationName}' and '{job.DropoffLocationName}'.");
            return;
        }

        SetFlying(true);
        NavigateTo(_pickupTarget.position);
    }

    // Called every frame while drone is moving
    void CheckArrival()
    {
        if (_agent.pathPending) return;
        if (_agent.remainingDistance > arrivedThreshold) return;

        // State machine: what to do when we arrive at current destination
        switch (_model.State)
        {
            case DroneState.MovingToPickup:
                StartCoroutine(PerformAction(DroneState.PickingUp, () =>
                {
                    _currentJob.SetStatus(JobStatus.InProgress);
                    _model.SetState(DroneState.MovingToDropoff);
                    NavigateTo(_dropoffTarget.position);
                }));
                break;

            case DroneState.MovingToDropoff:
                StartCoroutine(PerformAction(DroneState.DroppingOff, () =>
                {
                    _currentJob.SetStatus(JobStatus.Completed);
                    _model.SetState(DroneState.Returning);
                    NavigateTo(_model.BasePosition);
                }));
                break;

            case DroneState.Returning:
                SetFlying(false);
                MissionProgress = 0f;
                _currentJob = null;
                _model.ClearJob();
                break;
        }
    }

    IEnumerator PerformAction(DroneState actionState, System.Action onComplete)
    {
        _model.SetState(actionState);
        _agent.isStopped = true;
        SetFlying(false);

        transform.DOPunchScale(Vector3.one * 0.12f, actionDuration * 0.5f, 2);

        yield return new WaitForSeconds(actionDuration);

        _agent.isStopped = false;
        SetFlying(true);
        onComplete?.Invoke();
    }

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

    void NavigateTo(Vector3 worldPos)
    {
        _agent.SetDestination(worldPos);
        _totalDistance = Vector3.Distance(transform.position, worldPos);
        _coveredDistance = 0f;
    }

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

    void CacheFlyingBoolParameter()
    {
        _hasFlyingBool = false;
        _flyingBoolHash = 0;

        if (animator == null)
            return;

        // Try inspector-defined name first, then common variants.
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
        // signals propagated through DroneRegistry → SignalBus
    }

    public DroneModel Model => _model;
    public string DroneId => droneId;
}
}
