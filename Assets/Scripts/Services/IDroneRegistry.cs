using System.Collections.Generic;
using DroneDispatcher.Models;

namespace DroneDispatcher.Services
{
// Interface for the drone tracking service.
// DroneControllers call Register() in their Start() to add themselves.
// The rest of the app uses Drones/GetDrone to query drone state.
public interface IDroneRegistry
{
    IReadOnlyList<DroneModel> Drones { get; }
    DroneModel GetDrone(string droneId);
    void Register(DroneModel drone);
}
}
