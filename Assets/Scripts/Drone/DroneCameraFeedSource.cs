using System.Collections.Generic;
using UnityEngine;

namespace DroneDispatcher.Drone
{
[DisallowMultipleComponent]
[RequireComponent(typeof(DroneController))]
public class DroneCameraFeedSource : MonoBehaviour
{
    static readonly Dictionary<string, DroneCameraFeedSource> SourcesByDroneId = new Dictionary<string, DroneCameraFeedSource>();

    [Header("Feed Source")]
    [SerializeField] Camera droneCamera;
    [SerializeField] RenderTexture textureTemplate;
    [SerializeField] bool createUniqueRuntimeTexture = true;
    [SerializeField] int fallbackWidth = 1024;
    [SerializeField] int fallbackHeight = 1024;
    [SerializeField] int depthBuffer = 16;

    DroneController _controller;
    RenderTexture _runtimeTexture;

    public string DroneId => _controller != null ? _controller.DroneId : null;
    public RenderTexture OutputTexture => _runtimeTexture != null ? _runtimeTexture : textureTemplate;

    void Awake()
    {
        _controller = GetComponent<DroneController>();

        if (droneCamera == null)
            droneCamera = GetComponentInChildren<Camera>();

        SetupTexture();
    }

    void OnEnable()
    {
        Register();
    }

    void OnDisable()
    {
        Unregister();
    }

    void OnDestroy()
    {
        if (_runtimeTexture != null)
        {
            if (droneCamera != null && droneCamera.targetTexture == _runtimeTexture)
                droneCamera.targetTexture = null;

            Destroy(_runtimeTexture);
            _runtimeTexture = null;
        }
    }

    void SetupTexture()
    {
        if (droneCamera == null)
        {
            Debug.LogWarning($"[DroneCameraFeedSource] No Camera found on '{name}'.", this);
            return;
        }

        if (createUniqueRuntimeTexture)
        {
            _runtimeTexture = CreateRuntimeTexture();
            droneCamera.targetTexture = _runtimeTexture;
            return;
        }

        if (textureTemplate != null)
            droneCamera.targetTexture = textureTemplate;
    }

    RenderTexture CreateRuntimeTexture()
    {
        RenderTexture texture;

        if (textureTemplate != null)
        {
            texture = Instantiate(textureTemplate);
        }
        else
        {
            texture = new RenderTexture(fallbackWidth, fallbackHeight, depthBuffer)
            {
                graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm,
            };
            texture.Create();
        }

        texture.name = $"RT_{name}";
        return texture;
    }

    void Register()
    {
        if (string.IsNullOrEmpty(DroneId))
        {
            Debug.LogWarning($"[DroneCameraFeedSource] Missing DroneId on '{name}'.", this);
            return;
        }

        SourcesByDroneId[DroneId] = this;
    }

    void Unregister()
    {
        if (string.IsNullOrEmpty(DroneId))
            return;

        if (SourcesByDroneId.TryGetValue(DroneId, out var source) && source == this)
            SourcesByDroneId.Remove(DroneId);
    }

    public static bool TryGetTexture(string droneId, out RenderTexture texture)
    {
        texture = null;

        if (string.IsNullOrEmpty(droneId))
            return false;

        if (!SourcesByDroneId.TryGetValue(droneId, out var source) || source == null)
            return false;

        texture = source.OutputTexture;
        return texture != null;
    }
}
}