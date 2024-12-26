using Sandbox;

public sealed class Player : Component
{
	[Property] private SauceController _sauceController;
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
		CheckRespawnButton();
	}

	private void PrepareControllers()
	{
		if ( !_sauceController.IsValid() )
			_sauceController = Components.Get<SauceController>();
	}

	private void CheckResetButton()
	{
		if ( !Input.Pressed( "Reload" ) ) return;

		ResetProgress();
		Respawn();
	}

	private void CheckRespawnButton()
	{
		if ( !Input.Pressed( "Respawn" ) ) return;

		Respawn();
	}

	private void PrepareSpawns()
	{
		_startPos = WorldPosition;
		_startAng = _sauceController.LookAngle;
	}

	public void Respawn()
	{
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
