namespace CompCube.Models;

/** Messages below are internal UI events after Socket.IO payloads have been validated and converted. */
public sealed class MatchCreatedMessage(UserInfo red, UserInfo blue, VotingMap[] initialMaps)
{
    public UserInfo Red { get; } = red;
    public UserInfo Blue { get; } = blue;
    public VotingMap[] InitialMaps { get; } = initialMaps;
}

public sealed class PlayerSelectedMapMessage(VotingMap map)
{
    public VotingMap Map { get; } = map;
}

public sealed class RoundResultsMessage(Score redScore, Score blueScore, float redHealth, float blueHealth, DateTime resultsDueAt)
{
    public Score RedScore { get; } = redScore;
    public Score BlueScore { get; } = blueScore;
    public float RedHealth { get; } = redHealth;
    public float BlueHealth { get; } = blueHealth;
	public DateTime ResultsDueAt { get; } = resultsDueAt;
}

public sealed class PickPhaseMessage(VotingMap[] availableMaps, bool isOwnPick, float newMultiplier)
{
    public VotingMap[] AvailableMaps { get; } = availableMaps;
    public bool IsOwnPick { get; } = isOwnPick;
    public float NewMultiplier { get; } = newMultiplier;
}

public sealed class MatchFinishedMessage(int mmrChange, string result, string? reason = null)
{
    public int MmrChange { get; } = mmrChange;
    public string Result { get; } = result;
    public string? Reason { get; } = reason;
    public bool Won => Result == "win";
}

public sealed class CardsUpdatedMessage(VotingMap[] maps)
{
    public VotingMap[] Maps { get; } = maps;
}

/** Contains the score submission values accepted by the TypeScript backend. */
public sealed class ScoreSubmission(
    int rawScore,
    int modifiedScore,
    bool noFailTriggered,
    bool proMode,
    int missCount,
    bool fullCombo)
{
    public int RawScore { get; } = rawScore;
    public int ModifiedScore { get; } = modifiedScore;
    public bool NoFailTriggered { get; } = noFailTriggered;
    public bool ProMode { get; } = proMode;
    public int MissCount { get; } = missCount;
    public bool FullCombo { get; } = fullCombo;
}
