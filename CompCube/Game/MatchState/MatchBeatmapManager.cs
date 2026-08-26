using CompCube.Models;
using CompCube.Interfaces;
using Zenject;

namespace CompCube.Game.MatchState;

public class MatchBeatmapManager() : IInitializable, IDisposable
{
    [Inject] private readonly IServerListener _serverListener = null!;
    
    private List<VotingMap> _discardedMaps = [];
    private bool _discardSubmissionStarted;
    
    public IReadOnlyList<VotingMap?> DiscardedMaps => _discardedMaps;

    public bool InDiscardPhase { get; private set; } = true;

    private List<VotingMap> _maps = [];

    public IReadOnlyList<VotingMap> AvailablePicks => _maps;
    
    public bool CanDiscardMaps => InDiscardPhase && DiscardedMaps.Count < 2 && !_discardSubmissionStarted;

    public bool DiscardPhaseWasSkipped { get; private set; } = false;

    public event Action? CanNoLongerDiscardMaps;

    public bool DiscardMap(VotingMap map)
    {
        if (!CanDiscardMaps || !_maps.Remove(map))
            return false;
        
        _discardedMaps.Add(map);
        
        if (_discardedMaps.Count == 2)
        {
            _discardSubmissionStarted = true;
            CanNoLongerDiscardMaps?.Invoke();
        }

        return true;
    }

    public void SkipDiscardingMaps()
    {
        if (!InDiscardPhase || _discardSubmissionStarted)
            return;

        DiscardPhaseWasSkipped = true;
        _discardSubmissionStarted = true;
        CanNoLongerDiscardMaps?.Invoke();
    }

    public void Initialize()
    {
        _serverListener.OnMatchCreated += HandleMatchCreated;
        _serverListener.OnPickPhaseStarted += HandlePickPhaseStarted;
    }

    private void HandlePickPhaseStarted(PickPhaseMessage packet)
    {
        _maps = packet.AvailableMaps.ToList();
        InDiscardPhase = false;
    }
    
    private void HandleMatchCreated(MatchCreatedMessage packet)
    {
        _maps = packet.InitialMaps.ToList();
        _discardedMaps = [];
        _discardSubmissionStarted = false;
        
        InDiscardPhase = true;
        DiscardPhaseWasSkipped = false;
    }

    public void Dispose()
    {
        _serverListener.OnMatchCreated -= HandleMatchCreated;
        _serverListener.OnPickPhaseStarted -= HandlePickPhaseStarted;
    }
}
