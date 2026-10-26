using System;

public enum MapVotePhase
{
	Playing = 0,
	Voting = 1,
	Changing = 2,
}

/// <summary>
/// Таймер карты и голосование за следующую. Состояние пишет только хост, клиенты голосуют через RPC.
/// </summary>
public sealed class MapVote : Component
{
	public static MapVote Instance { get; private set; }

	[Property] public float MapDuration { get; set; } = 600f;
	[Property] public float VoteDuration { get; set; } = 30f;
	[Property] public float ChangeDelay { get; set; } = 3f;

	[Sync(SyncFlags.FromHost)] public MapVotePhase Phase { get; private set; } = MapVotePhase.Playing;
	[Sync(SyncFlags.FromHost)] public int SecondsLeft { get; private set; } = 0; // до конца текущей фазы
	[Sync(SyncFlags.FromHost)] public int CurrentMap { get; private set; } = -1;
	[Sync(SyncFlags.FromHost)] public int NextMap { get; private set; } = -1;
	[Sync(SyncFlags.FromHost)] public NetDictionary<Guid, int> Votes { get; set; } = new();

	/// <summary>
	/// Голос этого клиента в текущем голосовании, -1 если ещё не голосовал.
	/// </summary>
	public int LocalVote { get; private set; } = -1;

	private RealTimeUntil _phaseEnds;
	private bool _isHostReady = false;
	private bool _isChangeRequested = false;

	public IEnumerable<int> GetOptions()
	{
		for (int i = 0; i < MapList.All.Count; i++)
		{
			if (i == CurrentMap) continue;

			yield return i;
		}
	}

	public int GetVoteCount(int map)
	{
		return Votes.Count(x => x.Value == map);
	}

	public void Vote(int map)
	{
		if (Phase != MapVotePhase.Voting) return;

		LocalVote = map;
		CastVoteRpc(map);
	}

	public void StartVoteNow()
	{
		StartVoteRpc();
	}

	public void ForceChangeMap(int map)
	{
		ForceChangeMapRpc(map);
	}

	[Rpc.Host]
	private void CastVoteRpc(int map)
	{
		if (Phase != MapVotePhase.Voting) return;
		if (MapList.Get(map) is null) return;

		var caller = Rpc.Caller;
		if (caller is null) return;

		Votes.Remove(caller.Id);
		Votes.Add(caller.Id, map);
	}

	[Rpc.Host]
	private void StartVoteRpc()
	{
		if (Phase != MapVotePhase.Playing) return;

		StartVote();
	}

	[Rpc.Host]
	private void ForceChangeMapRpc(int map)
	{
		if (MapList.Get(map) is null) return;

		SetChanging(map);
	}

	private void StartVote()
	{
		Votes.Clear();
		Phase = MapVotePhase.Voting;
		_phaseEnds = VoteDuration;

		Log.Info("[MapVote] Start vote");
	}

	private void FinishVote()
	{
		var options = GetOptions().ToList();
		if (options.Count == 0)
		{
			Phase = MapVotePhase.Playing;
			_phaseEnds = MapDuration;
			return;
		}

		var maxVotes = options.Max(GetVoteCount);
		var winners = options.Where(x => GetVoteCount(x) == maxVotes).ToList(); // ничья или нет голосов - случайная из лидеров

		SetChanging(Game.Random.FromList(winners));
	}

	private void SetChanging(int map)
	{
		NextMap = map;
		Phase = MapVotePhase.Changing;
		_phaseEnds = ChangeDelay;

		Log.Info($"[MapVote] Next map: {MapList.Get(map)?.Name}");
	}

	private bool IsEveryoneVoted()
	{
		return Votes.Count > 0 && Votes.Count >= Connection.All.Count;
	}

	private void PrepareHost()
	{
		_isHostReady = true;

		if (CurrentMap < 0)
			CurrentMap = MapList.IndexOfScene(Scene.Source?.ResourcePath);

		// после смены хоста продолжаем с синхронизированного таймера
		if (Phase == MapVotePhase.Playing && SecondsLeft <= 0)
			_phaseEnds = MapDuration;
		else
			_phaseEnds = SecondsLeft;
	}

	private void UpdateHost()
	{
		if (!_isHostReady) PrepareHost();

		if (Phase == MapVotePhase.Voting && IsEveryoneVoted())
			_phaseEnds = 0f;

		SecondsLeft = Math.Max(0, ((float)_phaseEnds).CeilToInt());

		if (_phaseEnds > 0f) return;

		switch (Phase)
		{
			case MapVotePhase.Playing:
				StartVote();
				break;

			case MapVotePhase.Voting:
				FinishVote();
				break;

			case MapVotePhase.Changing:
				if (_isChangeRequested) break;

				_isChangeRequested = true;
				MapList.ChangeMap(NextMap);
				break;
		}
	}

	protected override void OnAwake()
	{
		Instance = this;
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
			Instance = null;
	}

	protected override void OnUpdate()
	{
		if (Phase != MapVotePhase.Voting)
			LocalVote = -1;

		if (!Networking.IsHost) return;

		UpdateHost();
	}
}
