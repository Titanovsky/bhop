public enum MapLoadState
{
	WaitingForHost = 0,
	LoadingMap = 1,
	Ready = 2,
}

/// <summary>
/// Лежит на каждой карте. Каждый клиент сам спавнит своего игрока и становится его владельцем,
/// но только когда карта загружена у хоста и у него самого - иначе игрок падает в пустоту.
/// </summary>
public sealed class GameManager : Component
{
	public static GameManager Instance { get; private set; }

	[Property] public GameObject PlayerPrefab { get; set; }

	/// <summary>
	/// Карта загружена у хоста.
	/// </summary>
	[Sync(SyncFlags.FromHost)] public bool IsHostReady { get; private set; } = false;

	private bool _isHostReadyRpc = false; // дублирует Sync, если RPC дошёл раньше снапшота
	private bool _isSpawned = false;
	private MapLoadState _lastState = MapLoadState.WaitingForHost;

	public bool IsHostMapReady => Networking.IsHost || IsHostReady || _isHostReadyRpc;

	/// <summary>
	/// Все карты (MapInstance) этой сцены загружены у нас.
	/// </summary>
	public bool IsLocalMapReady => Scene.GetAllComponents<MapInstance>().All(x => x.IsLoaded);

	public MapLoadState State
	{
		get
		{
			if (!IsHostMapReady) return MapLoadState.WaitingForHost;
			if (!IsLocalMapReady) return MapLoadState.LoadingMap;

			return MapLoadState.Ready;
		}
	}

	[Rpc.Broadcast]
	private void HostReadyRpc()
	{
		_isHostReadyRpc = true;
	}

	private void UpdateHost()
	{
		if (!Networking.IsHost) return;
		if (IsHostReady) return;
		if (!IsLocalMapReady) return;

		IsHostReady = true;
		HostReadyRpc();

		Log.Info("[GameManager] Host map ready");
	}

	private void LogState()
	{
		var state = State;
		if (state == _lastState) return;

		_lastState = state;
		Log.Info($"[GameManager] Load state: {state}");
	}

	private void UpdateSpawn()
	{
		if (_isSpawned) return;
		if (State != MapLoadState.Ready) return;

		_isSpawned = true;

		if (GameNetwork.IsDedicated) return; // у dedicated сервера своего игрока нет
		if (!PlayerPrefab.IsValid()) return;

		// свой игрок уже есть, например сцена пересобрана после смены хоста
		if (Scene.GetAllComponents<Player>().Any(x => !x.IsProxy)) return;

		var player = PlayerPrefab.Clone(GetSpawnTransform());
		player.Name = $"Player - {Connection.Local?.DisplayName}";
		player.NetworkSpawn();

		Log.Info($"[GameManager] Spawn {player.Name}");
	}

	private Transform GetSpawnTransform()
	{
		var spawns = Scene.GetAllComponents<SpawnPoint>().ToList();
		if (spawns.Count == 0) return WorldTransform;

		return Game.Random.FromList(spawns).WorldTransform;
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
		UpdateHost();
		LogState();
		UpdateSpawn();
	}
}
