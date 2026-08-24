using CompCube_Models.Models.Map;
using CompCube_Models.Models.Packets.ServerPackets;
using CompCube.Interfaces;
using Zenject;

namespace CompCube.Game.MatchState;

public class MatchBeatmapManager() : IInitializable, IDisposable
{
    [Inject] private readonly IServerListener _serverListener = null!;
    
    private List<VotingMap> _discardedMaps = [];
    
    public IReadOnlyList<VotingMap?> DiscardedMaps => _discardedMaps;

    public bool InDiscardPhase { get; private set; } = true;

    private List<VotingMap> _maps = [];

    public IReadOnlyList<VotingMap> AvailablePicks => _maps;
    
    public bool CanDiscardMaps => InDiscardPhase && DiscardedMaps.Count < 2 && !DiscardPhaseWasSkipped;

    public bool DiscardPhaseWasSkipped { get; private set; } = false;

    public event Action? CanNoLongerDiscardMaps;

    public void DiscardMap(VotingMap map)
    {
        _maps.Remove(map);
        
        _discardedMaps.Add(map);
        
        if (!CanDiscardMaps)
            CanNoLongerDiscardMaps?.Invoke();
    }

    public void SkipDiscardingMaps()
    {
        DiscardPhaseWasSkipped = true;
        CanNoLongerDiscardMaps?.Invoke();
    }

    public void Initialize()
    {
        _serverListener.OnMatchCreated += HandleMatchCreated;
        _serverListener.OnPickPhaseStarted += HandlePickPhaseStarted;
    }

    private void HandlePickPhaseStarted(StartPickPhasePacket packet)
    {
        _maps = packet.AvailableMaps.ToList();
        InDiscardPhase = false;
    }
    
    private void HandleMatchCreated(MatchCreatedPacket packet)
    {
        _maps = [];
        _discardedMaps = [];
        
        InDiscardPhase = true;
        DiscardPhaseWasSkipped = false;
    }

    public void Dispose()
    {
        _serverListener.OnMatchCreated -= HandleMatchCreated;
        _serverListener.OnPickPhaseStarted -= HandlePickPhaseStarted;
    }
}