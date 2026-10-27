using System.Threading.Tasks;

/// <summary>
/// Стартовая сцена с главным меню. Сам решает, что делать при запуске игры:
/// dedicated сервер и запуск с готовым лобби (модалка платформы, пати) сразу уходят на первую карту,
/// во всех остальных случаях игрок остаётся в меню.
/// </summary>
public sealed class Bootstrap : Component
{
	/// <summary>
	/// Запуск уже обработан: при возврате в меню из лобби повторно на карту не отправляем.
	/// </summary>
	private static bool _isLaunchHandled = false;

	/// <summary>
	/// Что сейчас происходит вместо меню, null - показываем меню.
	/// </summary>
	public static string Status { get; set; } = null;

	protected override void OnStart()
	{
		Status = null;

		if (GameNetwork.IsDedicated)
		{
			_ = HostFirstMap(GameNetwork.CreateLobbyFromLaunch);
			return;
		}

		if (_isLaunchHandled) return;
		_isLaunchHandled = true;

		if (Networking.IsActive || Networking.IsConnecting) return; // подключаемся к чужой игре, сцену пришлёт хост

		if (GameNetwork.IsPartyMember)
		{
			Status = "Waiting for party leader…";
			return;
		}

		if (GameNetwork.IsLobbyLaunch)
			_ = HostFirstMap(GameNetwork.CreateLobbyFromLaunch);
	}

	/// <summary>
	/// Создать лобби (если ещё нет) и отправить всех на первую карту.
	/// </summary>
	public static async Task HostFirstMap(System.Action createLobby)
	{
		Status = "Creating lobby…";

		if (!Networking.IsActive)
			createLobby();

		for (int i = 0; i < 150 && !Networking.IsActive; i++)
			await GameTask.DelayRealtimeSeconds(0.1f);

		if (!Networking.IsActive || !Networking.IsHost)
		{
			Log.Warning("[Bootstrap] Lobby was not created");
			Status = null;
			return;
		}

		Status = "Loading map…";
		MapList.ChangeMap(MapList.FirstMapIndex);
	}
}
