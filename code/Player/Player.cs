﻿using Sandbox.Services;

public sealed class Player : Component
{
	public static Player Instance { get; private set; }

	public PlayerStateEnum State { get; private set; } = PlayerStateEnum.Starting;

	[Property] public SauceController sauceController;

	private Vector3 _startPos = Vector3.Zero;
	private Vector2 _startAng = Vector2.Zero;

	public Segment segmentPrevious;
	public Segment segment;
	private Vector3 _segmentPos = Vector3.Zero;
	private Vector2 _segmentAng = Vector2.Zero;

	private TimeUntil _timeWalkthrough = 0f;
	private float _finalTime = 0f;

	public string mapName = ""; // for achievement, hardcode

	public void Teleport( Transform transform, bool resetVelocity = true )
	{
		if ( !sauceController.IsValid() ) return;

		if ( resetVelocity ) sauceController.Velocity = 0f;
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

		var pos = segment.IsValid() ? _segmentPos : _startPos;
		var ang = segment.IsValid() ? _segmentAng : _startAng;

		WorldPosition = pos;
		Rotate( ang );

		sauceController.CollisionBox.Enabled = true;
	}

	public void FinishProgress()
	{
		if ( State == PlayerStateEnum.Finished ) return;

		_finalTime = GetTime();

		ResetProgress();

		State = PlayerStateEnum.Finished;

		if ( CanCompleteStats() )
		{
			// hardcode achivements
			Achievements.Unlock( "the_final" );

			switch ( mapName )
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

				case "gear.bhop_rally":
					Achievements.Unlock( "win_rally" );
					break;

				case "gear.bhop_atom":
					Achievements.Unlock( "win_atom" );
					break;

				case "gear.bhop_colorshit":
					Achievements.Unlock( "win_colorshit" );
					break;

				default:
					break;
			}
		}

		Log.Info( "Finish" );
	}

	public void ResetProgress()
	{
		if ( !segment.IsValid() ) return;

		segment.IsNow = false;

		segment = SegmentHandler.Instance.Segments.First();
		_segmentPos = _startPos; // workaround
		_segmentAng = _startAng; // workaround

		State = PlayerStateEnum.Starting;
		_timeWalkthrough = 0f;

		foreach ( Segment seg in SegmentHandler.Instance.Segments )
		{
			if ( !seg.IsDone() ) continue;

			seg.TimeDone = 0f;
		}

		segmentPrevious = null;
	}

	public bool CheckConfig()
	{
		if ( sauceController.MaxSpeed != GameConfig.MaxSpeed ) return false;
		else if ( sauceController.MoveSpeed != GameConfig.MoveSpeed ) return false;
		else if ( sauceController.CrouchSpeed != GameConfig.CrouchSpeed ) return false;
		else if ( sauceController.StopSpeed != GameConfig.StopSpeed ) return false;
		else if ( sauceController.Friction != GameConfig.Friction ) return false;
		else if ( sauceController.Acceleration != GameConfig.Acceleration ) return false;
		else if ( sauceController.AirAcceleration != GameConfig.AirAcceleration ) return false;
		else if ( sauceController.MaxAirWishSpeed != GameConfig.MaxAirWishSpeed ) return false;
		else if ( sauceController.JumpForce.CeilToInt() != GameConfig.JumpForce.CeilToInt() ) return false;

		return true;
	}

	public bool CanCompleteStats()
	{
		if ( !CheckConfig() ) return false;
		else if ( Game.IsEditor ) return false;

		return true;
	}

	public void SetupSegment( Collider collider )
	{
		SetupSegment( collider.GetComponent<TriggerSegment>() );
	}

	public void SetupSegment( TriggerSegment trigger )
	{
		if ( segment.IsValid() && segment == trigger.Segment || segment.Id > trigger.Segment.Id ) return;

		if ( segment.IsValid() )
		{
			segment.Finish();

			segmentPrevious = segment;

			if ( segmentPrevious.IsBest() )
				segmentPrevious.TimeDonePrevious = segmentPrevious.TimeDone;
		}

		trigger.Segment.Start();

		segment = trigger.Segment;
		_segmentPos = GetSpawnPos( trigger.GameObject );
		_segmentAng = sauceController.LookAngle;

		SegmentLoader.Save();

		if ( CanCompleteStats() )
		{
			Stats.Increment( "checkpoints", 1 );
			Achievements.Unlock( "first_checkpoint" );
		}

		Log.Info( $"[Player] Take {trigger.GameObject}" );
	}

	public float GetTime()
	{
		if ( State == PlayerStateEnum.Finished ) return _finalTime;
		if ( State == PlayerStateEnum.Starting ) return 0f;

		return _timeWalkthrough * -1;
	}

	private void PrepareAchievements()
	{
		var map = Scene.Directory.FindByName("Map");
		if ( map is null ) return;
		if ( map.Count() == 0 ) return;
		
		var gameobj = map.First();

		var instance = gameobj.Components.Get<MapInstance>();
		if ( !instance.IsValid() ) return;

		mapName = instance.MapName;

		// disable light because march 2025 update (new bloom and postproccesing)
		GameObject lightEnvironment = instance.GameObject.Children.Where((gameObj) => gameObj.Name == "light_environment").First();
		lightEnvironment.Enabled = false;

		Log.Info( $"[Player] Map Instance: {mapName}" );
	}

	private void PrepareLookOnStart()
	{
		var rot = WorldRotation;

		Rotate( new Vector2( rot.Pitch(), rot.Yaw() ) );
	}

	private void PrepareControllers()
	{
		if ( !sauceController.IsValid() )
			sauceController = Components.Get<SauceController>();

		segment = SegmentHandler.Instance.Segments.First();

		_segmentPos = WorldPosition; // workaround
		_segmentAng = sauceController.LookAngle; // workaround
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

			segment.Start();
		}
	}

	private Vector3 GetSpawnPos(GameObject gameObj)
	{
		return gameObj.GetBounds().Center;
	}

	private void Rotate(Vector2 ang)
	{
		sauceController.LookAngle = ang;
	}

	protected override void OnAwake()
	{
		if ( !Instance.IsValid() )
			Instance = this;
	}

	protected override void OnStart()
	{
		PrepareAchievements();
		PrepareControllers();
		PrepareLookOnStart();
		PrepareSpawns();
	}

	protected override void OnFixedUpdate()
	{
		CheckResetButton();
		CheckRespawnButton();
		CheckMainMenuButton();
		CheckChangeStateToWalkthrough();
	}
}

public enum PlayerStateEnum
{
	Starting = 0,
	Walkthrough = 1,
	Finished = 2,
}
