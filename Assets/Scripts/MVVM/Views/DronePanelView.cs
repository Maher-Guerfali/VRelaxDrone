using System.Collections;
using System.Collections.Generic;
using DroneDispatcher.MVVM.ViewModels;
using UnityEngine;
using Zenject;

namespace DroneDispatcher.MVVM.Views
{
// Displays the list of drones in the UI.
// Similar to JobPanelView but with a twist: drones register themselves in Start(),
// so we need to wait one frame before the first Refresh() to make sure they're all registered.
public class DronePanelView : MonoBehaviour
{
    [SerializeField] Transform contentParent;     // ScrollView content container
    [SerializeField] DroneEntryView entryPrefab;  // prefab for a single drone row

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
        // Wait one frame so all DroneControllers have time to register in their Start()
        StartCoroutine(DelayedRefresh());
    }

    IEnumerator DelayedRefresh()
    {
        yield return null;  // skip one frame
        _vm.Refresh();      // now DroneRegistry has all drones
    }

    // Same pattern as JobPanelView: destroy all entries and recreate
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
