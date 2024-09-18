using System;
using Sandbox;

namespace Q3Movement
{
	public class MovementSettings
	{
		public float MaxSpeed { get; set; }
		public float Acceleration { get; set; }
		public float Deceleration { get; set; }

		public MovementSettings( float maxSpeed, float accel, float decel )
		{
			MaxSpeed = maxSpeed;
			Acceleration = accel;
			Deceleration = decel;
		}
	}

	public class Q3PlayerController : Component
	{
		//[Title( "Aiming" )]
		[Property] private CameraComponent m_Camera;
		[Property] private MouseLook m_MouseLook = new MouseLook();

		//[Title( "Movement" )]
		[Property] private float m_Friction = 6;
		[Property] private float m_Gravity = 20;
		[Property] private float m_JumpForce = 8;
		[Description( "Automatically jump when holding jump button" )]
		[Property] private bool m_AutoBunnyHop = false;
		[Description( "How precise air control is" )]
		[Property] private float m_AirControl = 0.3f;
		private MovementSettings m_GroundSettings = new MovementSettings( 7, 14, 10 );
		private MovementSettings m_AirSettings = new MovementSettings( 7, 2, 2 );
		private MovementSettings m_StrafeSettings = new MovementSettings( 1, 50, 50 );

		/// <summary>
		/// Returns player's current speed.
		/// </summary>
		public float Speed { get { return m_Character.Velocity.Length; } }

		private CharacterController m_Character;
		private Vector3 m_MoveDirectionNorm = Vector3.Zero;
		private Vector3 m_PlayerVelocity = Vector3.Zero;

		// Used to queue the next jump just before hitting the ground.
		private bool m_JumpQueued = false;

		// Used to display real time friction values.
		private float m_PlayerFriction = 0;

		private Vector3 m_MoveInput;
		private Transform m_Tran;
		private Transform m_CamTran;

		protected override void OnStart()
		{
			m_Tran = Transform.World;
			m_Character = GameObject.Components.Get<CharacterController>();

			if ( !m_Camera.IsValid )
				m_Camera = Components.GetInParentOrSelf<CameraComponent>();

			m_CamTran = m_Camera.Transform.World;
			m_MouseLook.Init( m_Tran, m_CamTran );
		}

		protected override void OnUpdate()
		{
			m_MoveInput = new Vector3( Input.MouseDelta.x, 0, Input.MouseDelta.y );
			m_MouseLook.UpdateCursorLock();
			QueueJump();

			// Set movement state.
			if ( m_Character.IsOnGround )
			{
				GroundMove();
			}
			else
			{
				AirMove();
			}

			// Rotate the character and camera.
			m_MouseLook.LookRotation( m_Tran, m_CamTran );

			// Move the character.
			m_Character.Velocity = m_PlayerVelocity * Time.Delta;
			m_Character.Move();
		}

		// Queues the next jump.
		private void QueueJump()
		{
			if ( m_AutoBunnyHop )
			{
				m_JumpQueued = Input.Pressed( "Jump" );
				return;
			}

			if ( Input.Down( "Jump" ) && !m_JumpQueued )
			{
				m_JumpQueued = true;
			}

			if ( Input.Pressed( "Jump" ) )
			{
				m_JumpQueued = false;
			}
		}

		// Handle air movement.
		private void AirMove()
		{
			float accel;

			var wishdir = new Vector3( m_MoveInput.x, 0, m_MoveInput.z );
			//wishdir = m_Tran.TransformDirection( wishdir );

			float wishspeed = wishdir.Length;
			wishspeed *= m_AirSettings.MaxSpeed;

			wishdir = wishdir.Normal;
			m_MoveDirectionNorm = wishdir;

			// CPM Air control.
			float wishspeed2 = wishspeed;
			if ( Vector3.Dot( m_PlayerVelocity, wishdir ) < 0 )
			{
				accel = m_AirSettings.Deceleration;
			}
			else
			{
				accel = m_AirSettings.Acceleration;
			}

			// If the player is ONLY strafing left or right
			if ( m_MoveInput.z == 0 && m_MoveInput.x != 0 )
			{
				if ( wishspeed > m_StrafeSettings.MaxSpeed )
				{
					wishspeed = m_StrafeSettings.MaxSpeed;
				}

				accel = m_StrafeSettings.Acceleration;
			}

			Accelerate( wishdir, wishspeed, accel );
			if ( m_AirControl > 0 )
			{
				AirControl( wishdir, wishspeed2 );
			}

			// Apply gravity
			m_PlayerVelocity.y -= m_Gravity * Time.Delta;
		}

		// Air control occurs when the player is in the air, it allows players to move side 
		// to side much faster rather than being 'sluggish' when it comes to cornering.
		private void AirControl( Vector3 targetDir, float targetSpeed )
		{
			// Only control air movement when moving forward or backward.
			if ( Math.Abs( m_MoveInput.z ) < 0.001 || Math.Abs( targetSpeed ) < 0.001 )
			{
				return;
			}

			float zSpeed = m_PlayerVelocity.y;
			m_PlayerVelocity.y = 0;
			/* Next two lines are equivalent to idTech's VectorNormalize() */
			float speed = m_PlayerVelocity.Length;
			m_PlayerVelocity = m_PlayerVelocity.Normal;

			float dot = Vector3.Dot( m_PlayerVelocity, targetDir );
			float k = 32;
			k *= m_AirControl * dot * dot * Time.Delta;

			// Change direction while slowing down.
			if ( dot > 0 )
			{
				m_PlayerVelocity.x *= speed + targetDir.x * k;
				m_PlayerVelocity.y *= speed + targetDir.y * k;
				m_PlayerVelocity.z *= speed + targetDir.z * k;

				m_PlayerVelocity = m_PlayerVelocity.Normal;
				m_MoveDirectionNorm = m_PlayerVelocity;
			}

			m_PlayerVelocity.x *= speed;
			m_PlayerVelocity.y = zSpeed; // Note this line
			m_PlayerVelocity.z *= speed;
		}

		// Handle ground movement.
		private void GroundMove()
		{
			// Do not apply friction if the player is queueing up the next jump
			if ( !m_JumpQueued )
			{
				ApplyFriction( 1.0f );
			}
			else
			{
				ApplyFriction( 0 );
			}

			var wishdir = new Vector3( m_MoveInput.x, 0, m_MoveInput.z );
			//wishdir = m_Tran.TransformDirection( wishdir );

			wishdir = wishdir.Normal;
			m_MoveDirectionNorm = wishdir;

			var wishspeed = wishdir.Length;
			wishspeed *= m_GroundSettings.MaxSpeed;

			Accelerate( wishdir, wishspeed, m_GroundSettings.Acceleration );

			// Reset the gravity velocity
			m_PlayerVelocity.y = -m_Gravity * Time.Delta;

			if ( m_JumpQueued )
			{
				m_PlayerVelocity.y = m_JumpForce;
				m_JumpQueued = false;
			}
		}

		private void ApplyFriction( float t )
		{
			// Equivalent to VectorCopy();
			Vector3 vec = m_PlayerVelocity;
			vec.y = 0;
			float speed = vec.Length;
			float drop = 0;

			// Only apply friction when grounded.
			if ( m_Character.IsOnGround )
			{
				float control = speed < m_GroundSettings.Deceleration ? m_GroundSettings.Deceleration : speed;
				drop = control * m_Friction * Time.Delta * t;
			}

			float newSpeed = speed - drop;
			m_PlayerFriction = newSpeed;
			if ( newSpeed < 0 )
			{
				newSpeed = 0;
			}

			if ( speed > 0 )
			{
				newSpeed /= speed;
			}

			m_PlayerVelocity.x *= newSpeed;
			// playerVelocity.y *= newSpeed;
			m_PlayerVelocity.z *= newSpeed;
		}

		// Calculates acceleration based on desired speed and direction.
		private void Accelerate( Vector3 targetDir, float targetSpeed, float accel )
		{
			float currentspeed = Vector3.Dot( m_PlayerVelocity, targetDir );
			float addspeed = targetSpeed - currentspeed;
			if ( addspeed <= 0 )
			{
				return;
			}

			float accelspeed = accel * Time.Delta * targetSpeed;
			if ( accelspeed > addspeed )
			{
				accelspeed = addspeed;
			}

			m_PlayerVelocity.x += accelspeed * targetDir.x;
			m_PlayerVelocity.z += accelspeed * targetDir.z;
		}
	}
}
