using Sandbox;

public sealed class MainMenuHandler : Component
{
	[Property] public PanelComponent MainMenu { get; set; }

	protected override void OnUpdate()
	{
		if (!MainMenu.IsValid()) return;

		if (Input.Pressed("Score") || Input.EscapePressed)
		{
			ToggleMainMenu();

            Input.EscapePressed = false;
        }
    }

	public void ToggleMainMenu()
	{
		MainMenu.Enabled = !MainMenu.Enabled;
    }
}
