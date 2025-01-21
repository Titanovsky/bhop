using Sandbox;

public sealed class TriggerVelocity : Component, Component.ITriggerListener
{
	[Property] public bool up = false;

	public void OnTriggerEnter( Collider other )
	{
		Player ply = other.GetComponent<Player>();
		if ( !ply.IsValid() ) return;

		ply.sauceController.Punch( new Vector3( 0f, 1f, 1f * 500f ) );
	}
}
