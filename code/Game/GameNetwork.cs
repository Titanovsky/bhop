using System;
using Sandbox.Network;

public static class GameNetwork
{
	public const int MaxPlayers = 16;
	public const string MenuScene = "scenes/start.scene";

	/// <summary>
	/// Хост без игрока - только настоящий dedicated сервер.
	/// Headless (запуск без окна) сюда не входит: это обычный компьютер игрока (host-client),
	/// он идёт в главное меню, спавнит себе игрока и сам лобби не создаёт.
	/// </summary>
	public static bool IsDedicated => Application.IsDedicatedServer;

	/// <summary>
	/// Мы лидер пати с друзьями: они ждут нашу игру, поэтому сразу создаём лобби.
	/// Create-game modal сюда не относится - платформа показывает её при каждом запуске мультиплеерной игры
	/// и всегда пишет LaunchArguments.MaxPlayers, так что обычный игрок по ней не отличим и идёт в меню.
	/// </summary>
	public static bool IsPartyLeader => PartyRoom.Current is { } party && party.Owner.IsMe && party.MemberCount > 1;

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
	/// Одиночная игра без сети: рвём лобби или подключение, если они есть, и грузим карту локально.
	/// </summary>
	public static void PlaySolo(string scenePath)
	{
		if (Networking.IsActive || Networking.IsConnecting)
			Networking.Disconnect();

		Game.ActiveScene.LoadFromFile(scenePath);

		Log.Info($"[GameNetwork] Play solo: {scenePath}");
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
