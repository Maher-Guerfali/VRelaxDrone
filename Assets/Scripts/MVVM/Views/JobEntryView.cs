using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DroneDispatcher.MVVM.Views
{
// A single row in the job list UI. Instantiated at runtime by JobPanelView.
// It's a prefab with a name label, status label, select button, and background highlight.
// Has no logic — just receives data via Setup() and forwards clicks via callback.
public class JobEntryView : MonoBehaviour
{
    [SerializeField] TMP_Text nameLabel;
    [SerializeField] TMP_Text statusLabel;
    [SerializeField] Button selectButton;
    [SerializeField] Image background;

    string _jobId;
    System.Action<string> _onSelect;

    public void Setup(string jobId, string jobName, string status, System.Action<string> onSelect)
    {
        _jobId = jobId;
        _onSelect = onSelect;

        nameLabel.text = jobName;
        statusLabel.text = status;

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => _onSelect?.Invoke(_jobId));
    }

    public void SetSelected(bool selected)
    {
        if (background != null)
            background.color = selected ? new Color(0.3f, 0.6f, 1f, 0.4f) : new Color(1f, 1f, 1f, 0.1f);
    }

    public void UpdateStatus(string status)
    {
        statusLabel.text = status;
    }
}
}
