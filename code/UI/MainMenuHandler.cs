using Sandbox;

public sealed class MainMenuHandler : Component
{
	[Property] public PanelComponent MainMenu { get; set; }

	private TimeUntil _delay = 0.25f;

	protected override void OnFixedUpdate()
	{
		if (!MainMenu.IsValid()) return;

		if (Input.Pressed("Score"))
			ToggleMainMenu();
    }

	public void ToggleMainMenu()
	{
		if (!_delay) return;

		_delay = 0.25f;

		MainMenu.Enabled = !MainMenu.Enabled;
    }
}
