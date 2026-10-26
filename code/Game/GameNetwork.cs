using Sandbox.Network;

public static class GameNetwork
{
	public const int MaxPlayers = 16;

	/// <summary>
	/// Обычно лобби создаёт create-game modal платформы (название, приватность, слоты).
	/// Это запасной путь для запуска из редактора, настройки совпадают с ProjectSettings/Networking.config.
	/// </summary>
	public static void CreateLobbyIfNeeded()
	{
		if (Networking.IsActive || Networking.IsConnecting) return;

		Networking.CreateLobby(new LobbyConfig
		{
			Name = "Bhop",
			MaxPlayers = MaxPlayers,
			Privacy = LobbyPrivacy.Public,
			DestroyWhenHostLeaves = false,
		});

		Log.Info("[GameNetwork] Create lobby");
	}
}
