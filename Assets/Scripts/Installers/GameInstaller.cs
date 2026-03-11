using DroneDispatcher.Core;
using DroneDispatcher.MVVM.ViewModels;
using DroneDispatcher.Services;
using Zenject;

namespace DroneDispatcher.Installers
{
// This is the heart of our dependency injection setup.
// Zenject reads this at scene load and creates all the singletons we need.
// Every class that has [Inject] or constructor injection gets its dependencies from here.
// We attach this to the SceneContext GameObject in Unity — that's how Zenject knows to run it.
public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // --- Signal Bus ---
        // First we install the SignalBus itself (Zenject's built-in event system),
        // then declare which signal types we'll be firing.
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<JobStatusChangedSignal>();
        Container.DeclareSignal<DroneStateChangedSignal>();

        // --- Services (business logic layer) ---
        // Bind interface → concrete class, AsSingle = one instance shared everywhere.
        // This means anyone who asks for IJobService gets the same JobService object.
        Container.Bind<IJobService>().To<JobService>().AsSingle();
        Container.Bind<IDroneRegistry>().To<DroneRegistry>().AsSingle();
        Container.Bind<IDispatcher>().To<Dispatcher>().AsSingle();

        // BindInterfacesAndSelfTo = binds both IWaypointRegistry AND WaypointRegistry.
        // We need this because WaypointRegistry doesn't implement IInitializable,
        // but we still want it accessible by both its interface and concrete type.
        Container.BindInterfacesAndSelfTo<WaypointRegistry>().AsSingle();

        // --- ViewModels (presentation logic) ---
        // BindInterfacesAndSelfTo is used here because JobPanelViewModel implements IInitializable,
        // so Zenject will automatically call its Initialize() method after all bindings are resolved.
        // Same for DronePanelViewModel which implements IDisposable.
        Container.BindInterfacesAndSelfTo<JobPanelViewModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<DronePanelViewModel>().AsSingle();
        Container.Bind<DispatchViewModel>().AsSingle();
    }
}
}
