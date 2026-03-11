using TMPro;
using DroneDispatcher.Drone;
using UnityEngine;
using UnityEngine.UI;

namespace DroneDispatcher.MVVM.Views
{
// A single row in the drone list UI. Similar to JobEntryView but with extra fields.
// Also has an optional RawImage that can show the drone's live camera feed.
// Uses DroneCameraFeedSource's static registry to find the right RenderTexture.
public class DroneEntryView : MonoBehaviour
{
    [SerializeField] TMP_Text nameLabel;
    [SerializeField] TMP_Text stateLabel;
    [SerializeField] TMP_Text jobLabel;
    [SerializeField] Button selectButton;
    [SerializeField] Image background;

    [Header("Optional Camera Preview")]
    [SerializeField] RawImage cameraPreview;
    [SerializeField] bool autoBindCameraTexture = true;
    [SerializeField] string cameraNameFormat = "{0}_Camera";

    string _droneId;
    System.Action<string> _onSelect;

    void Awake()
    {
        if (nameLabel == null || stateLabel == null || selectButton == null)
            Debug.LogWarning($"[DroneEntryView] Missing required UI references on '{name}'.", this);
    }

    public void Setup(string droneId, string droneName, string state, string jobInfo, System.Action<string> onSelect)
    {
        _droneId = droneId;
        _onSelect = onSelect;

        if (nameLabel != null) nameLabel.text = droneName;
        if (stateLabel != null) stateLabel.text = state;
        if (jobLabel != null) jobLabel.text = jobInfo;

        BindCameraPreview(droneId, droneName);

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => _onSelect?.Invoke(_droneId));
        }
    }

    // Auto-find scene camera by name and bind its render texture to this UI row
    void BindCameraPreview(string droneId, string droneName)
    {
        if (cameraPreview == null || !autoBindCameraTexture)
            return;

        // Fast path: use runtime registry when available.
        RenderTexture texture = null;
        if (!DroneCameraFeedSource.TryGetTexture(droneId, out texture))
            texture = FindTextureByCameraName(droneId, droneName);

        cameraPreview.texture = texture;
        cameraPreview.enabled = texture != null;
    }

    RenderTexture FindTextureByCameraName(string droneId, string droneName)
    {
        if (string.IsNullOrWhiteSpace(cameraNameFormat))
            return null;

        var cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        if (cameras == null || cameras.Length == 0)
            return null;

        var candidateA = FormatCameraName(droneId);
        var candidateB = FormatCameraName(droneName);

        for (int i = 0; i < cameras.Length; i++)
        {
            var cam = cameras[i];
            if (cam == null || cam.targetTexture == null)
                continue;

            if (IsMatch(cam.name, candidateA) || IsMatch(cam.name, candidateB))
                return cam.targetTexture;
        }

        return null;
    }

    string FormatCameraName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return string.Format(cameraNameFormat, value).Trim();
    }

    bool IsMatch(string cameraName, string candidateName)
    {
        if (string.IsNullOrWhiteSpace(cameraName) || string.IsNullOrWhiteSpace(candidateName))
            return false;

        return string.Equals(cameraName, candidateName, System.StringComparison.OrdinalIgnoreCase);
    }

    public void SetSelected(bool selected)
    {
        if (background != null)
            background.color = selected ? new Color(0.3f, 0.6f, 1f, 0.4f) : new Color(1f, 1f, 1f, 0.1f);
    }
}
}
