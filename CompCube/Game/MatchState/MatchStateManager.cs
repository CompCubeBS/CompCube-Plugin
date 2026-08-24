using CompCube_Models.Models.Packets.ServerPackets;
using CompCube.Interfaces;
using Zenject;

namespace CompCube.Game.MatchState;

public class MatchStateManager : IInitializable, IDisposable
{
    [Inject] private readonly IServerListener _serverListener = null!;
    [Inject] private readonly UserModelWrapper _userModelWrapper = null!;

    public CompCube_Models.Models.ClientData.UserInfo RedPlayer { get; private set; }
    public CompCube_Models.Models.ClientData.UserInfo BluePlayer { get; private set; }

    public float RedHealth { get; private set; } = 1f;
    public float BlueHealth { get; private set; } = 1f;

    public float DamageMultiplier { get; private set; } = 1f;

    public bool IsRedTeam => RedPlayer.UserId == _userModelWrapper.UserId;
    
    public CompCube_Models.Models.ClientData.UserInfo Opponent => !IsRedTeam ? RedPlayer : BluePlayer;
    
    public CompCube_Models.Models.ClientData.UserInfo Self => IsRedTeam ? RedPlayer : BluePlayer;
    
    public int CurrentRound { get; private set; } = 0;
    
    public void Initialize()
    {
        _serverListener.OnMatchCreated += HandleMatchCreated;
        _serverListener.OnRoundResults += HandleRoundResults;
        _serverListener.OnPickPhaseStarted += HandlePickPhaseStarted;
    }

    private void HandlePickPhaseStarted(StartPickPhasePacket packet)
    {
        CurrentRound++;
        DamageMultiplier = packet.NewMultiplier;
    }

    private void HandleMatchCreated(MatchCreatedPacket matchCreated)
    {
        RedPlayer = matchCreated.Red;
        BluePlayer = matchCreated.Blue;
        
        RedHealth = 1f;
        BlueHealth = 1f;

        DamageMultiplier = 1.0f;

        CurrentRound = 0;
    }
    
    private void HandleRoundResults(RoundResultsPacket results)
    {
        RedHealth = results.RedHealth;
        BlueHealth = results.BlueHealth;
    }

    public void Dispose()
    {
        _serverListener.OnMatchCreated -= HandleMatchCreated;
        _serverListener.OnRoundResults -= HandleRoundResults;
        _serverListener.OnPickPhaseStarted -= HandlePickPhaseStarted;
    }
}