using System.ComponentModel;
using CompCube.Configuration;
using CompCube.Game;
using CompCube.Game.MatchState;
using CompCube.Server;
using CompCube.Server.Debug;
using CompCube.Interfaces;
using CompCube.Networking;
using CompCube.UI;
using Zenject;

namespace CompCube.Installers
{
    internal class AppInstaller : Installer
    {
        private readonly PluginConfig _config;

        public AppInstaller(PluginConfig config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config);
            
            Container.BindInterfacesAndSelfTo<TransitionToLevelManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<DisconnectHandler>().AsSingle();
            Container.BindInterfacesAndSelfTo<MatchStateManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<MatchBeatmapManager>().AsSingle();
            
            Container.BindInterfacesAndSelfTo<UserModelWrapper>().AsSingle();

            Container.BindInterfacesAndSelfTo<SharedCoroutineStarter>().FromNewComponentOnNewGameObject().AsSingle();

            if (_config.SkipServer)
            {
                Container.BindInterfacesAndSelfTo<DebugServerListener>().AsSingle();
                Container.BindInterfacesAndSelfTo<DebugApi>().AsSingle();
                return;
            }

            Container.BindInterfacesAndSelfTo<ServerListener>().AsSingle();
            Container.BindInterfacesAndSelfTo<Api>().AsSingle();
        }
    }
}