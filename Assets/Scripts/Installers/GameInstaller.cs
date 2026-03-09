using DroneDispatcher.Core;
using DroneDispatcher.MVVM.ViewModels;
using DroneDispatcher.Services;
using Zenject;

namespace DroneDispatcher.Installers
{
public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Signals
        SignalBusInstaller.Install(Container);
        Container.DeclareSignal<JobStatusChangedSignal>();
        Container.DeclareSignal<DroneStateChangedSignal>();

        // Services
        Container.Bind<IJobService>().To<JobService>().AsSingle();
        Container.Bind<IDroneRegistry>().To<DroneRegistry>().AsSingle();
        Container.Bind<IDispatcher>().To<Dispatcher>().AsSingle();
        Container.BindInterfacesAndSelfTo<WaypointRegistry>().AsSingle();

        // ViewModels
        Container.BindInterfacesAndSelfTo<JobPanelViewModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<DronePanelViewModel>().AsSingle();
        Container.Bind<DispatchViewModel>().AsSingle();
    }
}
}
