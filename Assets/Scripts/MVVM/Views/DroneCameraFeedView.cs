using DroneDispatcher.Drone;
using DroneDispatcher.MVVM.ViewModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace DroneDispatcher.MVVM.Views
{
public class DroneCameraFeedView : MonoBehaviour
{
    [SerializeField] RawImage feedImage;
    [SerializeField] TMP_Text label;
    [SerializeField] string emptyLabel = "Select a drone to view feed";
    [SerializeField] bool hideWhenNoSelection;

    DronePanelViewModel _droneVM;

    [Inject]
    public void Construct(DronePanelViewModel droneVM)
    {
        _droneVM = droneVM;
    }

    void OnEnable()
    {
        _droneVM.OnSelectedDroneChanged += OnSelectedDroneChanged;
        _droneVM.OnDronesUpdated += OnDronesUpdated;
        RefreshFeed();
    }

    void OnDisable()
    {
        _droneVM.OnSelectedDroneChanged -= OnSelectedDroneChanged;
        _droneVM.OnDronesUpdated -= OnDronesUpdated;
    }

    void OnSelectedDroneChanged(string _) => RefreshFeed();
    void OnDronesUpdated() => RefreshFeed();

    void RefreshFeed()
    {
        if (feedImage == null)
            return;

        var selectedDroneId = _droneVM.SelectedDroneId;
        if (DroneCameraFeedSource.TryGetTexture(selectedDroneId, out var texture))
        {
            feedImage.texture = texture;
            feedImage.enabled = true;
            if (label != null)
                label.text = $"Feed: {selectedDroneId}";
            return;
        }

        feedImage.texture = null;
        feedImage.enabled = !hideWhenNoSelection;
        if (label != null)
            label.text = emptyLabel;
    }
}
}