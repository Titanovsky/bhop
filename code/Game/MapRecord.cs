using Sandbox;

/// <summary>
/// Лучший забег (PB) игрока на карте: итоговое время и сплиты по id чекпоинтов.
/// Хранится локально у игрока и перезаписывается только при новом рекорде.
/// </summary>
public sealed class MapRecord
{
	public float Time { get; set; } = 0f;
	public Dictionary<int, float> Splits { get; set; } = new();

	private static string GetPath(string mapName) => $"{mapName}.record.json";

	public static MapRecord Load(string mapName)
	{
		var record = FileSystem.Data.ReadJson<MapRecord>( GetPath(mapName) );
		if ( record is null || record.Time <= 0f ) return null;

		record.Splits ??= new();

		Log.Info( $"[MapRecord] Load {mapName}: {record.Time}" );

		return record;
	}

	public void Save(string mapName)
	{
		FileSystem.Data.WriteJson( GetPath(mapName), this );

		Log.Info( $"[MapRecord] Save {mapName}: {Time}" );
	}
}
