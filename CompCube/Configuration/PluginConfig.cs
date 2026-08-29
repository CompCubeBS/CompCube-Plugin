using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace CompCube.Configuration
{
    public class PluginConfig
    {
        public virtual string WebsocketIp { get; set; } = "wss://ws.compcube.net";
        public virtual string ApiIP { get; set; } = "https://api.compcube.net";
		public virtual string BeatKhanaApiIP { get; set; } = "https://api.beatkhana.com";

        public virtual bool ScoreSubmission { get; set; } = true;
		// Must match ROUND_RESULTS_SECONDS on the backend; the server validates it at connection time.
		public virtual float RoundResultsDurationSeconds { get; set; } = 6f;

        public virtual bool SkipServer { get; set; } = false;
    }
}