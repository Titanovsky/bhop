using System;
using Sandbox.Network;

public static class GameNetwork
{
	public const int MaxPlayers = 16;
	public const string MenuScene = "scenes/start.scene";

	/// <summary>
	/// Хост без игрока: dedicated сервер или запуск без окна.
	/// </summary>
	public static bool IsDedicated => Application.IsDedicatedServer || Application.IsHeadless;

	/// <summary>
	/// Игру запустили с готовым лобби: из create-game modal платформы (больше одного слота) или из пати.
	/// </summary>
	public static bool IsLobbyLaunch => PartyRoom.Current is not null || LaunchArguments.MaxPlayers > 1;

	/// <summary>
	/// В пати, но лидер не мы - движок сам подключит нас к его игре.
	/// </summary>
	public static bool IsPartyMember => PartyRoom.Current is { } party && !party.Owner.IsMe;

	public static string GetDefaultLobbyName()
	{
		var name = Connection.Local?.DisplayName;

		return string.IsNullOrEmpty(name) ? "Bhop" : $"{name}'s Bhop";
	}

	public static void CreateLobby(string name, LobbyPrivacy privacy, int maxPlayers)
	{
		if (Networking.IsActive || Networking.IsConnecting) return;

		Networking.CreateLobby(new LobbyConfig
		{
			Name = string.IsNullOrWhiteSpace(name) ? GetDefaultLobbyName() : name,
			MaxPlayers = Math.Clamp(maxPlayers, 1, MaxPlayers),
			Privacy = privacy,
			DestroyWhenHostLeaves = false,
		});

		Log.Info($"[GameNetwork] Create lobby {name} ({privacy}, {maxPlayers})");
	}

	/// <summary>
	/// Лобби с настройками, выбранными в create-game modal платформы.
	/// </summary>
	public static void CreateLobbyFromLaunch()
	{
		var maxPlayers = (LaunchArguments.MaxPlayers > 0) ? LaunchArguments.MaxPlayers : MaxPlayers;

		CreateLobby(LaunchArguments.ServerName, LaunchArguments.Privacy, maxPlayers);
	}

	/// <summary>
	/// Выйти из лобби (если оно есть) в главное меню.
	/// </summary>
	public static void LeaveToMenu()
	{
		if (Networking.IsActive || Networking.IsConnecting)
			Networking.Disconnect();

		Game.ActiveScene.LoadFromFile(MenuScene);
	}
}
