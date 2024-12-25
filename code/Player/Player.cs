using Sandbox;

public sealed class Player : Component
{
	[Property] private SauceController _sauceController;
	[Property] private GameObject _map;
	private List<Vector3> _spawns = new();

	protected override void OnStart()
	{
		PrepareSpawns();
		PrepareControllers();
	}

	protected override void OnFixedUpdate()
	{
		CheckRespawnButton();
	}

	private void PrepareControllers()
	{
		if ( !_sauceController.IsValid() )
			_sauceController = Components.Get<SauceController>();
	}

	private void CheckRespawnButton()
	{
		if ( !Input.Pressed( "Reload" ) ) return;

		Respawn();
	}

	private void PrepareSpawns()
	{
		if (!_map.IsValid())
		{
			Log.Error( "Where map/scene map?" );

			return;
		}

		foreach ( var gameobj in _map.Children )
		{
			if ( !gameobj.Components.Get<SpawnPoint>().IsValid() ) continue;

			_spawns.Add( gameobj.WorldPosition );

			Log.Info( $"new spawn point {gameobj}" );
		}
	}

	public void Respawn()
	{
		if ( _spawns.Count == 0 ) return;

		var pos = _spawns[Game.Random.Next( _spawns.Count )];

		WorldPosition = pos;

		_sauceController.Velocity = 0f;
	}
}
