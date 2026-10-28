using CompCube_Models.Models.ClientData;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube.Interfaces;
using Zenject;

namespace CompCube.Game.MatchState;

public class MatchStateManager : IInitializable, IDisposable
{
    [Inject] private readonly IServerListener _serverListener = null!;
    [Inject] private readonly UserModelWrapper _userModelWrapper = null!;

    public UserStatistics RedPlayer { get; private set; }
    public UserStatistics BluePlayer { get; private set; }

    public float RedHealth { get; private set; } = 1f;
    public float BlueHealth { get; private set; } = 1f;

    public float DamageMultiplier { get; private set; } = 1f;

    public bool IsRedTeam => RedPlayer.PlatformId == _userModelWrapper.UserId;
    
    public UserStatistics Opponent => !IsRedTeam ? RedPlayer : BluePlayer;
    
    public UserStatistics Self => IsRedTeam ? RedPlayer : BluePlayer;
    
    public int CurrentRound { get; private set; } = 0;
    
    public int RemainingReplays { get; private set; } = 1;
    
    public void Initialize()
    {
        _serverListener.OnMatchCreated += HandleMatchCreated;
        _serverListener.OnPickPhaseStarted += HandlePickPhaseStarted;
    }

    private void HandlePickPhaseStarted(StartPickPhasePacket packet)
    {
        CurrentRound++;
        DamageMultiplier = packet.Multiplier;
        RedHealth = packet.RedHealth;
        BlueHealth = packet.BlueHealth;
    }

    private void HandleMatchCreated(MatchCreatedPacket matchCreated)
    {
        RedPlayer = matchCreated.Red;
        BluePlayer = matchCreated.Blue;
        
        RedHealth = 1f;
        BlueHealth = 1f;

        DamageMultiplier = 1.0f;

        CurrentRound = 0;
        
        RemainingReplays = 1;
    }

    public async Task UseReplay()
    {
        if (RemainingReplays <= 0)
            throw new Exception("Tried to use replay with no more available replays");

        RemainingReplays--;

        await _serverListener.SendReplayRequestAsync(true);
    }

    public void Dispose()
    {
        _serverListener.OnMatchCreated -= HandleMatchCreated;
        _serverListener.OnPickPhaseStarted -= HandlePickPhaseStarted;
    }
}