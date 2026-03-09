using System.Collections;
using System.Collections.Generic;
using DroneDispatcher.MVVM.ViewModels;
using UnityEngine;
using Zenject;

namespace DroneDispatcher.MVVM.Views
{
public class DronePanelView : MonoBehaviour
{
    [SerializeField] Transform contentParent;
    [SerializeField] DroneEntryView entryPrefab;

    DronePanelViewModel _vm;
    readonly List<DroneEntryView> _entries = new List<DroneEntryView>();

    [Inject]
    public void Construct(DronePanelViewModel vm)
    {
        _vm = vm;
    }

    void OnEnable()
    {
        _vm.OnDronesUpdated += Rebuild;
    }

    void OnDisable()
    {
        _vm.OnDronesUpdated -= Rebuild;
    }

    void Start()
    {
        // Drones register in their Start(), so wait a frame before first build
        StartCoroutine(DelayedRefresh());
    }

    IEnumerator DelayedRefresh()
    {
        yield return null;
        _vm.Refresh();
    }

    void Rebuild()
    {
        foreach (var e in _entries) Destroy(e.gameObject);
        _entries.Clear();

        foreach (var drone in _vm.Drones)
        {
            var entry = Instantiate(entryPrefab, contentParent);
            var jobInfo = string.IsNullOrEmpty(drone.CurrentJobId) ? "—" : drone.CurrentJobId;
            entry.Setup(drone.Id, drone.DisplayName, drone.State.ToString(), jobInfo, OnDroneSelected);
            entry.SetSelected(drone.Id == _vm.SelectedDroneId);
            _entries.Add(entry);
        }
    }

    void OnDroneSelected(string droneId)
    {
        _vm.SelectDrone(droneId);

        for (int i = 0; i < _vm.Drones.Count && i < _entries.Count; i++)
            _entries[i].SetSelected(_vm.Drones[i].Id == droneId);
    }
}
}
