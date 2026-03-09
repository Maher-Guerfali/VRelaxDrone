using System.Collections.Generic;
using DroneDispatcher.MVVM.ViewModels;
using UnityEngine;
using Zenject;

namespace DroneDispatcher.MVVM.Views
{
public class JobPanelView : MonoBehaviour
{
    [SerializeField] Transform contentParent;
    [SerializeField] JobEntryView entryPrefab;

    JobPanelViewModel _vm;
    readonly List<JobEntryView> _entries = new List<JobEntryView>();

    [Inject]
    public void Construct(JobPanelViewModel vm)
    {
        _vm = vm;
    }

    void OnEnable()
    {
        _vm.OnJobsUpdated += Rebuild;
    }

    void OnDisable()
    {
        _vm.OnJobsUpdated -= Rebuild;
    }

    void Start()
    {
        // Zenject initializes JobPanelViewModel (IInitializable), so just render current state.
        Rebuild();
    }

    void Rebuild()
    {
        // clear old entries
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

    void OnJobSelected(string jobId)
    {
        _vm.SelectJob(jobId);

        // refresh highlights
        for (int i = 0; i < _vm.Jobs.Count && i < _entries.Count; i++)
            _entries[i].SetSelected(_vm.Jobs[i].Id == jobId);
    }
}
}
