using Sandbox;

public sealed class PlayerLook : Component
{
	[Property] private float _minAngleX = -90f; // for realistic look around
	[Property] private float _maxAngleX = 90f;
	[Property] private float _sens = 10f;

	private float _mouseX;
	private float _mouseY;
	private float _rotationX = 0f;
	private float _rotationY = 90f; // ÷òîáû ñðàçó ñìîòðåë íà äâåðü

	[Property] public CameraComponent camera;
	//
	private void Look()
	{
		_mouseX = Input.MouseDelta.x * _sens * Time.Delta;
		_mouseY = Input.MouseDelta.y * _sens * Time.Delta;

		_rotationX += _mouseY;
		_rotationX = MathX.Clamp( _rotationX, _minAngleX, _maxAngleX );

		_rotationY -= _mouseX;

		if ( camera != null )
		{
			//camera.transform.rotation = Quaternion.Euler( _rotationX, _rotationY, 0 );
			camera.WorldRotation = new Angles( _rotationX, _rotationY, 0 ).ToRotation();
		}
	}

	private void PrepareLook()
	{
		//camera = Components.GetOrCreate<CameraComponent>();
	}

	protected override void OnStart()
	{
		PrepareLook();
	}

	protected override void OnUpdate()
	{
		Look();
	}
}
