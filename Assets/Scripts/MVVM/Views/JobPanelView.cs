using System.Collections.Generic;
using DroneDispatcher.MVVM.ViewModels;
using UnityEngine;
using Zenject;

namespace DroneDispatcher.MVVM.Views
{
// Displays the list of available jobs in the UI.
// Subscribes to JobPanelViewModel.OnJobsUpdated and rebuilds the entire list
// whenever a job's status changes. Each row is a JobEntryView prefab instance.
// This is a "dumb" View — it just renders what the ViewModel tells it to.
public class JobPanelView : MonoBehaviour
{
    [SerializeField] Transform contentParent;   // the ScrollView content container
    [SerializeField] JobEntryView entryPrefab;  // prefab for a single job row

    JobPanelViewModel _vm;
    readonly List<JobEntryView> _entries = new List<JobEntryView>();

    [Inject]
    public void Construct(JobPanelViewModel vm)
    {
        _vm = vm;
    }

    void OnEnable()
    {
        _vm.OnJobsUpdated += Rebuild;  // rebuild UI whenever jobs change
    }

    void OnDisable()
    {
        _vm.OnJobsUpdated -= Rebuild;
    }

    void Start()
    {
        // First render — Zenject already called JobPanelViewModel.Initialize() by now
        Rebuild();
    }

    // Destroy all existing entries and recreate from scratch.
    // Simple approach — works well for small lists. For large lists we'd use object pooling.
    void Rebuild()
    {
        foreach (var e in _entries) Destroy(e.gameObject);
        _entries.Clear();

        foreach (var job in _vm.Jobs)
        {
            var entry = Instantiate(entryPrefab, contentParent);
            entry.Setup(job.Id, job.Name, job.Status.ToString(), OnJobSelected);
            entry.SetSelected(job.Id == _vm.SelectedJobId);
            _entries.Add(entry);
        }
    }

    // Called when user clicks a job row — tell ViewModel and update highlights
    void OnJobSelected(string jobId)
    {
        _vm.SelectJob(jobId);

        for (int i = 0; i < _vm.Jobs.Count && i < _entries.Count; i++)
            _entries[i].SetSelected(_vm.Jobs[i].Id == jobId);
    }
}
}
