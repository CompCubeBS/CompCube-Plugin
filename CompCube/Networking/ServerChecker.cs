using CompCube_Models.Models.Server;
using CompCube.Interfaces;
using Zenject;

namespace CompCube.Server;

public class ServerChecker
{
    [Inject] private readonly IApi _api = null!;
    
    public async Task<ServerCheckingResults> CanConnectToServer()
    {
        var serverStatus = await _api.GetServerStatus();

        if (serverStatus == null)
            return new ServerCheckingResults(false, "Could not connect to server!");
        
        if (!serverStatus.AllowedModVersions.Contains(IPA.Loader.PluginManager.GetPluginFromId("CompCube").HVersion.ToString()))
            return new ServerCheckingResults(false, "Plugin version is outdated. Please update your mod!");

        if (serverStatus.State == ServerState.State.Maintenance)
            return new ServerCheckingResults(false, "Server is undergoing maintenance. Please check back later!");
        
        return new ServerCheckingResults(true);
    }
}

public class ServerCheckingResults(bool canConnect, string reasonIfCant = "")
{
    public readonly bool CanConnect = canConnect;
    public readonly string Reason = reasonIfCant;
}