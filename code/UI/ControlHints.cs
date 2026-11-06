using Sandbox;

/// <summary>
/// Подсказки по управлению забегом: одни и те же тексты для Tab и HUD.
/// Клавиши берутся из настроек ввода, поэтому подсказка верна и после смены бинда.
/// </summary>
public static class ControlHints
{
	public const string RestartText = "Restart run: back to the start, timer resets";
	public const string CheckpointText = "Back to last checkpoint: timer keeps running";

	public const string RestartShortText = "Restart";
	public const string CheckpointShortText = "Checkpoint";

	public static string RestartKey => GetKey("Reload", "R");
	public static string CheckpointKey => GetKey("Respawn", "X");

	private static string GetKey(string action, string fallback)
	{
		var key = Input.GetButtonOrigin(action);

		return string.IsNullOrEmpty(key) ? fallback : key.ToUpperInvariant();
	}
}
