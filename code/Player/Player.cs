using Sandbox;
using Sandbox.Services;

public sealed class Player : Component
{
	public static Player Instance { get; private set; }

	public PlayerStateEnum State { get; private set; } = PlayerStateEnum.Starting;

	[Property] public SauceController sauceController;

	private GameObject _checkpoint;
	private Vector3 _startPos = Vector3.Zero;
	private Vector2 _startAng = Vector2.Zero;
	private Vector3 _checkpointPos = Vector3.Zero;
	private Vector2 _checkpointAng = Vector2.Zero;

	private TimeUntil _timeWalkthrough = 0f;
	private float _finalTime = 0f;

	private string _mapName = ""; // for achievement, hardcode

	public float GetTime()
	{
		if ( State == PlayerStateEnum.Finished ) return _finalTime;
		if ( State == PlayerStateEnum.Starting ) return 0f;

		return _timeWalkthrough * -1;
	}

	protected override void OnAwake()
	{
		if (!Instance.IsValid())
			Instance = this;
	}

	protected override void OnStart()
	{
		PrepareAchievements();
		PrepareControllers();
		PrepareLookOnStart();
		PrepareSpawns();
	}

	private void PrepareAchievements()
	{
		var map = Scene.Directory.FindByName("Map");
		if ( map is null ) return;
		if ( map.Count() == 0 ) return;
		
		var gameobj = map.First();

		var instance = gameobj.Components.Get<MapInstance>();
		if ( !instance.IsValid() ) return;

		_mapName = instance.MapName;

		Log.Info( $"🌋 Map Instance: {_mapName}" );
	}

	private void PrepareLookOnStart()
	{
		var rot = WorldRotation;

		Rotate( new Vector2( rot.Pitch(), rot.Yaw() ) );
	}

	protected override void OnFixedUpdate()
	{
		CheckResetButton();
		CheckRespawnButton();
		CheckMainMenuButton();
		CheckChangeStateToWalkthrough();
	}

	private void PrepareControllers()
	{
		if ( !sauceController.IsValid() )
			sauceController = Components.Get<SauceController>();
	}


	private void CheckMainMenuButton()
	{
		if ( !Input.Pressed( "Score" ) ) return;

		Scene.LoadFromFile( "scenes/start.scene" );
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
		_startAng = sauceController.LookAngle;
	}

	private void CheckChangeStateToWalkthrough()
	{
		if ( State == PlayerStateEnum.Walkthrough || State == PlayerStateEnum.Finished ) return;

		if ( Input.Pressed( "Forward" ) || Input.Pressed( "Jump" ) || Input.Pressed( "Left" ) || Input.Pressed( "Right" ) || Input.Pressed( "Backward" ) )
		{
			State = PlayerStateEnum.Walkthrough;
			_timeWalkthrough = 0f;
		}
	}

	public void Teleport( Transform transform, bool resetVelocity = true )
	{
		if ( !sauceController.IsValid() ) return;

		if (resetVelocity) sauceController.Velocity = 0f;
		sauceController.CollisionBox.Enabled = false; // fix bag with touch the other colliders

		WorldPosition = transform.Position;
		Rotate( new Vector2( transform.Rotation.Pitch(), transform.Rotation.Yaw() ) );

		sauceController.CollisionBox.Enabled = true;
	}

	public void Respawn()
	{
		if ( !sauceController.IsValid() ) return;

		sauceController.Velocity = 0f;
		sauceController.CollisionBox.Enabled = false; // fix bag with touch the other colliders

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

		sauceController.CollisionBox.Enabled = true;
	}

	public void FinishProgress()
	{
		if ( State == PlayerStateEnum.Finished ) return;

		_finalTime = GetTime();

		ResetProgress();

		State = PlayerStateEnum.Finished;

		// hardcode achivements
		Achievements.Unlock( "the_final" );

		switch ( _mapName )
		{
			case "bafkb.bhopaqua":
				Achievements.Unlock( "win_aqueous" );
				break;

			case "obc.bhop_swooloe":
				Achievements.Unlock( "win_swooloe" );
				break;

			case "starblue.bhop_nuke":
				Achievements.Unlock( "win_nuke" );
				break;

			default:
				break;
		}
		// todo: clean

		Log.Info( "Finish" );
	}

	public void ResetProgress()
	{
		RemoveCheckpoint();

		State = PlayerStateEnum.Starting;
		_timeWalkthrough = 0f;
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
		_checkpointAng = sauceController.LookAngle;

		Stats.Increment( "checkpoints", 1 );
		Achievements.Unlock( "first_checkpoint" );

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
		sauceController.LookAngle = ang;
	}
}

public enum PlayerStateEnum
{
	Starting = 0,
	Walkthrough = 1,
	Finished = 2,
}
