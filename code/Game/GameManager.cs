using System.Threading.Tasks;

/// <summary>
/// Лежит на каждой карте. Каждый клиент сам спавнит своего игрока и становится его владельцем.
/// </summary>
public sealed class GameManager : Component
{
	[Property] public GameObject PlayerPrefab { get; set; }

	protected override void OnStart()
	{
		GameNetwork.CreateLobbyIfNeeded(); // карта запущена прямо из редактора

		_ = SpawnLocalPlayer();
	}

	private async Task SpawnLocalPlayer()
	{
		for (int i = 0; i < 50 && !Networking.IsActive; i++)
			await Task.DelayRealtimeSeconds(0.1f);

		if (!this.IsValid()) return;
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
}
