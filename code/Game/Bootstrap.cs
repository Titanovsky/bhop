using System.Threading.Tasks;

/// <summary>
/// Стартовая сцена: хост сразу отправляет всех на случайную дефолтную карту.
/// </summary>
public sealed class Bootstrap : Component
{
	protected override void OnStart()
	{
		GameNetwork.CreateLobbyIfNeeded();

		_ = LoadFirstMap();
	}

	private async Task LoadFirstMap()
	{
		// ждём лобби из редактора, с модалкой сеть уже активна
		for (int i = 0; i < 50 && !Networking.IsActive; i++)
			await Task.DelayRealtimeSeconds(0.1f);

		if (!this.IsValid()) return;
		if (!Networking.IsHost) return; // клиент получит сцену от хоста

		MapList.ChangeMap(MapList.GetRandomIndex());
	}
}
