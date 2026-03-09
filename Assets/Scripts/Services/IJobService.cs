using System.Collections.Generic;
using DroneDispatcher.Models;

namespace DroneDispatcher.Services
{
public interface IJobService
{
    IReadOnlyList<JobModel> Jobs { get; }
    JobModel GetJob(string jobId);
    void Initialize();
}
}
