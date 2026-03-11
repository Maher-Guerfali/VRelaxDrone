using System.Collections.Generic;
using DroneDispatcher.Models;

namespace DroneDispatcher.Services
{
// Interface for the job management service.
// Exposes a read-only list of all jobs and methods to look up / initialize them.
// GameInstaller binds this to JobService.
public interface IJobService
{
    IReadOnlyList<JobModel> Jobs { get; }
    JobModel GetJob(string jobId);
    void Initialize();  // loads ScriptableObjects from Resources/Jobs/
}
}
