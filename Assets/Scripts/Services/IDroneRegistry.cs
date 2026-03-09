using System.Collections.Generic;
using DroneDispatcher.Models;

namespace DroneDispatcher.Services
{
public interface IDroneRegistry
{
    IReadOnlyList<DroneModel> Drones { get; }
    DroneModel GetDrone(string droneId);
    void Register(DroneModel drone);
}
}
