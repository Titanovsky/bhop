using System;

public sealed class MapEntry
{
	public string Name { get; init; }
	public string ScenePath { get; init; }

	public string GetIcon() => $"icons/{Name.ToLower()}.vtex";
}

/// <summary>
/// Дефолтные карты игры: для первой загрузки, голосования и смены карты хостом.
/// </summary>
public static class MapList
{
	public static readonly IReadOnlyList<MapEntry> All = new List<MapEntry>
	{
		new MapEntry { Name = "Atom", ScenePath = "scenes/map.bhop_atom.scene" },
		new MapEntry { Name = "Colorshit", ScenePath = "scenes/map.bhop_colorshit.scene" },
		new MapEntry { Name = "Nuke", ScenePath = "scenes/map.bhop_nuke.scene" },
		new MapEntry { Name = "Rally", ScenePath = "scenes/map.bhop_rally.scene" },
		new MapEntry { Name = "Swooloe", ScenePath = "scenes/map.bhop_swooloe.scene" },
		new MapEntry { Name = "Paper", ScenePath = "scenes/map.bhop_paper.scene" },
	};

	public static MapEntry Get(int index) => (index >= 0 && index < All.Count) ? All[index] : null;

	public static int IndexOfScene(string scenePath)
	{
		if (string.IsNullOrEmpty(scenePath)) return -1;

		for (int i = 0; i < All.Count; i++)
		{
			if (string.Equals(All[i].ScenePath, scenePath, StringComparison.OrdinalIgnoreCase))
				return i;
		}

		return -1;
	}

	public static int GetRandomIndex() => Game.Random.Next(All.Count);

	/// <summary>
	/// Смена карты для всех. Работает только на хосте, клиенты переходят на новую сцену вслед за ним.
	/// </summary>
	public static void ChangeMap(int index)
	{
		if (!Networking.IsHost) return;

		var map = Get(index);
		if (map is null) return;

		var options = new SceneLoadOptions();
		options.SetScene(map.ScenePath);

		Game.ChangeScene(options);

		Log.Info($"[MapList] Change map: {map.Name}");
	}
}
