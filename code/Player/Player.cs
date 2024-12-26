using Sandbox;

public sealed class Player : Component
{
	[Property] private SauceController _sauceController;
	//[Property] private GameObject _map;
	private List<Vector3> _spawns = new();

	private GameObject _checkpoint;
	private Vector3 _startPos = Vector3.Zero;
	private Vector2 _startAng = Vector2.Zero;
	private Vector3 _checkpointPos = Vector3.Zero;
	private Vector2 _checkpointAng = Vector2.Zero;

	protected override void OnStart()
	{
		PrepareControllers();
		PrepareSpawns();
	}

	protected override void OnFixedUpdate()
	{
		CheckResetButton();

		//Log.Info( _body.WorldRotation.Angles() );
	}

	private void PrepareControllers()
	{
		if ( !_sauceController.IsValid() )
			_sauceController = Components.Get<SauceController>();

		//if ( !_body.IsValid() )
			//_body = GameObject.Children.First();
	}

	private void CheckResetButton()
	{
		if ( !Input.Pressed( "Reload" ) ) return;

		ResetProgress();
		Respawn();
	}

	private void PrepareSpawns()
	{
		_startPos = WorldPosition;
		_startAng = _sauceController.LookAngle;

		//if (!_map.IsValid())
		//{
		//	Log.Error( "Where map/scene map?" );

		//	return;
		//}

		//foreach ( var gameobj in _map.Children )
		//{
		//	if ( !gameobj.Components.Get<SpawnPoint>().IsValid() ) continue;

		//	_spawns.Add( gameobj.WorldPosition );

		//	Log.Info( $"new spawn point {gameobj}" );
		//}
	}

	public void Respawn()
	{
		//if ( _spawns.Count == 0 ) return;

		//var pos = _spawns[Game.Random.Next( _spawns.Count )];

		_sauceController.Velocity = 0f;

		if ( _checkpoint.IsValid() )
		{
			WorldPosition = _checkpointPos;
			Rotate( _checkpointAng );
		}
		else
		{
			WorldPosition = _startPos;
			Rotate(_startAng);
		}

	}

	public void FinishProgress()
	{
		ResetProgress();

		Log.Info( "Finish" );
	}

	public void ResetProgress()
	{
		RemoveCheckpoint();
	}

	public void RemoveCheckpoint()
	{
		_checkpoint = null;
	}

	public void SetupCheckpoint(GameObject gameObj )
	{
		if ( !gameObj.IsValid() ) return;
		if ( _checkpoint.IsValid() && _checkpoint == gameObj ) return;

		var pos = GetSpawnPos( gameObj );

		_checkpoint = gameObj;
		_checkpointPos = pos;
		_checkpointAng = _sauceController.LookAngle;

		Log.Info( $"🎄 Checkpoint: {gameObj}" );
	}

	public void SetupCheckpoint( Collider collider )
	{
		SetupCheckpoint( collider.GameObject );
	}

	private Vector3 GetSpawnPos(GameObject gameObj)
	{
		return gameObj.GetBounds().Center;
	}

	private void Rotate(Vector2 ang)
	{
		_sauceController.LookAngle = ang;
	}
}
