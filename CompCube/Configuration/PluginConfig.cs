using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace CompCube.Configuration
{
    public class PluginConfig
    {
        public virtual string WebsocketIp { get; set; } = "wss://ws.compcube.net";
        public virtual string ApiIP { get; set; } = "https://api.compcube.net";

        public virtual bool ScoreSubmission { get; set; } = true;

        public virtual bool ConnectToDebugQueue { get; set; } = false;
        public virtual bool SkipServer { get; set; } = false;
    }
}