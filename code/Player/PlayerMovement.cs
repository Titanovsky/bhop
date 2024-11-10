using Sandbox;

public sealed class PlayerMovement : Component
{
	[Property] private CharacterController _char;
	[Property] private float _speed = 5f;

	private Vector3 _wishVelocity = Vector3.Zero;

	private void CheckInputs()
	{
		Rotation rot = _char.WorldRotation;

		if ( Input.Down( "Forward" ) )
			_wishVelocity += rot.Forward * _speed;

		if ( Input.Down( "Backward" ) )
			_wishVelocity += rot.Backward * _speed;

		if ( Input.Down( "Left" ) )
			_wishVelocity += rot.Left * _speed;

		if ( Input.Down( "Right" ) )
			_wishVelocity += rot.Right * _speed;

		if ( Input.Down( "Jump" ) ) // //
			Jump();
	}

	public void Jump()
	{
		if ( !_char.IsOnGround ) return;

		_char.Punch( Vector3.Up * 10f );
	}

	private void Move()
	{
		var gravity = Scene.PhysicsWorld.Gravity;

		_wishVelocity = _wishVelocity.WithZ( 0 );

		if ( _char.IsOnGround )
		{
			_char.Velocity = _char.Velocity.WithZ( 0 );
			_char.Accelerate( _wishVelocity );
			_char.ApplyFriction( .4f );
		}
		else
		{
			_char.Velocity += gravity * Time.Delta * 0.5f;
			_char.Accelerate( _wishVelocity.ClampLength( 50f ) );
			_char.ApplyFriction( .1f );
		}

		_char.Move();

		_wishVelocity = Vector3.Zero;
	}

	protected override void OnStart()
	{
		_char = Components.GetOrCreate<CharacterController>();
	}

	protected override void OnUpdate()
	{
		CheckInputs();
		Move();
	}
}
