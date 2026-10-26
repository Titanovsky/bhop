using Sandbox;

/// <summary>
/// Escape - меню паузы, зажатый Tab - scoreboard. Пока на экране курсор, взгляд игрока не крутится.
/// </summary>
public sealed class MenuInput : Component
{
	[Property] public PauseMenu PauseMenu { get; set; }
	[Property] public Scoreboard Scoreboard { get; set; }
	[Property] public MapVotePanel MapVotePanel { get; set; }

	public static bool IsCursorVisible { get; private set; } = false;

	protected override void OnUpdate()
	{
		if (PauseMenu.IsValid() && Input.EscapePressed)
		{
			TogglePauseMenu();

			Input.EscapePressed = false;
		}

		if (Scoreboard.IsValid())
		{
			var isOpen = Input.Down("Score");
			if (Scoreboard.Enabled != isOpen) Scoreboard.Enabled = isOpen;
		}

		IsCursorVisible = (PauseMenu.IsValid() && PauseMenu.Enabled)
			|| (Scoreboard.IsValid() && Scoreboard.Enabled)
			|| (MapVotePanel.IsValid() && MapVotePanel.WantsCursor);
	}

	protected override void OnDestroy()
	{
		IsCursorVisible = false;
	}

	public void TogglePauseMenu()
	{
		PauseMenu.Enabled = !PauseMenu.Enabled;
	}
}
